/* Copyright (c) 2023 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DumpReflection.Reflection;
using Gibbed.AddressLibrary;
using Gibbed.IO;
using NDesk.Options;
using Newtonsoft.Json;
using StarfieldDumping;
using TypeKind = DumpReflection.Natives.TypeKind;

namespace DumpReflection
{
    internal static class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            Environment.ExitCode = MainInternal(args);
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                Console.ReadLine();
            }
        }

        private static int MainInternal(string[] args)
        {
            bool showHelp = false;

            OptionSet options = new()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;
            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return -1;
            }

            if (extras.Count < 0 || extras.Count > 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ [output_json]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return -2;
            }

            var outputPath = extras.Count > 0
                ? extras[0]
                : "BSReflection.json";

            return -3 + DumpingHelpers.Main(outputPath, Dump);
        }

        private static int Dump(RuntimeProcess runtime, AddressLibrary addressLibrary, string outputPath)
        {
            var mainModuleBaseAddressValue = runtime.Process.MainModule.BaseAddress.ToInt64();
            IntPtr Id2Pointer(ulong id)
            {
                var offset = (long)addressLibrary[id];
                return new(mainModuleBaseAddressValue + offset);
            }

            BasicType.ExpectedVftablePointer = Id2Pointer(294379);
            ClassType.ExpectedVftablePointer = Id2Pointer(285167);
            EnumType.ExpectedVftablePointer = Id2Pointer(292531);

            var baseTypeKindOffset = Marshal.OffsetOf<Natives.BaseType>(nameof(Natives.BaseType.Kind)).ToInt32();
            var classNextOffset = Marshal.OffsetOf<Natives.ClassType>(nameof(Natives.ClassType.Next)).ToInt32();
            var enumNextOffset = Marshal.OffsetOf<Natives.EnumType>(nameof(Natives.EnumType.Next)).ToInt32();

            Queue<IntPtr> queue = new();

            // basic types
            var basicTypeTablePointer = Id2Pointer(885824);
            var basicTypeTableEntryPointer = basicTypeTablePointer;
            for (int i = 0; i < 11; i++, basicTypeTableEntryPointer += 8)
            {
                var typePointer = runtime.ReadPointer(basicTypeTableEntryPointer);
                queue.Enqueue(typePointer);
            }

            // class types
            foreach (var typePointer in ReadTypeList(runtime, Id2Pointer(885835), classNextOffset))
            {
                queue.Enqueue(typePointer);
            }

            // enumeration types
            foreach (var typePointer in ReadTypeList(runtime, Id2Pointer(885839), enumNextOffset))
            {
                queue.Enqueue(typePointer);
            }

            Dictionary<IntPtr, IType> typeMap = new();

            while (queue.Count > 0)
            {
                var nativePointer = queue.Dequeue();

                if (nativePointer == IntPtr.Zero)
                {
                    continue;
                }

                if (typeMap.ContainsKey(nativePointer) == true)
                {
                    // already processed
                    continue;
                }

                var nativeTypeKind = (TypeKind)runtime.ReadValueU8(nativePointer + baseTypeKindOffset);

                IType instance = nativeTypeKind switch
                {
                    TypeKind.Basic => new BasicType(),
                    TypeKind.String => new StringType(),
                    TypeKind.Enumeration => new EnumType(),
                    TypeKind.Class => new ClassType(),
                    TypeKind.List => new ListType(),
                    TypeKind.Set => new SetType(),
                    TypeKind.Map => new MapType(),
                    TypeKind.UniquePointer => new UniquePointerType(),
                    TypeKind.SharedPointer => new SharedPointerType(),
                    TypeKind.BorrowedPointer => new BorrowedPointerType(),
                    _ => throw new NotSupportedException(),
                };

                if (instance == null)
                {
                    continue;
                }

                instance.Read(runtime, nativePointer);

                typeMap[nativePointer] = instance;

                if (instance is ClassType classType)
                {
                    foreach (var property in classType.Properties)
                    {
                        queue.Enqueue(property.TypePointer);
                    }

                    foreach (var cast in classType.Downcasts)
                    {
                        queue.Enqueue(cast.TypePointer);
                    }
                }
                else if (instance is ReferenceType referenceType)
                {
                    queue.Enqueue(referenceType.UnderlyingTypePointer);
                }
                else if (instance is CollectionType collectionType)
                {
                    queue.Enqueue(collectionType.ItemTypePointer);
                }
                else if (instance is MapType mapType)
                {
                    queue.Enqueue(mapType.ItemTypePointer);
                }
            }

            foreach (var instance in typeMap.Values)
            {
                instance.Resolve(typeMap);
            }

            Dictionary<IntPtr, EnumMember> enumMemberMap = new();
            foreach (var instance in typeMap.Values.OfType<EnumType>())
            {
                foreach (var member in instance.Members)
                {
                    enumMemberMap.Add(member.NativePointer, member);
                }
            }

            // type attributes
            var typeAttributesHashMapPointerPointer = Id2Pointer(885842);
            var typeAttributesHashMapPointer = runtime.ReadPointer(typeAttributesHashMapPointerPointer);
            ReadAttributes(runtime, typeAttributesHashMapPointer, typeMap, p => typeMap[p].Attributes);

            // enum member attributes
            var enumMemberAttributesHashMapPointerPointer = Id2Pointer(885840);
            var enumMemberAttributesHashMapPointer = runtime.ReadPointer(enumMemberAttributesHashMapPointerPointer);
            ReadAttributes(runtime, enumMemberAttributesHashMapPointer, typeMap, p => enumMemberMap[p].Attributes);

            // class property attributes
            foreach (var instance in typeMap.Values.OfType<ClassType>())
            {
                instance.ReadPropertyAttributes(runtime, typeMap);
            }

            var addressLibraryPointer2Id = addressLibrary.Invert();
            ulong Pointer2Id(IntPtr pointer)
            {
                var offset = (ulong)(pointer.ToInt64() - mainModuleBaseAddressValue);
                return addressLibraryPointer2Id[offset];
            }

            string output;
            using (var stringWriter = new StringWriter())
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                writer.IndentChar = ' ';

                writer.WriteStartObject();

                writer.WritePropertyName("generated_from");
                writer.WriteStartObject();

                writer.WritePropertyName("file_name");
                writer.WriteValue(addressLibrary.FileName);

                writer.WritePropertyName("version");
                writer.WriteValue(addressLibrary.FileVersion.ToString());

                writer.WriteEndObject();

                writer.WritePropertyName("types");
                writer.WriteStartObject();
                foreach (var type in typeMap.Values
                    .OrderBy(v => GetSort(v.Kind))
                    .ThenBy(v => GetSort(v))
                    .ThenBy(v => v.Name)
                    .ThenBy(v => Pointer2Id(v.NativePointer)))
                {
                    writer.WritePropertyName($"{Pointer2Id(type.NativePointer)}");
                    writer.WriteStartObject();
                    type.WriteJson(writer, Pointer2Id);
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();

                writer.WriteEndObject();

                writer.Flush();
                stringWriter.Flush();

                output = stringWriter.ToString();
            }

            File.WriteAllText(outputPath, output);
            return 0;
        }

        private static int GetSort(IType type)
        {
            if (type is BasicType basicType)
            {
                return basicType.Id;
            }

            return 0;
        }

        private static int GetSort(TypeKind kind) => kind switch
        {
            TypeKind.Basic => 0,
            TypeKind.String => 1,
            TypeKind.Enumeration => 3,
            TypeKind.Class => 2,
            TypeKind.List => 4,
            TypeKind.Set => 5,
            TypeKind.Map => 6,
            TypeKind.UniquePointer => 7,
            TypeKind.SharedPointer => 8,
            TypeKind.BorrowedPointer => 9,
            _ => throw new NotImplementedException(),
        };

        private static void ReadAttributes(RuntimeProcess runtime, IntPtr nativePointer, Dictionary<IntPtr, IType> typeMap, Func<IntPtr, List<Attributes.IAttribute>> getAttributesForPointer)
        {
            var native = runtime.ReadStructure<Natives.AttributesHashMap>(nativePointer);

            if (native.Size > int.MaxValue)
            {
                throw new InvalidOperationException();
            }

            var entryCount = (int)native.Size;

            var entrySize = Natives.AttributesHashMap.EntrySize;
            var endPointer = native.Table + entrySize * entryCount;
            var table = new Natives.AttributesHashMap.Entry[native.Size];
            var entryPointer = native.Table;
            for (var i = 0; entryPointer != endPointer; i++, entryPointer += entrySize)
            {
                table[i] = runtime.ReadStructure<Natives.AttributesHashMap.Entry>(entryPointer);
            }

            int index = 0;
            while (index < entryCount)
            {
                while (index < entryCount && table[index].NextIndex == -1)
                {
                    index++;
                }

                var pair = table[index].Pair;

                var attributes = getAttributesForPointer(pair.Key);
                if (attributes.Count > 0)
                {
                    throw new InvalidOperationException();
                }
                attributes.Clear();
                attributes.AddRange(ReadAttributes(runtime, pair.Value, typeMap));
                index++;
            }
        }

        internal static IEnumerable<Attributes.IAttribute> ReadAttributes(RuntimeProcess runtime, Natives.AttributeData native, Dictionary<IntPtr, IType> typeMap)
        {
            List<Attributes.IAttribute> attributes = new();
            int nextOffset = native.FirstOffset;
            while (nextOffset != -1)
            {
                var attributePointer = native.Data.Start + nextOffset;
                var attribute = runtime.ReadStructure<Natives.Attribute>(attributePointer);
                attributes.Add(ReadAttribute(runtime, attributePointer, attribute, typeMap));
                nextOffset = attribute.NextOffset;
            }
            return attributes;
        }

        private static Attributes.IAttribute ReadAttribute(RuntimeProcess runtime, IntPtr nativePointer, Natives.Attribute native, Dictionary<IntPtr, IType> typeMap)
        {
            var type = typeMap[native.Type];
            IntPtr dataPointer = nativePointer - ((int)type.Size).Align(8);
            var attribute = AttributeFactory.Create(type);
            var size = Marshal.SizeOf(attribute.NativeType);
            if (size != type.Size)
            {
                throw new InvalidOperationException();
            }
            attribute.Read(runtime, dataPointer, typeMap);
            return attribute;
        }

        private static List<IntPtr> ReadTypeList(RuntimeProcess runtime, IntPtr listPointer, int nextOffset)
        {
            List<IntPtr> typePointers = new();
            IntPtr nextPointer = runtime.ReadPointer(listPointer);
            while (nextPointer != IntPtr.Zero && nextPointer != listPointer)
            {
                var nativePointer = nextPointer;
                typePointers.Add(nativePointer);
                nextPointer = runtime.ReadPointer(nativePointer + nextOffset);
            }
            return typePointers;
        }
    }
}
