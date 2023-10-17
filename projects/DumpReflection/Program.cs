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
using System.Linq;
using System.Runtime.InteropServices;
using DumpReflection.Reflection;
using Gibbed.AddressLibrary;
using StarfieldDumping;
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

            // attributes
            var classAttributesHashMapPointerPointer = Id2Pointer(885842);
            var classAttributesHashMapPointer = runtime.ReadPointer(classAttributesHashMapPointerPointer);
            ReadClassAttributes(runtime, classAttributesHashMapPointer, typeMap);

            foreach (var instance in typeMap.Values.OfType<ClassType>())
            {
                Console.WriteLine($"class {instance.Name}");

                foreach (var property in instance.Properties)
                {
                    Console.WriteLine($"  {property.Name} : {property.Type.Name} @{property.Offset:X}");
                }

                Console.WriteLine();
            }

            foreach (var instance in typeMap.Values.OfType<EnumType>())
            {
                Console.WriteLine($"enum {instance.Name}");

                foreach (var member in instance.Members)
                {
                    Console.WriteLine($"  {member.Name} = {member.Value}");
                }

                Console.WriteLine();
            }

            return 0;
        }

        private static void ReadClassAttributes(RuntimeProcess runtime, IntPtr nativePointer, Dictionary<IntPtr, IType> typeMap)
        {
            var native = runtime.ReadStructure<Natives.ClassAttributesHashMap>(nativePointer);

            if (native.Size > int.MaxValue)
            {
                throw new InvalidOperationException();
            }

            var entryCount = (int)native.Size;

            var entrySize = Natives.ClassAttributesHashMap.EntrySize;
            var endPointer = native.Table + entrySize * entryCount;
            var table = new Natives.ClassAttributesHashMap.Entry[native.Size];
            var entryPointer = native.Table;
            for (var i = 0; entryPointer != endPointer; i++, entryPointer += entrySize)
            {
                table[i] = runtime.ReadStructure<Natives.ClassAttributesHashMap.Entry>(entryPointer);
            }

            int index = 0;
            while (index < entryCount)
            {
                while (index < entryCount && table[index].NextIndex == -1)
                {
                    index++;
                }

                var pair = table[index].Pair;
                var type = typeMap[pair.Key];
                if (type.Attributes.Count > 0)
                {
                    throw new InvalidOperationException();
                }
                type.Attributes.Clear();
                type.Attributes.AddRange(ReadAttributes(runtime, pair.Value, typeMap));
                index++;
            }
        }

        private static List<Attributes.IAttribute> ReadAttributes(RuntimeProcess runtime, Natives.ClassAttributes native, Dictionary<IntPtr, IType> typeMap)
        {
            List<Attributes.IAttribute> attributes = new();
            int nextOffset = native.FirstOffset;
            while (nextOffset != -1)
            {
                var attributePointer = native.Data.Start + nextOffset;
                var attribute = runtime.ReadStructure<Natives.ClassAttribute>(attributePointer);
                attributes.Add(ReadAttribute(runtime, attributePointer, attribute, typeMap));
                nextOffset = attribute.NextOffset;
            }
            return attributes;
        }

        private static Attributes.IAttribute ReadAttribute(RuntimeProcess runtime, IntPtr nativePointer, Natives.ClassAttribute native, Dictionary<IntPtr, IType> typeMap)
        {
            var type = typeMap[native.Type];
            IntPtr dataPointer = nativePointer - ((int)type.TypeSize).Align(8);
            var attribute = AttributeFactory.Create(type.Name);
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
