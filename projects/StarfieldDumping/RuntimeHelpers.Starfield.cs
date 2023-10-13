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

namespace StarfieldDumping
{
    public static partial class RuntimeHelpers
    {
        public static string ReadShortString(this RuntimeProcess runtime, IntPtr basePointer)
        {
            var stringPointer = runtime.ReadPointer(basePointer);
            if (stringPointer == IntPtr.Zero)
            {
                return null;
            }
            var length = runtime.ReadValueU16(basePointer + 0x8);
            //var capacity = runtime.ReadValueU16(basePointer + 0x8);
            if (length == 0)
            {
                return string.Empty;
            }
            var dataPointer = runtime.ReadPointer(basePointer + 0x0);
            return runtime.ReadString(dataPointer, length, Encoding.ASCII);
        }

        public static string ReadFixedString(this RuntimeProcess runtime, IntPtr basePointer)
        {
            var stringPointer = runtime.ReadPointer(basePointer);
            while (stringPointer != IntPtr.Zero)
            {
                var flags = runtime.ReadValueU32(stringPointer + 0x14);
                if ((flags & (1u << 1)) == 0)
                {
                    break;
                }
                stringPointer = runtime.ReadPointer(stringPointer + 0x08);
            }
            if (stringPointer == IntPtr.Zero)
            {
                return null;
            }
            return runtime.ReadStringZ(stringPointer + 0x18, Encoding.ASCII);
        }
    }
}
