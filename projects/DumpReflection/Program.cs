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
using System.Text;
using DumpReflection.Reflection;
using Gibbed.AddressLibrary;
using Gibbed.IO;
using StarfieldDumping;
using IndentedTextWriter = System.CodeDom.Compiler.IndentedTextWriter;
using TypeId = DumpReflection.Natives.TypeId;

namespace DumpReflection
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Environment.ExitCode = DumpingHelpers.Main(args, Dump);
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                Console.ReadLine();
            }
        }

        private static int Dump(RuntimeProcess runtime, AddressLibrary addressLibrary, string[] args)
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

            var baseTypeTypeOffset = Marshal.OffsetOf<Natives.BaseType>(nameof(Natives.BaseType.TypeId)).ToInt32();
            var classNextOffset = Marshal.OffsetOf<Natives.ClassType>(nameof(Natives.ClassType.Next)).ToInt32();
            var enumNextOffset = Marshal.OffsetOf<Natives.EnumType>(nameof(Natives.EnumType.Next)).ToInt32();

            Queue<IntPtr> queue = new();

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

                var nativeTypeType = (TypeId)runtime.ReadValueU8(nativePointer + baseTypeTypeOffset);

                IType instance = nativeTypeType switch
                {
                    TypeId.Basic => new BasicType(),
                    TypeId.String => new StringType(),
                    TypeId.Enumeration => new EnumType(),
                    TypeId.Class => new ClassType(),
                    TypeId.List => new ListType(),
                    TypeId.Set => new SetType(),
                    TypeId.Map => new MapType(),
                    TypeId.UniquePointer => new UniquePointerType(),
                    TypeId.SharedPointer => new SharedPointerType(),
                    TypeId.BorrowedPointer => new BorrowedPointerType(),
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


            // TODO(gibbed): JSON output
            // TODO(gibbed): C# output

            StringBuilder sb = new();
            using StringWriter stringWriter = new(sb);
            using IndentedTextWriter writer = new(stringWriter, "  ");

            foreach (var instance in typeMap.Values.OfType<ClassType>())
            {
                WriteAttributes(writer, instance.Attributes);
                writer.WriteLine($"class {instance.Name}");
                writer.WriteLine("{");
                foreach (var property in instance.Properties)
                {
                    writer.Indent++;
                    WriteAttributes(writer, property.Attributes);
                    writer.WriteLine($"public {property.Type.Name} {property.Name}; // @{property.Offset:X}");
                    writer.Indent--;
                }
                writer.WriteLine("}");
                writer.WriteLine();
            }

            foreach (var instance in typeMap.Values.OfType<EnumType>())
            {
                WriteAttributes(writer, instance.Attributes);
                writer.WriteLine($"enum {instance.Name}");
                writer.WriteLine("{");
                foreach (var member in instance.Members)
                {
                    writer.Indent++;
                    WriteAttributes(writer, member.Attributes);
                    writer.WriteLine($"{member.Name} = {member.Value},");
                    writer.Indent--;
                }
                writer.WriteLine("}");
                writer.WriteLine();
            }

            writer.Flush();
            stringWriter.Flush();

            File.WriteAllText("reflection_dump.cs", sb.ToString());
            return 0;
        }

        private static void WriteAttributes(IndentedTextWriter writer, List<Attributes.IAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                writer.Write($"[{attribute.NativeName}");
                if (attribute is Attributes.BaseEmptyAttribute)
                {
                }
                else if (attribute is Attributes.BaseByteAttribute byteAttribute)
                {
                    writer.Write($"(0x{byteAttribute.Value:X})");
                }
                else if (attribute is Attributes.BaseUIntAttribute uintAttribute)
                {
                    writer.Write($"(0x{uintAttribute.Value:X})");
                }
                else if (attribute is Attributes.BaseStringAttribute stringAttribute)
                {
                    var value = stringAttribute.Value;
                    value = value.Replace("\\", "\\\\");
                    value = value.Replace("\n", "\\n");
                    value = value.Replace("\r", "\\r");
                    value = value.Replace("\"", "\\\"");
                    value = value.Replace("\"", "\\\"");
                    writer.Write($"(\"{value}\")");
                }
                else if (attribute is Attributes.BasePointerAttribute pointerAttribute)
                {
                    writer.Write($"(0x{pointerAttribute.Value.ToInt64():X})");
                }
                else if (attribute is Attributes.AttributeAttribute attributeAttribute)
                {
                    writer.Write($"(Usage = {attributeAttribute.Usage})");
                }
                else
                {
                    writer.Write($"(?)");
                }
                writer.WriteLine($"]");
            }
        }

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
            IntPtr dataPointer = nativePointer - ((int)type.TypeSize).Align(8);
            var attribute = AttributeFactory.Create(type.Name);
            attribute.NativeName = type.Name;
            var size = Marshal.SizeOf(attribute.NativeType);
            if (size != type.TypeSize)
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
