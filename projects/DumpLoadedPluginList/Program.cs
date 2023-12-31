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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.AddressLibrary;
using StarfieldDumping;

namespace DumpLoadedPluginList
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Environment.ExitCode = DumpingHelpers.Main(args, Dump);
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                Console.ReadLine();
            }
        }

        private static int Dump(RuntimeProcess runtime, AddressLibrary addressLibrary, string[] args)
        {
            var mainModuleBaseAddressValue = runtime.Process.MainModule.BaseAddress.ToInt64();
            IntPtr Id2Pointer(ulong id)
            {
                var offset = (long)addressLibrary[id];
                return new(mainModuleBaseAddressValue + offset);
            }

            var dataHandlerPointerPointer = Id2Pointer(825890);
            var dataHandlerPointer = runtime.ReadPointer(dataHandlerPointerPointer);
            if (dataHandlerPointer == null)
            {
                Console.WriteLine("Starfield data handler is not yet initialized.");
                return -1;
            }

            List<IntPtr> pluginPointers = new();
            var pluginListEntryPointer = dataHandlerPointer + 0x14F0;
            while (pluginListEntryPointer != IntPtr.Zero)
            {
                var pluginPointer = runtime.ReadPointer(pluginListEntryPointer + 0);
                if (pluginPointer == IntPtr.Zero)
                {
                    break;
                }
                pluginPointers.Add(pluginPointer);
                pluginListEntryPointer = runtime.ReadPointer(pluginListEntryPointer + 8);
            }

            List<PluginInfo> pluginInfos = new();
            Dictionary<IntPtr, PluginInfo> pluginInfoMap = new();
            foreach (var pluginPointer in pluginPointers)
            {
                var fileName = runtime.ReadString(pluginPointer + 0x038, 260, Encoding.Default);
                var flags = runtime.ReadValueU32(pluginPointer + 0x1B8);

                var id = runtime.ReadValueU8(pluginPointer + 0x208);
                var lightId = runtime.ReadValueU16(pluginPointer + 0x20A);

                var masterFullList = ReadPointerList(runtime, pluginPointer + 0x1D0);
                var masterNormalList = ReadPointerList(runtime, pluginPointer + 0x1E0);
                var masterLightList = ReadPointerList(runtime, pluginPointer + 0x1F0);

                PluginInfo pluginInfo = new()
                {
                    Pointer = pluginPointer,
                    FileName = fileName,
                    Flags = flags,
                    Id = id,
                    LightId = lightId,
                    MasterFullList = masterFullList,
                    MasterNormalList = masterNormalList,
                    MasterLightList = masterLightList,
                };

                pluginInfos.Add(pluginInfo);
                pluginInfoMap.Add(pluginPointer, pluginInfo);
            }

            Dictionary<IntPtr, PluginNode> pluginNodes = new();
            foreach (var pluginInfo in pluginInfos.Where(pi => (pi.Flags & 4) != 0))
            {
                pluginNodes.Add(pluginInfo.Pointer, new()
                {
                    Info = pluginInfo,
                });
            }
            foreach (var pluginNode in pluginNodes.Values)
            {
                foreach (var masterPointer in pluginNode.Info.MasterFullList)
                {
                    if (pluginNodes.TryGetValue(masterPointer, out var masterNode) == false)
                    {
                        continue;
                    }
                    pluginNode.Parents.Add(masterNode);
                    masterNode.Children.Add(pluginNode);
                }
            }

            Console.WriteLine("# PLUGINS");
            foreach (var pluginInfo in pluginInfos)
            {
                string idString = "", loadedMarker = "";
                if ((pluginInfo.Flags & 4) != 0)
                {
                    idString = pluginInfo.IdString;
                    loadedMarker = "*";
                }

                Console.Write($"{idString,5} ");
                Console.Write($"{loadedMarker}{pluginInfo.FileName}");
                //Console.Write($" {pluginInfo.Flags:X}");
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("# LOADED DEPENDENCY TREE");
            Stack<(PluginNode node, int depth)> pluginNodeQueue = new();
            foreach (var pluginNode in pluginNodes.Values.Where(pn => pn.Parents.Count == 0))
            {
                pluginNodeQueue.Push((pluginNode, 0));
            }
            while (pluginNodeQueue.Count > 0)
            {
                var (parent, depth) = pluginNodeQueue.Pop();

                var padding = "".PadLeft(depth * 2, ' ');

                Console.WriteLine($"{parent.Info.IdString,5}{padding} {parent.Info.FileName}");
                
                foreach (var child in ((IEnumerable<PluginNode>)parent.Children).Reverse())
                {
                    pluginNodeQueue.Push((child, depth + 1));
                }
            }
            //Console.WriteLine();

            return 0;
        }

        private static IntPtr[] ReadPointerList(RuntimeProcess runtime, IntPtr listPointer)
        {
            var count = runtime.ReadValueU32(listPointer + 0);
            var entries = new IntPtr[count];
            if (count > 0)
            {
                var entryPointer = runtime.ReadPointer(listPointer + 8);
                for (uint i = 0; i < count; i++, entryPointer += 8)
                {
                    entries[i] = runtime.ReadPointer(entryPointer);
                }
            }
            return entries;
        }

        private class PluginNode
        {
            public PluginInfo Info;
            public readonly List<PluginNode> Parents = new();
            public readonly List<PluginNode> Children = new();
        }

        private struct PluginInfo
        {
            public IntPtr Pointer;
            public string FileName;
            public uint Flags;
            public byte Id;
            public ushort LightId;
            public IntPtr[] MasterFullList;
            public IntPtr[] MasterNormalList;
            public IntPtr[] MasterLightList;

            public string IdString
            {
                get
                {
                    return (this.Flags & 0x100) == 0
                        ? $"{this.Id:X02}"
                        : $"{this.Id:X02}{this.LightId:X03}";
                }
            }
        }
    }
}
