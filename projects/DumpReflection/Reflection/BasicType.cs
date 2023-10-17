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
using System.Text;
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal class BasicType : BaseType<Natives.BasicType>
    {
        public static IntPtr ExpectedVftablePointer;

        #region Fields
        private string _Name;
        private byte _Id;
        private bool _IsSigned;
        #endregion

        #region Properties
        public override string Name => this._Name;
        public byte Id => this._Id;
        public bool IsSigned => this._IsSigned;
        #endregion

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.BasicType native)
        {
            this.Read(nativePointer, native.Base);

            if (this.VftablePointer != ExpectedVftablePointer)
            {
                throw new InvalidOperationException();
            }

            if (this.Kind != Natives.TypeKind.Basic)
            {
                throw new InvalidOperationException();
            }

            if (this.Flags != Natives.TypeFlags.Everything)
            {
                throw new InvalidOperationException();
            }

            this._Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            this._Id = native.Id;
            this._IsSigned = native.IsSigned;
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WritePropertyName("id");
            writer.WriteValue(this.Id);

            writer.WritePropertyName("is_signed");
            writer.WriteValue(this.IsSigned);
        }
    }
}
