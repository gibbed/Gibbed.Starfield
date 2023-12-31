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
using System.Runtime.InteropServices;

namespace DumpReflection.Natives
{
    // appears to be BSTHashMap<>
    [StructLayout(LayoutKind.Sequential)]
    internal struct AttributesHashMap
    {
        public static readonly int EntrySize;

        public Pair Unknown; // 00
        public IntPtr Table; // 28
        public ulong Size; // 30
        public ulong Free; // 38
        public ulong LastFree; // 40

        [StructLayout(LayoutKind.Sequential)]
        public struct Pair
        {
            public IntPtr Key; // 00
            public AttributeData Value; // 08
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            public Pair Pair; // 00
            public int NextIndex; // 28
            public int Index; // 2C
        }

        static AttributesHashMap()
        {
            if (Marshal.SizeOf(typeof(AttributesHashMap)) != 0x48)
            {
                throw new InvalidOperationException();
            }

            if (Marshal.SizeOf(typeof(Pair)) != 0x28)
            {
                throw new InvalidOperationException();
            }

            EntrySize = Marshal.SizeOf(typeof(Entry));
            if (EntrySize != 0x30)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
