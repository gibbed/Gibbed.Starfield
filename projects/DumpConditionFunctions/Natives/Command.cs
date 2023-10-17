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
using System.Runtime.InteropServices;

namespace DumpConditionFunctions.Natives
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Command
    {
        public IntPtr LongName;
        public IntPtr ShortName;
        public uint Opcode;
        public IntPtr HelpText;
        public byte NeedsParent;
        public ushort NumParams;
        public IntPtr Params;
        public IntPtr Execute;
        public byte Unknown38;
        public IntPtr Parse;
        public IntPtr Eval;
        public byte Flags;
        public byte Unknown51;

        static Command()
        {
            if (Marshal.SizeOf(typeof(Command)) != 0x58)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
