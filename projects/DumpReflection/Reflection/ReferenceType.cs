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
    internal abstract class ReferenceType : BaseType<Natives.BaseType>
    {
        #region Fields
        private readonly Natives.TypeKind _ExpectedTypeId;
        private string _Name;
        private IntPtr _UnderlyingTypePointer;
        private IType _UnderlyingType;
        #endregion

        protected ReferenceType(Natives.TypeKind expectedTypeId)
        {
            this._ExpectedTypeId = expectedTypeId;
        }

        #region Properties
        public override string Name => this._Name;
        public IntPtr UnderlyingTypePointer => this._UnderlyingTypePointer;
        public IType UnderlyingType => this._UnderlyingType;
        public string UnderlyingTypeName => $"{this._UnderlyingType?.Name ?? ("unknown:" + this._UnderlyingTypePointer.ToString("X"))}";
        #endregion

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.BaseType native)
        {
            this.Read(nativePointer, native);

            if (this.Kind != this._ExpectedTypeId)
            {
                throw new InvalidOperationException();
            }

            var knownFlags =
                Natives.TypeFlags.HasUnknownCallback20 |
                Natives.TypeFlags.HasUnknownCallback38 |
                Natives.TypeFlags.HasUnknownCallback30 |
                Natives.TypeFlags.HasUnknownCallback28;
            var unknownFlags = this.Flags & ~knownFlags;
            if (this.Flags != Natives.TypeFlags.Everything &&
                unknownFlags != Natives.TypeFlags.None)
            {
                throw new InvalidOperationException();
            }

            this._Name = Helpers.GetStringFromVftable(runtime, native.Vftable, 4);
            this._UnderlyingTypePointer = Helpers.GetPointerFromVftable(runtime, native.Vftable, 6);
        }

        public override void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
            base.Resolve(typeMap);
            typeMap.TryGetValue(this._UnderlyingTypePointer, out this._UnderlyingType);
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            if (this.UnderlyingTypePointer != IntPtr.Zero)
            {
                writer.WritePropertyName("underlying_type");
                writer.WriteValue(pointer2Id(this.UnderlyingTypePointer));
            }
        }
    }
}
