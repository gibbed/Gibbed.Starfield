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
    internal class EditWidgetAttribute : BaseAttribute<EditWidgetAttribute.Native>
    {
        public EditWidgetAttribute(IType type) : base(type)
        {
        }

        public override bool CollapseJson => false;

        public string Name { get; set; }
        public ulong Unknown { get; set; }

        protected override void Read(RuntimeProcess runtime, Native native, Dictionary<IntPtr, IType> typeMap)
        {
            this.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            this.Unknown = native.Unknown;
        }

        protected override void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WritePropertyName("name");
            writer.WriteValue(this.Name);

            writer.WritePropertyName("__unknown");
            writer.WriteValue(this.Unknown);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            public IntPtr Name; // 0
            public ulong Unknown; // 8

            static Native()
            {
                if (Marshal.SizeOf(typeof(Native)) != 0x10)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
