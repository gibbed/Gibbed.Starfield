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
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal abstract class BaseType<TNative> : IType
    {
        #region Fields
        private IntPtr _NativePointer;
        private IntPtr _VftablePointer;
        private uint _TypeSize;
        private ushort _TypeAlignment;
        private Natives.TypeId _TypeId;
        private Natives.TypeFlags _TypeFlags;
        #endregion

        #region Properties
        public IntPtr NativePointer => this._NativePointer;
        public IntPtr VftablePointer => this._VftablePointer;
        public uint TypeSize => this._TypeSize;
        public ushort TypeAlignment => this._TypeAlignment;
        public Natives.TypeId TypeId => this._TypeId;
        public Natives.TypeFlags TypeFlags => this._TypeFlags;
        public abstract string Name { get; }
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
            this._TypeSize = native.TypeSize;
            this._TypeAlignment = native.TypeAlignment;
            this._TypeId = native.TypeId;
            this._TypeFlags = native.TypeFlags;
        }

        protected abstract void Read(RuntimeProcess runtime, IntPtr nativePointer, TNative native);

        public virtual void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
        }

        public override string ToString()
        {
            return $"{this.Name ?? base.ToString()} {this.TypeSize} {this.TypeAlignment}";
        }
    }
}
