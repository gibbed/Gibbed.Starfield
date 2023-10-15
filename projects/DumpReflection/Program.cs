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
using System.Text;
using Gibbed.AddressLibrary;
using StarfieldDumping;

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

            var classTypes = ReadClassTypes(runtime, Id2Pointer);
            var enumTypes = ReadEnumTypes(runtime, Id2Pointer);

            Dictionary<IntPtr, string> typeNames = new();
            foreach (var (typeId, typeName) in GetTypeNames())
            {
                typeNames[Id2Pointer(typeId)] = typeName;
            }

            Dictionary<IntPtr, ClassType> classTypeMap = new();
            foreach (var instance in classTypes)
            {
                classTypeMap.Add(instance.Pointer, instance);
            }

            Dictionary<IntPtr, EnumType> enumTypeMap = new();
            foreach (var instance in enumTypes)
            {
                enumTypeMap.Add(instance.Pointer, instance);
            }

            foreach (var instance in classTypes)
            {
                Console.WriteLine($"{instance.Name}");
                foreach (var field in instance.Fields)
                {
                    string typeName;
                    if (classTypeMap.TryGetValue(field.Type, out var classType) == true)
                    {
                        typeName = classType.Name;
                    }
                    else if (enumTypeMap.TryGetValue(field.Type, out var enumType) == true)
                    {
                        typeName = enumType.Name;
                    }
                    else if (typeNames.TryGetValue(field.Type, out typeName) == false)
                    {
                        typeName = $"unknown:{runtime.ToStaticAddress(field.Type):X}";
                    }

                    Console.WriteLine($"  {field.Name} : {typeName} @{field.Offset:X}");
                }
                Console.WriteLine();
            }

            return 0;
        }

        private static List<ClassType> ReadClassTypes(RuntimeProcess runtime, Func<ulong, IntPtr> id2Pointer)
        {
            var classTypesPointerPointer = id2Pointer(885835);
            var classTypeVftablePointer = id2Pointer(285167);

            List<Natives.ClassType> natives = new();
            List<ClassType> instances = new();
            IntPtr nextPointer = runtime.ReadPointer(classTypesPointerPointer);
            while (nextPointer != IntPtr.Zero && nextPointer != classTypesPointerPointer)
            {
                var nativePointer = nextPointer;
                var native = runtime.ReadStructure<Natives.ClassType>(nativePointer);
                nextPointer = native.Next;

                if (native.Vftable != classTypeVftablePointer)
                {
                    var vftable = runtime.ToStaticAddress(native.Vftable);
                    throw new InvalidOperationException($"unknown vftable @ {vftable:X}");
                }

                natives.Add(native);
                ClassType instance = ReadClassType(runtime, nativePointer);
                instances.Add(instance);
            }
            return instances;
        }

        private static ClassType ReadClassType(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassType>(nativePointer);

            ClassType instance = new();
            instance.Pointer = nativePointer;
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.Size = native.Size;
            instance.Alignment = native.Alignment;

            var fieldSize = Marshal.SizeOf(typeof(Natives.ClassField));

            List<ClassField> fields = new();
            for (var fieldPointer = native.Fields.Start; fieldPointer != native.Fields.End; fieldPointer += fieldSize)
            {
                var field = ReadClassField(runtime, fieldPointer);
                fields.Add(field);
            }

            instance.Fields.AddRange(fields);

            return instance;
        }

        private static ClassField ReadClassField(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassField>(nativePointer);

            if (native.Unknown14 != 0 ||
                native.Unknown18 != IntPtr.Zero ||
                native.Unknown20 != IntPtr.Zero ||
                native.Unknown28 != IntPtr.Zero ||
                native.Unknown30 != -1 ||
                native.Unknown34 != -1)
            {

            }

            ClassField instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.Type = native.Type;
            instance.Offset = native.Offset;
            return instance;
        }

        private static List<EnumType> ReadEnumTypes(RuntimeProcess runtime, Func<ulong, IntPtr> id2Pointer)
        {
            IntPtr enumTypesPointerPointer = id2Pointer(885839);
            IntPtr enumTypeVftablePointer = id2Pointer(292531);

            List<Natives.EnumType> natives = new();
            List<EnumType> instances = new();
            IntPtr nextPointer = runtime.ReadPointer(enumTypesPointerPointer);
            while (nextPointer != IntPtr.Zero && nextPointer != enumTypesPointerPointer)
            {
                var nativePointer = nextPointer;
                var native = runtime.ReadStructure<Natives.EnumType>(nativePointer);
                nextPointer = native.Next;

                if (native.Vftable != enumTypeVftablePointer)
                {
                    var vftable = runtime.ToStaticAddress(native.Vftable);
                    throw new InvalidOperationException($"unknown vftable @ {vftable:X}");
                }

                natives.Add(native);
                EnumType instance = ReadEnumType(runtime, nativePointer);
                instances.Add(instance);
            }
            return instances;
        }

        private static EnumType ReadEnumType(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.EnumType>(nativePointer);

            EnumType instance = new();
            instance.Pointer = nativePointer;
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.Size = native.Size;
            instance.Alignment = native.Alignment;
            instance.Unknown0E = native.Unknown0E;

            var memberSize = Marshal.SizeOf(typeof(Natives.EnumMember));

            List<EnumMember> members = new();
            for (var memberPointer = native.Members.Start; memberPointer != native.Members.End; memberPointer += memberSize)
            {
                var member = ReadEnumMember(runtime, memberPointer);
                members.Add(member);
            }

            instance.Members.AddRange(members);

            return instance;
        }

        private static EnumMember ReadEnumMember(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.EnumMember>(nativePointer);

            EnumMember instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.Value = native.Value;
            return instance;
        }
    }
}
