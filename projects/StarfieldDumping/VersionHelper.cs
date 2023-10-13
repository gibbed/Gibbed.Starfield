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

namespace StarfieldDumping
{
    internal class VersionHelper
    {
        private const string _DllName = "version";

        [DllImport(_DllName, EntryPoint = "GetFileVersionInfoSizeExW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint GetFileVersionInfoSizeEx(uint flags, string filename, out uint handle);

        [DllImport(_DllName, EntryPoint = "GetFileVersionInfoExW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileVersionInfoEx(uint flags, string filename, uint handle, uint size, IntPtr data);

        [DllImport(_DllName, EntryPoint = "VerQueryValueW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VerQueryValue(IntPtr block, string subblock, out IntPtr buffer, out uint size);

        public static string GetProductVersion(string path)
        {
            return GetProductVersionInternal(path);
        }

        private static unsafe string GetProductVersionInternal(string path)
        {
            var size = GetFileVersionInfoSizeEx(0, path, out var handle);
            if (size != 0)
            {
                var dataBytes = new byte[size];
                fixed (byte* dataBuffer = &dataBytes[0])
                {
                    var dataPointer = new IntPtr(dataBuffer);
                    if (GetFileVersionInfoEx(0, path, 0u, size, dataPointer) == true &&
                        GetTranslationValue(dataPointer, out var translation) == true &&
                        GetProductVersion(dataPointer, translation, out var productVersion) == true)
                    {
                        return productVersion;
                    }
                }
            }
            return "";
        }

        private static bool GetTranslationValue(IntPtr data, out uint value)
        {
            if (VerQueryValue(data, @"\VarFileInfo\Translation", out var pointer, out var size) == false)
            {
                value = 0;
                return false;
            }

            if (size != 4)
            {
                value = 0;
                return false;
            }

            value = ((uint)Marshal.ReadInt16(pointer) << 16) | (ushort)Marshal.ReadInt16((IntPtr)((long)pointer + 2));
            return true;
        }

        private static bool GetProductVersion(IntPtr data, uint codepage, out string value)
        {
            if (VerQueryValue(data, $@"\StringFileInfo\{codepage:X8}\ProductVersion", out var pointer, out var size) == false)
            {
                value = null;
                return false;
            }

            if (size == 0)
            {
                value = "";
                return true;
            }

            value = Marshal.PtrToStringUni(pointer);
            return true;
        }
    }
}
