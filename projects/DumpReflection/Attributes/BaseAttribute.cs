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
using DumpReflection.Reflection;
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Attributes
{
    internal abstract class BaseAttribute<TNative> : IAttribute
    {
        #region Fields
        private readonly IType _Type;
        private IntPtr _NativePointer;
        #endregion

        protected BaseAttribute(IType type)
        {
            this._Type = type;
        }

        #region Properties
        public IType Type => this._Type;
        public Type NativeType => typeof(TNative);
        public IntPtr NativePointer => this._NativePointer;
        public virtual bool CollapseJson => true;
        #endregion

        public void Read(RuntimeProcess runtime, IntPtr nativePointer, Dictionary<IntPtr, IType> typeMap)
        {
            TNative native = runtime.ReadStructure<TNative>(nativePointer);
            this._NativePointer = nativePointer;
            Read(runtime, native, typeMap);
        }

        protected abstract void Read(RuntimeProcess runtime, TNative native, Dictionary<IntPtr, IType> typeMap);

        void IAttribute.WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WriteStartObject();

            var oldFormatting = writer.Formatting;
            if (this.CollapseJson == true)
            {
                writer.Formatting = Formatting.None;
            }

            writer.WritePropertyName("type");
            writer.WriteValue(pointer2Id(this.Type.NativePointer));

            this.WriteJson(writer, pointer2Id);

            writer.WriteEndObject();

            writer.Formatting = oldFormatting;
        }

        protected abstract void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id);
    }
}
