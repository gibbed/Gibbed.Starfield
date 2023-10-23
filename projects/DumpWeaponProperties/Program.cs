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

namespace DumpWeaponProperties
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ [output_csv]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return -2;
            }

            var outputPath = extras.Count > 0
                ? extras[0]
                : "Weapon Properties.csv";

            var result = DumpingHelpers.Main(outputPath, Dump);
            return result < 0 ? -3 + result : result;
        }

        private static int Dump(RuntimeProcess runtime, AddressLibrary addressLibrary, string outputPath)
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

            var weaponPropertyCountFunctionPointer = Id2Pointer(103869);
            var weaponPropertyCount = PatternHelpers.GetWeaponPropertyCount(runtime, weaponPropertyCountFunctionPointer);

            var weaponPropertySize = Marshal.SizeOf(typeof(Natives.WeaponProperty));
            var weaponPropertiesPointer = Id2Pointer(773235);
            var weaponProperties = new Natives.WeaponProperty[weaponPropertyCount];
            var weaponPropertyPointer = weaponPropertiesPointer;
            for (int i = 0; i < weaponPropertyCount; i++, weaponPropertyPointer += weaponPropertySize)
            {
                weaponProperties[i] = runtime.ReadStructure<Natives.WeaponProperty>(weaponPropertyPointer);
            }

            List<(string, string, string, string, int, string)> lines = new();
            foreach (var weaponProperty in weaponProperties)
            {
                if (weaponProperty.CopyCallback == IntPtr.Zero)
                {
                    continue;
                }

                var weaponPropertyName = weaponProperty.Name == IntPtr.Zero
                    ? "(null)"
                    : runtime.ReadStringZ(weaponProperty.Name, Encoding.ASCII);

                if (PatternHelpers.GetValueLoadOffsets(runtime, weaponProperty.CopyCallback, out var info) == false)
                {
                    var copyCallbackId = Pointer2Id(weaponProperty.CopyCallback);
                    Console.WriteLine($"Failed to match weapon property copy callback {Pointer2Id(weaponProperty.CopyCallback)}!");
                    continue;
                }

                var (structName, struct2Name) = GetName(info.structOffset, info.struct2Offset);

                string gameSettingName = "";
                if (weaponProperty.ValueGameSetting != IntPtr.Zero)
                {
                    var gameSettingNamePointer = runtime.ReadPointer(weaponProperty.ValueGameSetting + 0x18);
                    gameSettingName = gameSettingNamePointer == IntPtr.Zero
                        ? "(null)"
                        : runtime.ReadStringZ(gameSettingNamePointer, Encoding.ASCII);
                }

                lines.Add((GetTypeName(info.type, weaponPropertyName), weaponPropertyName, structName ?? "", struct2Name ?? "", info.fieldOffset, gameSettingName));
            }

            string output;
            using (var writer = new StringWriter())
            {
                writer.WriteLine("Type,Property Name,Struct 1,Struct 2,Offset,Game Setting Value");
                foreach (var line in lines
                    .OrderBy(l => l.Item3)
                    .ThenBy(l => l.Item4)
                    .ThenBy(l => l.Item5))
                {
                    writer.WriteLine($"{line.Item1},{line.Item2},{line.Item3},{line.Item4},+0x{line.Item5:X},{line.Item6}");
                }

                writer.Flush();

                output = writer.ToString();
            }

            File.WriteAllText(outputPath, output);
            return 0;
        }

        private static string GetTypeName(PatternHelpers.LoadType type, string name) => type switch
        {
            PatternHelpers.LoadType.UInt8 => GetUInt8Name(name),
            PatternHelpers.LoadType.UInt16 => GetUInt16Name(name),
            PatternHelpers.LoadType.UInt32 => GetUInt32Name(name),
            PatternHelpers.LoadType.Single => "float",
            PatternHelpers.LoadType.SingleToInt32 => "float (cast to int32)",
            PatternHelpers.LoadType.FormId => "formid",
            PatternHelpers.LoadType.FormIdAndInt32 => "formid+int32",
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

        private static (string name, string name2) GetName(int offset, int offset2) => offset switch
        {
            -1 => (null, GetWeaponName(offset2)),
            0x18 => ("WAIM", GetAimName(offset2)),
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

        private static string GetWeaponName(int offset) => offset switch
        {
            -1 => null,
            _ => throw new NotSupportedException(),
        };

        private static string GetAimName(int offset) => offset switch
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
