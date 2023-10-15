﻿/* Copyright (c) 2023 Rick (rick 'at' gibbed 'dot' us)
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
    internal abstract class CollectionType : BaseType<Natives.BaseType>
    {
        #region Fields
        private readonly Natives.TypeId _ExpectedTypeId;
        private string _Name;
        private IntPtr _ItemTypePointer;
        private IType _ItemType;
        #endregion

        protected CollectionType(Natives.TypeId expectedTypeId)
        {
            this._ExpectedTypeId = expectedTypeId;
        }

        #region Properties
        public override string Name => $"{this._Name}<{this.ItemTypeName}>";
        public IntPtr ItemTypePointer => this._ItemTypePointer;
        public IType ItemType => this._ItemType;
        public string ItemTypeName => $"{this._ItemType?.Name ?? ("unknown:" + this._ItemTypePointer.ToString("X"))}";
        #endregion

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.BaseType native)
        {
            this.Read(nativePointer, native);

            if (this.TypeId != _ExpectedTypeId)
            {
                throw new InvalidOperationException();
            }

            this._Name = Helpers.GetStringFromVftable(runtime, native.Vftable, 4);
            this._ItemTypePointer = Helpers.GetPointerFromVftable(runtime, native.Vftable, 8);
        }

        public override void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
            base.Resolve(typeMap);
            typeMap.TryGetValue(this._ItemTypePointer, out this._ItemType);
        }
    }
}
