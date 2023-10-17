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
using DumpReflection.Reflection;
using Newtonsoft.Json;
using StarfieldDumping;

namespace DumpReflection.Attributes
{
    internal class RangeAttribute : BaseAttribute<RangeAttribute.Native>
    {
        public RangeAttribute(IType type) : base(type)
        {
        }

        public override bool CollapseJson => false;

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Step { get; set; }
        public int Unknown18 { get; set; }
        public int Unknown1C { get; set; }

        protected override void Read(RuntimeProcess runtime, Native native, Dictionary<IntPtr, IType> typeMap)
        {
            this.Minimum = native.Minimum;
            this.Maximum = native.Maximum;
            this.Step = native.Step;
            this.Unknown18 = native.Unknown18;
            this.Unknown1C = native.Unknown1C;
            
            if (this.Unknown18 != 4 || this.Unknown1C != 1)
            {
                //throw new InvalidOperationException();
            }
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WritePropertyName("min");
            writer.WriteValue(this.Minimum);

            writer.WritePropertyName("max");
            writer.WriteValue(this.Maximum);

            writer.WritePropertyName("step");
            writer.WriteValue(this.Step);

            writer.WritePropertyName("__unknown18");
            writer.WriteValue(this.Unknown18);

            writer.WritePropertyName("__unknown1C");
            writer.WriteValue(this.Unknown1C);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            public double Minimum;
            public double Maximum;
            public double Step;
            public int Unknown18;
            public int Unknown1C;

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
