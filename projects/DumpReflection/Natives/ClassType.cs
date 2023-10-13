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

namespace DumpReflection.Natives
{
    [StructLayout(LayoutKind.Sequential)]
    internal class ClassType
    {
        public IntPtr Vftable;
        public uint Size;
        public ushort Alignment;
        public byte Unknown0E;
        public byte Unknown0F;
        public IntPtr Next;
        public IntPtr Name;
        public IntPtr Unknown20;
        public IntPtr Unknown28;
        public IntPtr Unknown30;
        public IntPtr Unknown38;
        public StdVector Fields;
        public IntPtr Unknown58;
        public IntPtr Unknown60;
        public IntPtr Unknown68;
        public IntPtr Unknown70;
        public IntPtr Unknown78;
        public IntPtr Unknown80;
        public uint Unknown88;
        public ushort Unknown8C;
    }
}
