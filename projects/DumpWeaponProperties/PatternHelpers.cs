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

namespace DumpWeaponProperties
{
    internal static class PatternHelpers
    {
        public enum LoadType
        {
            UInt8,
            UInt16,
            UInt32,
            Single,
            SingleToInt32,
            FormId,
            FormIdAndInt32,
        };

        internal struct LoadPattern
        {
            public ByteSearch.Pattern Pattern;
            public int StructOffsetOffset;
            public int StructOffsetSize;
            public int Struct2OffsetOffset;
            public int Struct2OffsetSize;
            public int FieldOffsetOffset;
            public int FieldOffsetSize;
            public LoadType Type;

            public LoadPattern(
                ByteSearch.Pattern pattern,
                int structOffsetOffset, int structOffsetSize,
                int struct2OffsetOffset, int struct2OffsetSize,
                int fieldOffsetOffset, int fieldOffsetSize,
                LoadType type)
            {
                this.Pattern = pattern;
                this.StructOffsetOffset = structOffsetOffset;
                this.StructOffsetSize = structOffsetSize;
                this.Struct2OffsetOffset = struct2OffsetOffset;
                this.Struct2OffsetSize = struct2OffsetSize;
                this.FieldOffsetOffset = fieldOffsetOffset;
                this.FieldOffsetSize = fieldOffsetSize;
                this.Type = type;
            }

            public static implicit operator LoadPattern((
                ByteSearch.Pattern pattern,
                int structOffsetOffset, int structOffsetSize,
                int struct2OffsetOffset, int struct2OffsetSize,
                int fieldOffsetOffset, int fieldOffsetSize,
                LoadType type) value)
            {
                return new
                (
                    value.pattern,
                    value.structOffsetOffset, value.structOffsetSize,
                    value.struct2OffsetOffset, value.struct2OffsetSize,
                    value.fieldOffsetOffset, value.fieldOffsetSize,
                    value.type
                );
            }

            public static implicit operator LoadPattern((
                ByteSearch.Pattern pattern,
                int structOffsetOffset, int structOffsetSize,
                int fieldOffsetOffset, int fieldOffsetSize,
                LoadType type) value)
            {
                return new
                (
                    value.pattern,
                    value.structOffsetOffset, value.structOffsetSize,
                    -1, 0,
                    value.fieldOffsetOffset, value.fieldOffsetSize,
                    value.type
                );
            }

            public static implicit operator LoadPattern((
                ByteSearch.Pattern pattern,
                int fieldOffsetOffset, int fieldOffsetSize,
                LoadType type) value)
            {
                return new
                (
                    value.pattern,
                    -1, 0,
                    -1, 0,
                    value.fieldOffsetOffset, value.fieldOffsetSize,
                    value.type
                );
            }
        }

        private static readonly ByteSearch.Pattern _WeaponPropertyCountPattern;
        private static readonly int _MaxLoadSize;
        private static readonly LoadPattern[] _LoadPatterns;

        static PatternHelpers()
        {
            _WeaponPropertyCountPattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x83, 0xEC, 0x28 }, // sub rsp, 0x28
                new byte[] { 0x0F, 0xB7, 0x42, 0x18 }, // movzx eax, word ptr [rdx+0x18]
                new byte[] { 0x4D, 0x8B, 0xD0 }, // mov r10, r8
                new byte[] { 0x25, 0xFF, 0x07, 0x00, 0x00 }, // and eax, 0x7FF
                new byte[] { 0x3D }, // cmp eax, 150
                ByteSearch.AnyBytes(4),
                new byte[] { 0x73, 0x21 }, // jnb short $
            };
            var loadUInt16Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x44, 0x0F, 0xB7, 0x81 }, // movzx r8d, word ptr [rcx+fieldOffset]
                ByteSearch.AnyBytes(4),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x44, 0x89, 0x42, 0x08 }, // mov [rdx+8], r8d
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructUInt8Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x0F, 0xB6, 0x48 }, // movzx ecx, byte ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x89, 0x4A, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructUInt16Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x0F, 0xB7, 0x48 }, // movzx ecx, word ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x89, 0x4A, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructUInt32Pattern11 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x8B, 0x48 }, // mov ecx, [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0x89, 0x4A, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xC3 }, // retn
            };
            var loadStructUInt32Pattern14 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x8B, 0x88 }, // mov ecx, [rax+fieldOffset]
                ByteSearch.AnyBytes(4),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0x89, 0x4A, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xC3 }, // retn
            };
            var loadStructFormIdAndInt32Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x48, 0x89, 0x4A, 0x08 }, // mov [rdx+8], rcx
                new byte[] { 0xC7, 0x02, 0x03, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 3
                new byte[] { 0xC3 }, // retn
            };
            var loadStructSinglePattern11 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xC5, 0xFA, 0x10, 0x40 }, // vmovss xmm0, dword ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x83, 0x22, 0x00 }, // and dword ptr [rdx], 0
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC5, 0xFA, 0x11, 0x42, 0x08 }, // vmovss dword ptr [rdx+8], xmm0
                new byte[] { 0xC3 }, // retn
            };
            var loadStructSinglePattern14 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xC5, 0xFA, 0x10, 0x80 }, // vmovss xmm0, dword ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(4),
                new byte[] { 0x83, 0x22, 0x00 }, // and dword ptr [rdx], 0
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC5, 0xFA, 0x11, 0x42, 0x08 }, // vmovss dword ptr [rdx+8], xmm0
                new byte[] { 0xC3 }, // retn
            };
            var loadStructSinglePattern41 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x81 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(4),
                new byte[] { 0xC5, 0xFA, 0x10, 0x40 }, // vmovss xmm0, dword ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x83, 0x22, 0x00 }, // and dword ptr [rdx], 0
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC5, 0xFA, 0x11, 0x42, 0x08 }, // vmovss dword ptr [rdx+8], xmm0
                new byte[] { 0xC3 }, // retn
            };
            var loadStructFormIdPattern11 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov ecx, [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x33, 0xC0 }, // xor eax, eax
                new byte[] { 0x48, 0x85, 0xC9 }, // test rcx, rcx
                new byte[] { 0x74, 0x03 }, // jz $+3
                new byte[] { 0x8B, 0x41, 0x30 }, // mov eax, [rcx+0x30]
                new byte[] { 0x89, 0x42, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructFormIdPattern41 = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x81 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(4),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov ecx, [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x33, 0xC0 }, // xor eax, eax
                new byte[] { 0x48, 0x85, 0xC9 }, // test rcx, rcx
                new byte[] { 0x74, 0x03 }, // jz $+3
                new byte[] { 0x8B, 0x41, 0x30 }, // mov eax, [rcx+0x30]
                new byte[] { 0x89, 0x42, 0x08 }, // mov [rdx+8], ecx
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructStructUInt8Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+struct2Offset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x44, 0x0F, 0xB6, 0x41 }, // mov r8d, byte ptr [rcx+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x44, 0x89, 0x42, 0x08 }, // mov [rdx+8], r8d
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructStructUInt32Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+struct2Offset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0x44, 0x8B, 0x41 }, // mov r8d, [rcx+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0x44, 0x89, 0x42, 0x08 }, // mov [rdx+8], r8d
                new byte[] { 0xC3 }, // retn
            };
            var loadStructStructSinglePattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+struct2Offset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC5, 0xFA, 0x10, 0x41 }, // vmovss xmm0, dword ptr [rax+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x83, 0x22, 0x00 }, // and dword ptr [rdx], 0
                new byte[] { 0xC5, 0xFA, 0x11, 0x42, 0x08 }, // vmovss dword ptr [rdx+8], xmm0
                new byte[] { 0xC3 }, // retn
            };
            var loadStructStructSingleToInt32Pattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+struct2Offset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC5, 0x7A, 0x2C, 0x41 }, // vcvttss2si r8d, dword ptr [rcx+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x44, 0x89, 0x42, 0x08 }, // mov [rdx+8], r8d
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            var loadStructStructFormIdPattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8B, 0x41 }, // mov rax, [rcx+structOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x48, 0x8B, 0x48 }, // mov rcx, [rax+struct2Offset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x33, 0xC0 }, // xor eax, eax
                new byte[] { 0x4C, 0x8B, 0x41 }, // mov r8, [rcx+fieldOffset]
                ByteSearch.AnyBytes(1),
                new byte[] { 0x4D, 0x85, 0xC0 }, // test r8, r8
                new byte[] { 0x74, 0x04 }, // jz $+4
                new byte[] { 0x41, 0x8B, 0x40, 0x30 }, // mov eax, [rcx+0x30]
                new byte[] { 0x89, 0x42, 0x08 }, // mov [rdx+8], eax
                new byte[] { 0xB0, 0x01 }, // mov al, 1
                new byte[] { 0xC7, 0x02, 0x01, 0x00, 0x00, 0x00 }, // mov dword ptr [rdx], 1
                new byte[] { 0xC3 }, // retn
            };
            _LoadPatterns = new LoadPattern[]
            {
                (loadUInt16Pattern, 4, 4, LoadType.UInt16),
                (loadStructUInt8Pattern, 3, 1, 7, 1, LoadType.UInt8),
                (loadStructUInt16Pattern, 3, 1, 7, 1, LoadType.UInt16),
                (loadStructUInt32Pattern11, 3, 1, 6, 1, LoadType.UInt32),
                (loadStructUInt32Pattern14, 3, 1, 6, 4, LoadType.UInt32),
                (loadStructFormIdAndInt32Pattern, 3, 1, 7, 1, LoadType.FormIdAndInt32),
                (loadStructSinglePattern11, 3, 1, 8, 1, LoadType.Single),
                (loadStructSinglePattern14, 3, 1, 8, 4, LoadType.Single),
                (loadStructSinglePattern41, 3, 4, 11, 1, LoadType.Single),
                (loadStructFormIdPattern11, 3, 1, 7, 1, LoadType.FormId),
                (loadStructFormIdPattern41, 3, 4, 10, 1, LoadType.FormId),
                (loadStructStructUInt8Pattern, 3, 1, 7, 1, 14, 1, LoadType.UInt8),
                (loadStructStructUInt32Pattern, 3, 1, 7, 1, 13, 1, LoadType.UInt32),
                (loadStructStructSinglePattern, 3, 1, 7, 1, 14, 1, LoadType.Single),
                (loadStructStructSingleToInt32Pattern, 3, 1, 7, 1, 14, 1, LoadType.SingleToInt32),
                (loadStructStructFormIdPattern, 3, 1, 7, 1, 13, 1, LoadType.FormId),
            };
            _MaxLoadSize = _LoadPatterns.Max(t => t.Pattern.Count);
        }

        public static int GetWeaponPropertyCount(RuntimeProcess runtime, IntPtr functionPointer)
        {
            var bytes = runtime.ReadBytes(functionPointer, _WeaponPropertyCountPattern.Count);
            if (ByteSearch.Match(bytes, _WeaponPropertyCountPattern, out var codeOffset) == false ||
                codeOffset != 0)
            {
                throw new InvalidOperationException();
            }
            return BitConverter.ToInt32(bytes, 17);
        }

        public static bool GetValueLoadOffsets(
            RuntimeProcess runtime,
            IntPtr functionPointer,
            out (int structOffset, int struct2Offset, int fieldOffset, LoadType type) info)
        {
            var bytes = runtime.ReadBytes(functionPointer, _MaxLoadSize);
            foreach (var loadPattern in _LoadPatterns)
            {
                if (ByteSearch.Match(bytes, loadPattern.Pattern, out var codeOffset) == false || codeOffset != 0)
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
                var structOffset = Read(bytes, loadPattern.StructOffsetOffset, loadPattern.StructOffsetSize);
                var struct2Offset = Read(bytes, loadPattern.Struct2OffsetOffset, loadPattern.Struct2OffsetSize);
                var fieldOffset = Read(bytes, loadPattern.FieldOffsetOffset, loadPattern.FieldOffsetSize);
                info = (structOffset, struct2Offset, fieldOffset, loadPattern.Type);
                return true;
            }
            info = default;
            return false;
        }
    }
}
