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
using StarfieldDumping;

namespace DumpReflection.Attributes
{
    internal abstract class BaseAttribute<TNative> : IAttribute
    {
        #region Fields
        private string _NativeName;
        private IntPtr _NativePointer;
        #endregion

        #region Properties
        public IntPtr NativePointer => this._NativePointer;
        public string NativeName { get => this._NativeName; set => this._NativeName = value; }
        public Type NativeType => typeof(TNative);
        #endregion

        public void Read(RuntimeProcess runtime, IntPtr nativePointer, Dictionary<IntPtr, IType> typeMap)
        {
            TNative native = runtime.ReadStructure<TNative>(nativePointer);
            this._NativePointer = nativePointer;
            Read(runtime, native, typeMap);
        }

        protected abstract void Read(RuntimeProcess runtime, TNative native, Dictionary<IntPtr, IType> typeMap);
    }
}
