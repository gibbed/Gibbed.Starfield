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
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal class ClassType : BaseType<Natives.ClassType>
    {
        public static IntPtr ExpectedVftablePointer;

        #region Fields
        private string _Name;
        private readonly List<ClassProperty> _Properties;
        private readonly List<ClassCast> _Upcasts;
        private readonly List<ClassCast> _Downcasts;
        #endregion

        public ClassType()
        {
            this._Properties = new();
            this._Upcasts = new();
            this._Downcasts = new();
        }

        #region Properties
        public override string Name => this._Name;
        public IntPtr UnknownCallback20 { get; set; }
        public IntPtr UnknownCallback28 { get; set; }
        public IntPtr UnknownCallback30 { get; set; }
        public IntPtr UnknownCallback38 { get; set; }
        public List<ClassProperty> Properties => this._Properties;
        public List<ClassCast> Upcasts => this._Upcasts;
        public List<ClassCast> Downcasts => this._Downcasts;
        public Natives.ClassFlags ClassFlags { get; set; }
        public ushort Unknown8C { get; set; }
        #endregion

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.ClassType native)
        {
            this.Read(nativePointer, native.Base);

            this._Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);

            if (this.VftablePointer != ExpectedVftablePointer)
            {
                throw new InvalidOperationException();
            }

            if (this.Kind != Natives.TypeKind.Class)
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

            if ((native.Base.Flags & ~Natives.TypeFlags.IsStruct) != expectedTypeFlags)
            {
                throw new InvalidOperationException();
            }

            var knownFlags =
                Natives.ClassFlags.Unknown1 |
                Natives.ClassFlags.Unknown2 |
                Natives.ClassFlags.ClaimsToBeAStruct |
                Natives.ClassFlags.NotDiffed;
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

            this.UnknownCallback20 = native.UnknownCallback20;
            this.UnknownCallback28 = native.UnknownCallback28;
            this.UnknownCallback30 = native.UnknownCallback30;
            this.UnknownCallback38 = native.UnknownCallback38;
            this.ClassFlags = native.Flags;
            this.Unknown8C = native.Unknown8C;
        }

        private static ClassProperty ReadProperty(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassProperty>(nativePointer);

            ClassProperty instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.TypePointer = native.Type;
            instance.Offset = native.Offset;
            instance.Unknown = native.Unknown14;
            instance.AttributeData = native.AttributeData;
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

        public void ReadPropertyAttributes(RuntimeProcess runtime, Dictionary<IntPtr, IType> typeMap)
        {
            foreach (var property in this._Properties)
            {
                property.Attributes.Clear();
                property.Attributes.AddRange(Program.ReadAttributes(runtime, property.AttributeData, typeMap));
            }
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            if (this.ClassFlags != Natives.ClassFlags.None)
            {
                writer.WritePropertyName("class_flags");
                writer.WriteValueFlags(this.ClassFlags, Natives.ClassFlags.None);
            }

            if (this.UnknownCallback20 != IntPtr.Zero)
            {
                writer.WritePropertyName("__unknown20callback");
                writer.WriteValue(pointer2Id(this.UnknownCallback20));
            }

            if (this.UnknownCallback28 != IntPtr.Zero)
            {
                writer.WritePropertyName("__unknown28callback");
                writer.WriteValue(pointer2Id(this.UnknownCallback28));
            }

            if (this.UnknownCallback30 != IntPtr.Zero)
            {
                writer.WritePropertyName("__unknown30callback");
                writer.WriteValue(pointer2Id(this.UnknownCallback30));
            }

            if (this.UnknownCallback38 != IntPtr.Zero)
            {
                writer.WritePropertyName("__unknown38callback");
                writer.WriteValue(pointer2Id(this.UnknownCallback38));
            }


            if (this.Unknown8C != 0)
            {
                writer.WritePropertyName("__unknown8C");
                writer.WriteValue(this.Unknown8C);
            }

            if (this.Properties.Count > 0)
            {
                writer.WritePropertyName("properties");
                writer.WriteStartArray();
                foreach (var property in this.Properties)
                {
                    property.WriteJson(writer, pointer2Id);
                }
                writer.WriteEndArray();
            }

            if (this.Upcasts.Count > 0)
            {
                writer.WritePropertyName("upcasts");
                writer.WriteStartArray();
                foreach (var cast in this.Upcasts)
                {
                    cast.WriteJson(writer, pointer2Id);
                }
                writer.WriteEndArray();
            }

            // TODO(gibbed): necessary? let people generate it from upcasts?
            if (this.Downcasts.Count > 0)
            {
                writer.WritePropertyName("downcasts");
                writer.WriteStartArray();
                foreach (var cast in this.Downcasts)
                {
                    cast.WriteJson(writer, pointer2Id);
                }
                writer.WriteEndArray();
            }
        }
    }
}
