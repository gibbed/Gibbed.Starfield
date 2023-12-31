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
using Newtonsoft.Json;

namespace DumpReflection.Reflection
{
    internal class ClassCast
    {
        public IntPtr TypePointer { get; set; }
        public IType Type { get; set; }
        public long Offset { get; set; }

        public void WriteJson(JsonWriter writer, Func<IntPtr, ulong> pointer2Id)
        {
            writer.WriteStartObject();

            var oldFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;

            writer.WritePropertyName("type");
            writer.WriteValue(pointer2Id(this.TypePointer));

            if (this.Offset != 0)
            {
                writer.WritePropertyName("offset");
                writer.WriteValue(this.Offset);
            }

            writer.WriteEndObject();

            writer.Formatting = oldFormatting;
        }

        public override string ToString()
        {
            return $"{this.Type.Name} : {this.Offset}";
        }
    }
}
