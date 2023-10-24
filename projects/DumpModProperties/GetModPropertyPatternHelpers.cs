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
using System.Linq;
using StarfieldDumping;

namespace DumpModProperties
{
    internal static class GetModPropertyPatternHelpers
    {
        internal struct PatternInfo
        {
            public ByteSearch.Pattern Pattern;
            public int CountOffsetOffset;
            public int CountOffsetSize;
            public int TableOffsetOffset;
            public int TableOffsetSize;

            public PatternInfo(
                ByteSearch.Pattern pattern,
                int countOffsetOffset, int countOffsetSize,
                int tableOffsetOffset, int tableOffsetSize)
            {
                this.Pattern = pattern;
                this.CountOffsetOffset = countOffsetOffset;
                this.CountOffsetSize = countOffsetSize;
                this.TableOffsetOffset = tableOffsetOffset;
                this.TableOffsetSize = tableOffsetSize;
            }

            public static implicit operator PatternInfo((
                ByteSearch.Pattern pattern,
                int countOffsetOffset, int countOffsetSize,
                int tableOffsetOffset, int tableOffsetSize) value)
            {
                return new
                (
                    value.pattern,
                    value.countOffsetOffset, value.countOffsetSize,
                    value.tableOffsetOffset, value.tableOffsetSize
                );
            }
        }

        private static readonly int _MaxPatternSize;
        private static readonly PatternInfo[] _PatternInfos;

        static GetModPropertyPatternHelpers()
        {
            var getModPropertyFunctionPattern1 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x83, 0xEC, 0x28 }, // sub rsp, 0x28
                new byte[] { 0x0F, 0xB7, 0x42, 0x18 }, // movzx eax, word ptr [rdx+0x18]
                new byte[] { 0x4D, 0x8B, 0xD0 }, // mov r10, r8
                new byte[] { 0x25, 0xFF, 0x07, 0x00, 0x00 }, // and eax, 0x7FF
                new byte[] { 0x83, 0xF8 }, // cmp eax, count
                ByteSearch.AnyBytes(1),
                new byte[] { 0x73, 0x21 }, // jnb short $
                new byte[] { 0x48, 0x85, 0xC9 }, // test rcx, rcx
                new byte[] { 0x74, 0x1C }, // jz short $
                new byte[] { 0x4C, 0x8D, 0x04, 0x40 }, // lea r8, [rax+rax*2]
                new byte[] { 0x4D, 0x03, 0xC0 }, // add r8, r8
                new byte[] { 0x48, 0x8D, 0x05 }, // lea rax, table
                ByteSearch.AnyBytes(4),
            };
            var getModPropertyFunctionPattern4 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x83, 0xEC, 0x28 }, // sub rsp, 0x28
                new byte[] { 0x0F, 0xB7, 0x42, 0x18 }, // movzx eax, word ptr [rdx+0x18]
                new byte[] { 0x4D, 0x8B, 0xD0 }, // mov r10, r8
                new byte[] { 0x25, 0xFF, 0x07, 0x00, 0x00 }, // and eax, 0x7FF
                new byte[] { 0x3D }, // cmp eax, count
                ByteSearch.AnyBytes(4),
                new byte[] { 0x73, 0x21 }, // jnb short $
                new byte[] { 0x48, 0x85, 0xC9 }, // test rcx, rcx
                new byte[] { 0x74, 0x1C }, // jz short $
                new byte[] { 0x4C, 0x8D, 0x04, 0x40 }, // lea r8, [rax+rax*2]
                new byte[] { 0x4D, 0x03, 0xC0 }, // add r8, r8
                new byte[] { 0x48, 0x8D, 0x05 }, // lea rax, table
                ByteSearch.AnyBytes(4),
            };
            _PatternInfos = new PatternInfo[]
            {
                (getModPropertyFunctionPattern1, 18, 1, 36, 4),
                (getModPropertyFunctionPattern4, 17, 4, 38, 4),
            };
            _MaxPatternSize = _PatternInfos.Max(t => t.Pattern.Count);
        }

        public static bool Match(
            RuntimeProcess runtime,
            IntPtr functionPointer,
            out (int count, IntPtr tablePointer) info)
        {
            var bytes = runtime.ReadBytes(functionPointer, _MaxPatternSize);
            foreach (var patternInfo in _PatternInfos)
            {
                if (ByteSearch.Match(bytes, patternInfo.Pattern, out var codeOffset) == false || codeOffset != 0)
                {
                    continue;
                }
                static int Read(byte[] bytes, int offset, int size)
                {
                    if (offset < 0)
                    {
                        return -1;
                    }
                    return size switch
                    {
                        1 => bytes[offset],
                        4 => BitConverter.ToInt32(bytes, offset),
                        _ => throw new NotSupportedException(),
                    };
                }
                var count = Read(bytes, patternInfo.CountOffsetOffset, patternInfo.CountOffsetSize);
                var tableRva = Read(bytes, patternInfo.TableOffsetOffset, patternInfo.TableOffsetSize);
                IntPtr tablePointer = functionPointer + codeOffset + patternInfo.TableOffsetOffset + patternInfo.TableOffsetSize + tableRva;
                info = (count, tablePointer);
                return true;
            }
            info = default;
            return false;
        }
    }
}
