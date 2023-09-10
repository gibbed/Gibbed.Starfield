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
using System.Text;
using Gibbed.IO;
using Gibbed.Starfield.FileFormats.ComponentDatabase;

namespace Gibbed.Starfield.FileFormats
{
    public partial class ComponentDatabaseFile
    {
        private const uint Signature = 0x48544542; // 'BETH'

        private readonly List<object> _Instances;

        public ComponentDatabaseFile()
        {
            this._Instances = new();
        }

        public List<object> Instances => this._Instances;

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            // 'BETH' "header" is technically a chunk itself, but treating it as special here anyway

            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var headerSize = input.ReadValueU32(endian);
            if (headerSize != 8)
            {
                throw new FormatException();
            }

            var fileVersion = input.ReadValueU32(endian);
            if (fileVersion != 4)
            {
                throw new FormatException();
            }

            var chunkCount = input.ReadValueU32(endian);
            if (chunkCount < 1)
            {
                throw new FormatException();
            }
            chunkCount--; // count includes BETH chunk

            Queue<Chunk> chunkQueue = new();
            for (int i = 0; i < chunkCount; i++)
            {
                var chunkType = (ChunkType)input.ReadValueU32(endian);
                var chunkSize = input.ReadValueU32(endian);
                var chunkPosition = input.Position;
                input.Position += chunkSize;
                chunkQueue.Enqueue(new(chunkType, chunkPosition, chunkSize));
            }

            Dictionary<int, Class> typeMap = new();

            DeserializeState state = new()
            {
                Stream = input,
                Endian = endian,
                ChunkQueue = chunkQueue,
                TypeMap = typeMap,
            };

            using var stringTable = state.StringTable = ConsumeStringTable(state);

            var typeCount = ConsumeTypeCount(state);
            var types = new Class[typeCount];
            for (uint i = 0; i < typeCount; i++)
            {
                var type = types[i] = ConsumeClass(state);
                typeMap.Add(type.NameOffset, type);
            }

            List<object> instances = new();
            while (chunkQueue.Count > 0)
            {
                var chunkType = chunkQueue.Peek().Type;
                var instance = chunkType switch
                {
                    ChunkType.OBJT => ConsumeObject(state),
                    ChunkType.USER => ConsumeObject(state),
                    ChunkType.DIFF => ConsumeObject(state),
                    ChunkType.USRD => ConsumeObject(state),
                    ChunkType.MAPC => ConsumeMap(state, false),
                    ChunkType.LIST => ConsumeList(state, false),
                    _ => throw new FormatException($"unexpected {chunkType} chunk"),
                };
                instances.Add(instance);
            }

            this._Instances.AddRange(instances);
        }

        private static StringTable ConsumeStringTable(DeserializeState state)
        {
            return new(state.ConsumeChunk(ChunkType.STRT));
        }

        private static uint ConsumeTypeCount(DeserializeState state)
        {
            var bytes = state.ConsumeChunk(ChunkType.TYPE);
            if (bytes.Length != 4)
            {
                throw new FormatException();
            }
            using MemoryStream data = new(bytes, false);
            return data.ReadValueU32(state.Endian);
        }

        private static Class ConsumeClass(DeserializeState state)
        {
            var endian = state.Endian;

            var bytes = state.ConsumeChunk(ChunkType.CLAS);
            using MemoryStream input = new(bytes, false);

            var nameOffset = input.ReadValueS32(endian);
            var name = state.StringTable.Get(nameOffset);
            var classTypeId = input.ReadValueU32(endian);
            var flags = (ClassFlags)input.ReadValueU16(endian);
            var fieldCount = input.ReadValueU16(endian);

            var knownFlags = ClassFlags.IsUser | ClassFlags.IsStruct;
            var unknownFlags = flags & ~knownFlags;
            if (unknownFlags != ClassFlags.None)
            {
                throw new FormatException();
            }

            var fields = new Field[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                Field field;
                field.Name = state.ReadString(input);
                field.Type = new(input.ReadValueS32(endian));
                field.Offset = input.ReadValueU16(endian);
                field.Size = input.ReadValueU16(endian);
                fields[i] = field;
            }

            Class instance = new()
            {
                NameOffset = nameOffset,
                Name = name,
                TypeId = classTypeId,
                Flags = flags,
            };
            instance.Fields.AddRange(fields);

            if (input.Position != input.Length)
            {
                throw new FormatException();
            }

            return instance;
        }

        private object ConsumeObject(DeserializeState state)
        {
            var chunkType = state.ChunkQueue.Peek().Type;
            var (isCast, isDiff) = chunkType switch
            {
                ChunkType.OBJT => (false, false),
                ChunkType.USER => (true, false),
                ChunkType.DIFF => (false, true),
                ChunkType.USRD => (true, true),
                _ => throw new FormatException($"wanted OBJT/USER/DIFF/USRD chunk, got {chunkType} chunk"),
            };
            return ConsumeObject(state, chunkType, isCast, isDiff);
        }

        private object ConsumeObject(DeserializeState state, ChunkType chunkType, bool isCast, bool isDiff)
        {
            var endian = state.Endian;
            var bytes = state.ConsumeChunk(chunkType);
            using MemoryStream input = new(bytes, false);

            TypeReference typeRef;
            TypeReference targetTypeRef;
            if (isCast == false)
            {
                typeRef = new(input.ReadValueS32(endian));
                targetTypeRef = typeRef;
            }
            else
            {
                targetTypeRef = new(input.ReadValueS32(endian));
                typeRef = new(input.ReadValueS32(endian));
            }

            var instance = ReadValue(state, typeRef, input, isDiff);

            uint unknown = isCast == true ? input.ReadValueU32(endian) : 0;

            if (input.Position != input.Length)
            {
                throw new FormatException();
            }

            return instance;
        }

        private object ReadValue(DeserializeState state, TypeReference typeRef, Stream input, bool isDiff)
        {
            if (typeRef.Id < 0)
            {
                var builtinType = (BuiltinType)typeRef.Id;
                if (IsChunk(builtinType) == false)
                {
                    return ReadPrimitive(state, builtinType, input, isDiff);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            var endian = state.Endian;
            var type = state.TypeMap[typeRef.Id];

            Dictionary<string, object> fieldValues = new();

            List<Field> chunkFields = new();

            if (isDiff == false)
            {
                foreach (var field in type.Fields)
                {
                    if (IsChunk(field.Type, state) == false)
                    {
                        var fieldValue = ReadValue(state, field.Type, input, isDiff);
                        fieldValues.Add(field.Name, fieldValue);
                    }
                    else
                    {
                        chunkFields.Add(field);
                    }
                }
            }
            else
            {
                while (true)
                {
                    var fieldIndex = input.ReadValueU16(endian);
                    if (fieldIndex == 0xFFFF)
                    {
                        break;
                    }
                    var field = type.Fields[fieldIndex];

                    if (IsChunk(field.Type, state) == false)
                    {
                        var fieldValue = ReadValue(state, field.Type, input, isDiff);
                        fieldValues.Add(field.Name, fieldValue);
                    }
                    else
                    {
                        chunkFields.Add(field);
                    }
                }
            }

            foreach (var field in chunkFields)
            {
                object fieldValue;
                if (field.Type.Id < 0)
                {
                    fieldValue = (BuiltinType)field.Type.Id switch
                    {
                        BuiltinType.Map => ConsumeMap(state, isDiff),
                        BuiltinType.List => ConsumeList(state, isDiff),
                        _ => throw new NotSupportedException(),
                    };
                }
                else
                {
                    fieldValue = ConsumeObject(state);
                }
                fieldValues.Add(field.Name, fieldValue);
            }

            ObjectInstance instance = new()
            {
                Type = type,
            };
            foreach (var kv in fieldValues)
            {
                instance.Fields.Add(kv.Key, kv.Value);
            }
            return instance;
        }

        private object ConsumeList(DeserializeState state, bool isDiff)
        {
            var endian = state.Endian;
            var bytes = state.ConsumeChunk(ChunkType.LIST);
            using MemoryStream input = new(bytes, false);

            TypeReference typeRef = new(input.ReadValueS32(endian));
            var itemCount = input.ReadValueS32(endian);

            List<object> list = new();
            for (int i = 0; i < itemCount; i++)
            {
                var item = ReadValue(state, typeRef, input, isDiff);
                list.Add(item);
            }

            if (input.Position != input.Length)
            {
                throw new FormatException();
            }

            return list;
        }

        private object ConsumeMap(DeserializeState state, bool isDiff)
        {
            var endian = state.Endian;
            var bytes = state.ConsumeChunk(ChunkType.MAPC);
            using MemoryStream input = new(bytes, false);

            TypeReference keyTypeRef = new(input.ReadValueS32(endian));
            TypeReference valueTypeRef = new(input.ReadValueS32(endian));
            var itemCount = input.ReadValueS32(endian);

            Dictionary<object, object> map = new();
            for (int i = 0; i < itemCount; i++)
            {
                var key = ReadValue(state, keyTypeRef, input, isDiff);
                var value = ReadValue(state, valueTypeRef, input, isDiff);
                map.Add(key, value);
            }

            if (input.Position != input.Length)
            {
                throw new FormatException();
            }

            return map;
        }

        private bool IsChunk(TypeReference type, DeserializeState state) => type.Id switch
        {
            < 0 => IsChunk((BuiltinType)type.Id),
            _ => (state.TypeMap[type.Id].Flags & ClassFlags.IsUser) != 0,
        };

        private bool IsChunk(BuiltinType type) => type switch
        {
            BuiltinType.List => true,
            BuiltinType.Map => true,
            _ => false,
        };

        private object ReadPrimitive(DeserializeState state, BuiltinType type, Stream input, bool isDiff) => type switch
        {
            BuiltinType.Null => null,
            BuiltinType.String => ReadPrimitiveString(input, state.Endian),
            BuiltinType.List => null,
            BuiltinType.Map => null,
            BuiltinType.Ref => ReadPrimitiveRef(state, input, isDiff),
            BuiltinType.Int8 => input.ReadValueS8(),
            BuiltinType.UInt8 => input.ReadValueU8(),
            BuiltinType.Int16 => input.ReadValueS16(state.Endian),
            BuiltinType.UInt16 => input.ReadValueU16(state.Endian),
            BuiltinType.Int32 => input.ReadValueS32(state.Endian),
            BuiltinType.UInt32 => input.ReadValueU32(state.Endian),
            BuiltinType.Int64 => input.ReadValueS64(state.Endian),
            BuiltinType.UInt64 => input.ReadValueU64(state.Endian),
            BuiltinType.Bool => input.ReadValueB8(),
            BuiltinType.Float => input.ReadValueF32(state.Endian),
            BuiltinType.Double => input.ReadValueF64(state.Endian),
            _ => throw new NotSupportedException(),
        };

        private object ReadPrimitiveRef(DeserializeState state, Stream input, bool isDiff)
        {
            var typeId = input.ReadValueS32(state.Endian);
            if (typeId < 0)
            {
                return ReadPrimitive(state, (BuiltinType)typeId, input, isDiff);
            }

            var type = state.TypeMap[typeId];

            if ((type.Flags & ClassFlags.IsUser) != 0)
            {
                return ConsumeObject(state);
                //return new TypeReference(typeId);
            }

            return ReadValue(state, new(typeId), input, isDiff);
        }

        private string ReadPrimitiveString(Stream input, Endian endian)
        {
            var length = input.ReadValueU16(endian);
            return input.ReadString(length, true, Encoding.ASCII);
        }
    }
}
