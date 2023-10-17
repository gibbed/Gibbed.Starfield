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
using System.Runtime.InteropServices;
using System.Text;
using DumpReflection.Reflection;
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Attributes
{
    internal class ConditionalDisplayNameAttribute : BaseAttribute<ConditionalDisplayNameAttribute.Native>
    {
        public ConditionalDisplayNameAttribute(IType type) : base(type)
        {
        }

        public override bool CollapseJson => false;

        public string Unknown00 { get; set; }
        public ulong Unknown08 { get; set; }
        public string Unknown10 { get; set; }
        public string Unknown18 { get; set; }

        protected override void Read(RuntimeProcess runtime, Native native, Dictionary<IntPtr, IType> typeMap)
        {
            this.Unknown00 = runtime.ReadStringZ(native.Unknown00, Encoding.ASCII);
            this.Unknown08 = native.Unknown08;
            this.Unknown10 = runtime.ReadStringZ(native.Unknown10, Encoding.ASCII);
            this.Unknown18 = runtime.ReadStringZ(native.Unknown18, Encoding.ASCII);
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WritePropertyName("__unknown00");
            writer.WriteValue(this.Unknown00);

            writer.WritePropertyName("__unknown08");
            writer.WriteValue(this.Unknown08);

            writer.WritePropertyName("__unknown10");
            writer.WriteValue(this.Unknown10);

            writer.WritePropertyName("__unknown18");
            writer.WriteValue(this.Unknown18);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            public IntPtr Unknown00; // 0
            public ulong Unknown08; // 0
            public IntPtr Unknown10; // 0
            public IntPtr Unknown18; // 0

            static Native()
            {
                if (Marshal.SizeOf(typeof(Native)) != 0x20)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
