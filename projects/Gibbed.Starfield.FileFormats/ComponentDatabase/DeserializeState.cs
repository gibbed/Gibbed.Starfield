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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Starfield.FileFormats.ComponentDatabase
{
    internal class DeserializeState
    {
        public Stream Stream;
        public Endian Endian;
        public Queue<Chunk> ChunkQueue;
        public StringTable StringTable;
        public Dictionary<int, Class> TypeMap;

        public string ReadString(Stream input)
        {
            var offset = input.ReadValueS32(this.Endian);
            return this.StringTable.Get(offset);
        }

        public byte[] ConsumeChunk(ChunkType type)
        {
            var chunk = this.ChunkQueue.Dequeue();
            if (chunk.Type != type)
            {
                throw new FormatException($"wanted {type} chunk, got {chunk.Type} chunk");
            }
            var input = this.Stream;
            input.Position = chunk.Position;
            return input.ReadBytes((int)chunk.Size);
        }
    }
}
