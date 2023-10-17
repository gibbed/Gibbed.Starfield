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
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal abstract class BaseType<TNative> : IType
    {
        #region Fields
        private IntPtr _NativePointer;
        private IntPtr _VftablePointer;
        private uint _Size;
        private ushort _Alignment;
        private Natives.TypeKind _Kind;
        private Natives.TypeFlags _Flags;
        private readonly List<Attributes.IAttribute> _Attributes;
        #endregion

        public BaseType()
        {
            this._Attributes = new();
        }

        #region Properties
        public IntPtr NativePointer => this._NativePointer;
        public IntPtr VftablePointer => this._VftablePointer;
        public uint Size => this._Size;
        public ushort Alignment => this._Alignment;
        public Natives.TypeKind Kind => this._Kind;
        public Natives.TypeFlags Flags => this._Flags;
        public abstract string Name { get; }
        public List<Attributes.IAttribute> Attributes => this._Attributes;
        #endregion

        public void Read(RuntimeProcess runtime, IntPtr nativePointer)
        {
            TNative native = runtime.ReadStructure<TNative>(nativePointer);
            Read(runtime, nativePointer, native);
        }

        protected void Read(IntPtr nativePointer, Natives.BaseType native)
        {
            this._NativePointer = nativePointer;
            this._VftablePointer = native.Vftable;
            this._Size = native.Size;
            this._Alignment = native.Alignment;
            this._Kind = native.Kind;
            this._Flags = native.Flags;
        }

        protected abstract void Read(RuntimeProcess runtime, IntPtr nativePointer, TNative native);

        public virtual void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
        }

        void IType.WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WritePropertyName("name");
            writer.WriteValue(this.Name);

            if (this is CollectionType && this.Name == "std::map")
            {
                writer.WritePropertyName("name_is_liar");
                writer.WriteValue(true);

                writer.WritePropertyName("actual_name");
                writer.WriteValue("std::set");
            }

            //writer.WritePropertyName("id");
            //writer.WriteValue(pointer2Id(this.NativePointer));

            writer.WritePropertyName("vftable");
            writer.WriteValue(pointer2Id(this.VftablePointer));

            writer.WritePropertyName("size");
            writer.WriteValue(this.Size);

            writer.WritePropertyName("alignment");
            writer.WriteValue(this.Alignment);

            writer.WritePropertyName("kind");
            writer.WriteValue(this.Kind.ToString());

            if (this is not BasicType &&
                this is not EnumType &&
                this.Flags != Natives.TypeFlags.None)
            {
                writer.WritePropertyName("flags");
                writer.WriteValueFlags(this.Flags, Natives.TypeFlags.None, Natives.TypeFlags.Everything);
            }

            this.WriteJson(writer, pointer2Id);

            if (this.Attributes.Count > 0)
            {
                writer.WritePropertyName("attributes");
                writer.WriteStartArray();
                foreach (var attribute in this.Attributes)
                {
                    attribute.WriteJson(writer, pointer2Id);
                }
                writer.WriteEndArray();
            }
        }

        protected abstract void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id);

        public override string ToString()
        {
            return $"{this.Name ?? base.ToString()} {this.Size} {this.Alignment}";
        }
    }
}
