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
using StarfieldDumping;

namespace DumpReflection
{
    internal static class Helpers
    {
        private static int _ReturnMaxCount;
        private static readonly ByteSearch.Pattern _ReturnZeroPattern;
        private static readonly ByteSearch.Pattern _ReturnPointerPattern;

        static Helpers()
        {
            _ReturnZeroPattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x33, 0xC0, 0xC3 },
            };
            _ReturnPointerPattern = new ByteSearch.Pattern()
            {
                new byte[] { 0x48, 0x8D, 0x05, },
                ByteSearch.AnyBytes(4),
                new byte[] { 0xC3 },
            };
            _ReturnMaxCount = Math.Max(_ReturnZeroPattern.Count, _ReturnPointerPattern.Count);
        }

        private static bool MatchReturnZero(byte[] bytes)
        {
            return ByteSearch.Match(bytes, _ReturnZeroPattern, out var matchOffset) == true && matchOffset == 0;
        }

        private static bool MatchReturnPointer(byte[] bytes, out int offset)
        {
            if (ByteSearch.Match(bytes, _ReturnPointerPattern, out int matchOffset) == false || matchOffset != 0)
            {
                offset = default;
                return false;
            }

            var rva = BitConverter.ToInt32(bytes, 3);
            offset = 3 + 4 + rva;
            return true;
        }

        public static IntPtr GetPointerFromVftable(RuntimeProcess runtime, IntPtr vftablePointer, int functionIndex)
        {
            var functionPointer = runtime.ReadPointer(vftablePointer + (8 * functionIndex));
            if (functionPointer == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr resultPointer;
            var bytes = runtime.ReadBytes(functionPointer, _ReturnMaxCount);
            if (MatchReturnPointer(bytes, out var offset) == true)
            {
                resultPointer = functionPointer + offset;
            }
            else if (MatchReturnZero(bytes) == true)
            {
                resultPointer = IntPtr.Zero;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return resultPointer;
        }

        public static string GetStringFromVftable(RuntimeProcess runtime, IntPtr vftablePointer, int functionIndex)
        {
            var stringPointer = GetPointerFromVftable(runtime, vftablePointer, functionIndex);
            return runtime.ReadStringZ(stringPointer, Encoding.ASCII);
        }
    }
}
