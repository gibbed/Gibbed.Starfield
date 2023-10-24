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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gibbed.AddressLibrary;
using NDesk.Options;
using StarfieldDumping;
using LoadType = DumpModProperties.LoadPatternHelpers.LoadType;

namespace DumpModProperties
{
    internal static class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            Environment.ExitCode = MainInternal(args);
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                Console.ReadLine();
            }
        }

        public static int MainInternal(string[] args)
        {
            bool showHelp = false;

            OptionSet options = new()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;
            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return -1;
            }

            if (extras.Count < 0 || extras.Count > 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ [output_directory]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return -2;
            }

            var outputBasePath = extras.Count > 0
                ? extras[0]
                : "mod properties";

            outputBasePath = Path.GetFullPath(outputBasePath);

            var result = DumpingHelpers.Main(outputBasePath, Dump);
            return result < 0 ? -3 + result : result;
        }

        private static int Dump(RuntimeProcess runtime, AddressLibrary addressLibrary, string outputBasePath)
        {
            var mainModuleBaseAddressValue = runtime.Process.MainModule.BaseAddress.ToInt64();
            IntPtr Id2Pointer(ulong id)
            {
                var offset = (long)addressLibrary[id];
                return new(mainModuleBaseAddressValue + offset);
            }
            var addressLibraryPointer2Id = addressLibrary.Invert();
            ulong Pointer2Id(IntPtr pointer)
            {
                var offset = (ulong)(pointer.ToInt64() - mainModuleBaseAddressValue);
                return addressLibraryPointer2Id[offset];
            }

            var getPropertyFunctionInfos = new (string typeName, string className, ulong functionId, Func<int, int, (string, string)> getName)[]
            {
                ("Spaceship", "Spaceship::InstanceData", 98649, null),
                ("FLOR", "TESFloraInstanceData", 102909, null),
                ("FURN", "TESFurniture::InstanceData", 102985, null),
                ("ARMO", "TESObjectARMOInstanceData", 103446, null),
                ("CONT", "TESObjectCONT::InstanceData", 103582, null),
                ("WEAP", "TESObjectWEAPInstanceData", 103869, GetWeaponName),
                ("NPC_", "TESNPC::InstanceData", 112007, null),
            };

            var propertySize = Marshal.SizeOf(typeof(Natives.ModProperty));

            foreach (var (typeName, className, getPropertyFunctionId, getName) in getPropertyFunctionInfos)
            {
                var getPropertyFunctionPointer = Id2Pointer(getPropertyFunctionId);

                if (GetModPropertyPatternHelpers.Match(runtime, getPropertyFunctionPointer, out var getPropertyMatchInfo) == false)
                {
                    Console.WriteLine($"Failed to match get mod property function for '{className}' ({getPropertyFunctionId})!");
                    continue;
                }
                var (propertyCount, propertyTablePointer) = getPropertyMatchInfo;

                var properties = new Natives.ModProperty[propertyCount];
                var propertyPointer = propertyTablePointer;
                for (int i = 0; i < propertyCount; i++, propertyPointer += propertySize)
                {
                    properties[i] = runtime.ReadStructure<Natives.ModProperty>(propertyPointer);
                }

                List<(string propertyId, string propertyType, string propertyName, string structName, string struct2Name, string fieldName, string settingName)> lines = new();
                foreach (var property in properties)
                {
                    var propertyId = Encoding.ASCII.GetString(BitConverter.GetBytes(property.Id));

                    var propertyName = property.Name == IntPtr.Zero
                        ? "(null)"
                        : runtime.ReadStringZ(property.Name, Encoding.ASCII);

                    string propertyType, structName, struct2Name, fieldName, gameSettingName;

                    if (property.CopyCallback == IntPtr.Zero)
                    {
                        propertyType = structName = struct2Name = fieldName = "";
                    }
                    else
                    {
                        if (LoadPatternHelpers.Match(runtime, property.CopyCallback, out var loadInfo) == false)
                        {
                            var copyCallbackId = Pointer2Id(property.CopyCallback);
                            Console.WriteLine($"Failed to match property copy callback {Pointer2Id(property.CopyCallback)}!");
                            continue;
                        }

                        propertyType = GetTypeName(loadInfo.type, propertyName);
                        (structName, struct2Name) = getName == null
                            ? (ToOffsetString(loadInfo.structOffset), ToOffsetString(loadInfo.struct2Offset))
                            : GetWeaponName(loadInfo.structOffset, loadInfo.struct2Offset);
                        fieldName = ToOffsetString(loadInfo.fieldOffset);
                    }

                    if (property.ValueGameSetting == IntPtr.Zero)
                    {
                        gameSettingName = "";
                    }
                    else
                    {
                        var gameSettingNamePointer = runtime.ReadPointer(property.ValueGameSetting + 0x18);
                        gameSettingName = gameSettingNamePointer == IntPtr.Zero
                            ? "(null)"
                            : runtime.ReadStringZ(gameSettingNamePointer, Encoding.ASCII);
                    }

                    lines.Add((propertyId, propertyType, propertyName, structName ?? "", struct2Name ?? "", fieldName, gameSettingName));
                }

                string output;
                using (var writer = new StringWriter())
                {
                    writer.WriteLine("Id,Type,Property Name,Struct 1,Struct 2,Field,Value Game Setting");
                    foreach (var line in lines
                        .OrderBy(l => l.structName)
                        .ThenBy(l => l.struct2Name)
                        .ThenBy(l => l.fieldName)
                        .ThenBy(l => l.propertyId))
                    {
                        writer.WriteLine($"{line.propertyId},{line.propertyType},{line.propertyName},{line.structName},{line.struct2Name},{line.fieldName},{line.settingName}");
                    }
                    writer.Flush();
                    output = writer.ToString();
                }

                var outputPath = Path.Combine(outputBasePath, $"{typeName}.csv");

                var outputParentPath = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrEmpty(outputParentPath) == false)
                {
                    Directory.CreateDirectory(outputParentPath);
                }

                File.WriteAllText(outputPath, output);
            }

            return 0;
        }

        private static string ToOffsetString(int offset)
        {
            return offset < 0
                ? ""
                : $"+0x{offset:X}";
        }

        private static string GetTypeName(LoadType type, string name) => type switch
        {
            LoadType.UInt8 => GetUInt8Name(name),
            LoadType.Int16 => GetInt16Name(name),
            LoadType.UInt16 => GetUInt16Name(name),
            LoadType.UInt32 => GetUInt32Name(name),
            LoadType.Single => "float",
            LoadType.SingleToInt32 => "float (cast to int32)",
            LoadType.FormId => "formid",
            LoadType.FormIdAndInt32 => "formid+int32",
            _ => throw new NotSupportedException(),
        };

        private static string GetUInt8Name(string name) => name[0] switch
        {
            'b' => "bool",
            'c' => "int8",
            'u' => "uint8",
            'e' => "uint8",
            _ => throw new NotImplementedException(),
        };

        private static string GetInt16Name(string name) => name[0] switch
        {
            'i' => "int16",
            _ => throw new NotImplementedException(),
        };

        private static string GetUInt16Name(string name) => name[0] switch
        {
            'i' => "int16",
            _ => throw new NotImplementedException(),
        };

        private static string GetUInt32Name(string name) => name[0] switch
        {
            'i' => "int32",
            'u' => "uint32",
            'e' => "uint32",
            _ => throw new NotImplementedException(),
        };

        private static (string name, string name2) GetWeaponName(int offset, int offset2) => offset switch
        {
            -1 => (null, GetInvalidName(offset2)),
            0x18 => ("WAIM", GetWeaponAimName(offset2)),
            0x20 => ("WAMM", GetInvalidName(offset2)),
            0x28 => ("WAUD", GetInvalidName(offset2)),
            0x30 => ("WCHG", GetInvalidName(offset2)),
            0x38 => ("WDMG", GetInvalidName(offset2)),
            0x40 => ("WFIR", GetInvalidName(offset2)),
            0x48 => ("WFLG", GetInvalidName(offset2)),
            0x50 => ("WGEN", GetInvalidName(offset2)),
            0x58 => ("WMEL", GetInvalidName(offset2)),
            0x60 => ("QNAM", GetInvalidName(offset2)),
            0x68 => ("WRLO", GetInvalidName(offset2)),
            0x78 => ("WVAR", GetInvalidName(offset2)),
            0x80 => ("WVIS", GetInvalidName(offset2)),
            _ => throw new NotSupportedException(),
        };

        private static string GetWeaponAimName(int offset) => offset switch
        {
            -1 => null,
            0x20 => "pzAimDownSightTemplate",
            0x30 => "pdAimModelTemplate",
            0x48 => "pbAimAssistTemplate",
            _ => throw new NotSupportedException(),
        };

        private static string GetInvalidName(int offset) => offset switch
        {
            -1 => null,
            _ => throw new NotSupportedException(),
        };
    }
}
