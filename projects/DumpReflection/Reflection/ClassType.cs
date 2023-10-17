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
using System.Runtime.InteropServices;
using System.Text;
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal class ClassType : BaseType<Natives.ClassType>
    {
        public static IntPtr ExpectedVftablePointer;

        private string _Name;
        private readonly List<ClassProperty> _Properties;
        private readonly List<ClassCast> _Upcasts;
        private readonly List<ClassCast> _Downcasts;

        public ClassType()
        {
            this._Properties = new();
            this._Upcasts = new();
            this._Downcasts = new();
        }

        public override string Name => this._Name;
        public List<ClassProperty> Properties => this._Properties;
        public List<ClassCast> Upcasts => this._Upcasts;
        public List<ClassCast> Downcasts => this._Downcasts;

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.ClassType native)
        {
            this.Read(nativePointer, native.Base);

            this._Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);

            if (this.VftablePointer != ExpectedVftablePointer)
            {
                throw new InvalidOperationException();
            }

            if (this.TypeId != Natives.TypeId.Class)
            {
                throw new InvalidOperationException();
            }

            var expectedTypeFlags = Natives.TypeFlags.None;
            if (native.UnknownCallback20 != IntPtr.Zero)
            {
                expectedTypeFlags |= Natives.TypeFlags.HasUnknownCallback20;
            }
            if (native.UnknownCallback28 != IntPtr.Zero)
            {
                expectedTypeFlags |= Natives.TypeFlags.HasUnknownCallback28;
            }
            if (native.UnknownCallback30 != IntPtr.Zero)
            {
                expectedTypeFlags |= Natives.TypeFlags.HasUnknownCallback30;
            }
            if (native.UnknownCallback38 != IntPtr.Zero)
            {
                expectedTypeFlags |= Natives.TypeFlags.HasUnknownCallback38;
            }
            if ((native.Flags & Natives.ClassFlags.ClaimsToBeAStruct) != 0)
            {
                expectedTypeFlags |= Natives.TypeFlags.ClaimsToBeAStruct;
            }

            if ((native.Base.TypeFlags & ~Natives.TypeFlags.IsStruct) != expectedTypeFlags)
            {
                throw new InvalidOperationException();
            }

            var knownFlags =
                Natives.ClassFlags.Unknown1 |
                Natives.ClassFlags.Unknown2 |
                Natives.ClassFlags.ClaimsToBeAStruct |
                Natives.ClassFlags.Unknown4;
            var unknownFlags = native.Flags & ~knownFlags;
            if (unknownFlags != Natives.ClassFlags.None)
            {
                throw new InvalidOperationException();
            }

            var propertySize = Marshal.SizeOf(typeof(Natives.ClassProperty));
            List<ClassProperty> properties = new();
            for (var propertyPointer = native.Properties.Start; propertyPointer != native.Properties.End; propertyPointer += propertySize)
            {
                properties.Add(ReadProperty(runtime, propertyPointer));
            }

            var castSize = Marshal.SizeOf(typeof(Natives.ClassCast));
            List<ClassCast> upcasts = new();
            for (var castPointer = native.Upcasts.Start; castPointer != native.Upcasts.End; castPointer += castSize)
            {
                upcasts.Add(ReadCast(runtime, castPointer));
            }
            List<ClassCast> downcasts = new();
            for (var castPointer = native.Downcasts.Start; castPointer != native.Downcasts.End; castPointer += castSize)
            {
                downcasts.Add(ReadCast(runtime, castPointer));
            }

            this._Properties.Clear();
            this._Properties.AddRange(properties);
            this._Upcasts.Clear();
            this._Upcasts.AddRange(upcasts);
            this._Downcasts.Clear();
            this._Downcasts.AddRange(downcasts);
        }

        private static ClassProperty ReadProperty(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassProperty>(nativePointer);

            if (native.Unknown14 != 0 ||
                native.Unknown18 != IntPtr.Zero ||
                native.Unknown20 != IntPtr.Zero ||
                native.Unknown28 != IntPtr.Zero ||
                native.Unknown30 != -1 ||
                native.Unknown34 != -1)
            {

            }

            ClassProperty instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.TypePointer = native.Type;
            instance.Offset = native.Offset;
            return instance;
        }

        private static ClassCast ReadCast(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassCast>(nativePointer);

            ClassCast instance = new();
            instance.TypePointer = native.Type;
            instance.Offset = native.Offset;
            return instance;
        }

        public override void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
            base.Resolve(typeMap);

            foreach (var property in this._Properties)
            {
                if (typeMap.TryGetValue(property.TypePointer, out var fieldType) == false)
                {
                    throw new InvalidOperationException();
                }
                property.Type = fieldType;
            }

            foreach (var upcast in this._Upcasts)
            {
                if (typeMap.TryGetValue(upcast.TypePointer, out var castType) == false)
                {
                    throw new InvalidOperationException();
                }
                upcast.Type = castType;
            }

            foreach (var downcast in this._Downcasts)
            {
                if (typeMap.TryGetValue(downcast.TypePointer, out var castType) == false)
                {
                    throw new InvalidOperationException();
                }
                downcast.Type = castType;
            }
        }
    }
}
