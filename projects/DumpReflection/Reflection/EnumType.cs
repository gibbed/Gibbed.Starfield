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
    internal class EnumType : BaseType<Natives.EnumType>
    {
        public static IntPtr ExpectedVftablePointer;

        private string _Name;
        private readonly List<EnumMember> _Members;

        public EnumType()
        {
            this._Members = new();
        }

        public override string Name => this._Name;
        public List<EnumMember> Members => this._Members;

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.EnumType native)
        {
            this.Read(nativePointer, native.Base);

            if (this.VftablePointer != ExpectedVftablePointer)
            {
                throw new InvalidOperationException();
            }

            if (this.TypeId != Natives.TypeId.Enumeration)
            {
                throw new InvalidOperationException();
            }

            this._Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);

            var memberSize = Marshal.SizeOf(typeof(Natives.EnumMember));
            List<EnumMember> members = new();
            for (var memberPointer = native.Members.Start; memberPointer != native.Members.End; memberPointer += memberSize)
            {
                var member = ReadMember(runtime, memberPointer);
                members.Add(member);
            }

            this._Members.Clear();
            this._Members.AddRange(members);
        }

        private static EnumMember ReadMember(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.EnumMember>(nativePointer);

            EnumMember instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.Value = native.Value;
            return instance;
        }
    }
}
