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
using System.Diagnostics;
using System.Linq;
using Gibbed.AddressLibrary;

namespace StarfieldDumping
{
    public static class DumpingHelpers
    {
        public delegate int MainDelegate<T>(RuntimeProcess runtime, AddressLibrary addressLibrary, T arg);

        public static int Main<T>(T arg, MainDelegate<T> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var process = FindSuitableProcess();
            if (process == null)
            {
                Console.WriteLine("Failed to find suitable Starfield process.");
                return -1;
            }

            var isSteamVersion = process.Modules
                .Cast<ProcessModule>()
                .Any(m => IsSteamModule(m.ModuleName) == true);
            var isMsStoreVersion = isSteamVersion == false;

            AddressLibrary addressLibrary;
            try
            {
                addressLibrary = AddressLibraryLoader.LoadFor(process.MainModule.FileName, isMsStoreVersion);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when fetching address library for Starfield:");
                Console.WriteLine(e);
                return -2;
            }

            if (addressLibrary == null)
            {
                Console.WriteLine("Failed to load address library for Starfield.");
                return -3;
            }

            using RuntimeProcess runtime = new();
            if (runtime.OpenProcess(process) == false)
            {
                Console.WriteLine("Failed to open Starfield process.");
                return -4;
            }

            var result = callback(runtime, addressLibrary, arg);
            return result >= 0
                ? result
                : -5 + result;
        }

        private static bool IsSteamModule(string name)
        {
            return name.StartsWith("steam_api64", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static Process FindSuitableProcess()
        {
            foreach (var processName in GetProcessNames())
            {
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                if (process != null)
                {
                    return process;
                }
            }
            return null;
        }

        private static IEnumerable<string> GetProcessNames()
        {
            yield return "Starfield";
            yield return "Starfield.noaslr";
            foreach (var version in GetVersions().Reverse())
            {
                yield return $"Starfield.noaslr.{version}";
            }
        }

        private static IEnumerable<string> GetVersions()
        {
            yield return "1.7.23.0";
            yield return "1.7.29.0";
            yield return "1.7.33.0";
            yield return "1.7.36.0";
        }
    }
}
