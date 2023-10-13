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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.AddressLibrary
{
    public class AddressLibrary : IReadOnlyDictionary<ulong, ulong>
    {
        private Dictionary<ulong, ulong> _Entries;

        internal AddressLibrary(Version fileVersion, string fileName, uint pointerSize, Dictionary<ulong, ulong> entries)
        {
            this._Entries = entries ?? throw new ArgumentNullException(nameof(entries));
            this.FileVersion = fileVersion;
            this.FileName = fileName;
            this.PointerSize = pointerSize;
        }

        public Version FileVersion { get; }
        public string FileName { get; }
        public uint PointerSize { get; }

        public ulong this[ulong key] => this._Entries[key];
        public IEnumerable<ulong> Keys => this._Entries.Keys;
        public IEnumerable<ulong> Values => this._Entries.Values;
        public int Count => this._Entries.Count;

        public static AddressLibrary Load(Stream input)
        {
            const Endian endian = Endian.Little;

            var formatVersion = input.ReadValueU32(endian);
            if (formatVersion != 2)
            {
                throw new FormatException($"unexpected format version {formatVersion}");
            }

            var fileVersion = ReadVersion(input, endian);
            var fileNameLength = input.ReadValueU32(endian);
            var fileName = input.ReadString((int)fileNameLength, Encoding.UTF8);
            var pointerSize = input.ReadValueU32(endian);
            var entryCount = input.ReadValueU32(endian);

            ulong previousId = 0;
            ulong previousOffset = 0;
            Dictionary<ulong, ulong> entries = new();
            for (uint i = 0; i < entryCount; i++)
            {
                var flags = input.ReadValueU8();

                var idType = (byte)((flags >> 0) & 0xF);
                var offsetType = (byte)((flags >> 4) & 0x7);
                var isPointer = (flags & 0x80) != 0;

                ulong id = idType switch
                {
                    0 => input.ReadValueU64(endian),
                    1 => previousId + 1,
                    2 => previousId + input.ReadValueU8(),
                    3 => previousId - input.ReadValueU8(),
                    4 => previousId + input.ReadValueU16(endian),
                    5 => previousId - input.ReadValueU16(endian),
                    6 => input.ReadValueU16(endian),
                    7 => input.ReadValueU32(endian),
                    _ => throw new FormatException($"unexpected id type {idType}"),
                };

                if (isPointer == true)
                {
                    previousOffset /= pointerSize;
                }

                ulong offset = offsetType switch
                {
                    0 => input.ReadValueU64(endian),
                    1 => previousOffset + 1,
                    2 => previousOffset + input.ReadValueU8(),
                    3 => previousOffset - input.ReadValueU8(),
                    4 => previousOffset + input.ReadValueU16(endian),
                    5 => previousOffset - input.ReadValueU16(endian),
                    6 => input.ReadValueU16(endian),
                    7 => input.ReadValueU32(endian),
                    _ => throw new FormatException($"unexpected offset type {idType}"),
                };

                if (isPointer == true)
                {
                    offset *= pointerSize;
                }

                entries.Add(id, offset);

                previousId = id;
                previousOffset = offset;
            }

            return new(fileVersion, fileName, pointerSize, entries);
        }

        private static Version ReadVersion(Stream input, Endian endian)
        {
            var major = input.ReadValueS32(endian);
            var minor = input.ReadValueS32(endian);
            var build = input.ReadValueS32(endian);
            var revision = input.ReadValueS32(endian);
            return new(major, minor, build, revision);
        }

        public bool ContainsKey(ulong key)
        {
            return this._Entries.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<ulong, ulong>> GetEnumerator()
        {
            return this._Entries.GetEnumerator();
        }

        public bool TryGetValue(ulong key, out ulong value)
        {
            return this._Entries.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._Entries.GetEnumerator();
        }
    }
}
