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

//#define DUMP_ALL_PARAM_NAMES

using System;
#if DUMP_ALL_PARAM_NAMES
using System.Collections.Generic;
using System.Linq;
#endif
using System.Text;
using Gibbed.AddressLibrary;
using StarfieldDumping;
using System.Runtime.InteropServices;
using System.IO;

namespace DumpConditionFunctions
{
    internal static partial class Program
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
            TextWriter writer = Console.Out;

            var mainModuleBaseAddressValue = runtime.Process.MainModule.BaseAddress.ToInt64();
            IntPtr Id2Pointer(ulong id)
            {
                var offset = (long)addressLibrary[id];
                return new(mainModuleBaseAddressValue + offset);
            }

            var commandSize = Marshal.SizeOf(typeof(Natives.Command));
            var commandParamSize = Marshal.SizeOf(typeof(Natives.CommandParam));

            var commandsPointer = Id2Pointer(841467);
            // TODO(gibbed): pattern match this function to make sure this is actually where the count is
            int commandCount = runtime.ReadValueS32(Id2Pointer(13358) + 18);
            var commandPointer = commandsPointer;
            var commands = new Natives.Command[commandCount];
            for (int i = 0; i < commandCount; i++, commandPointer += commandSize)
            {
                commands[i] = runtime.ReadStructure<Natives.Command>(commandPointer);
            }

#if DUMP_ALL_PARAM_NAMES
            Dictionary<uint, List<string>> allParamNamesMap = new();
#endif

            uint? lastOpcode = null;
            for (int i = 0, commandId = 0; i < commandCount; i++)
            {
                var command = commands[i];
                if (command.Eval == IntPtr.Zero)
                {
                    continue;
                }

                if (lastOpcode != null && lastOpcode + 1 != command.Opcode)
                {
                    writer.WriteLine();
                }
                lastOpcode = command.Opcode;

                int commandParamCount = Math.Min(3, (int)command.NumParams);
                var commandParams = new Natives.CommandParam[commandParamCount];
                var commandParamPointer = command.Params;
                for (int j = 0; j < commandParamCount; j++, commandParamPointer += commandParamSize)
                {
                    commandParams[j] = runtime.ReadStructure<Natives.CommandParam>(commandParamPointer);
                }

                var commandParamNames = new string[commandParamCount];
                for (int j = 0; j < commandParamCount; j++)
                {
                    commandParamNames[j] = runtime.ReadStringZ(commandParams[j].TypeName, Encoding.ASCII);
                }

                var longName = runtime.ReadStringZ(command.LongName, Encoding.ASCII);
                //var shortName = runtime.ReadStringZ(command.ShortName, Encoding.ASCII);

                // (Index:   1; Name: 'GetDistance'; ParamType1: ptObjectReference),

                StringBuilder sb = new();
                sb.Append($"(Index: {i,3:D}, Name: '{longName}'");
                for (int j = 0; j < commandParamCount; j++)
                {
                    var param = commandParams[j];

#if DUMP_ALL_PARAM_NAMES
                    if (allParamNamesMap.TryGetValue(param.TypeId, out var allParamNames) == false)
                    {
                        allParamNamesMap[param.TypeId] = allParamNames = new();
                    }

                    if (allParamNames.Contains(commandParamNames[j]) == false)
                    {
                        allParamNames.Add(commandParamNames[j]);
                    }
#endif

                    var paramTypeName = ParamTypeNames.Get(param.TypeId);
                    sb.Append($", ParamType{1 + j}: pt{paramTypeName}");
                }
                sb.Append("),");

                writer.WriteLine($"{sb}    //   {commandId}");

                commandId++;
            }

#if DUMP_ALL_PARAM_NAMES
            writer.WriteLine();
            writer.WriteLine();

            foreach (var kv in allParamNamesMap.OrderBy(kv => kv.Key))
            {
                writer.WriteLine($"ptUnknown{kv.Key:X2}:");
                foreach (var paramName in kv.Value.OrderBy(v => v))
                {
                    writer.WriteLine($"  {paramName}");
                }
            }
#endif

            return 0;
        }
    }
}
