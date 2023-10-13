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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.AddressLibrary;

namespace StarfieldDumping
{
    public static class AddressLibraryLoader
    {
        public static AddressLibrary LoadFor(string exePath, bool isMsStoreVersion)
        {
            var version = GetVersion(exePath, isMsStoreVersion);
            if (string.IsNullOrEmpty(version) == true)
            {
                return null;
            }

            var gamePath = Path.GetDirectoryName(exePath);

            var libraryName = isMsStoreVersion == false
                ? $"versionlib-{version.Replace('.', '-')}.bin"
                : $"versionlib-{version.Replace('.', '-')}-1.bin";

            string libraryPath = GetDataPaths(gamePath, libraryName)
                .Where(p => File.Exists(p) == true)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(libraryPath) == true)
            {
                return null;
            }

            var inputBytes = File.ReadAllBytes(libraryPath);
            using MemoryStream input = new(inputBytes, false);
            return AddressLibrary.Load(input);
        }

        private static IEnumerable<string> GetDataPaths(string gamePath, string fileName)
        {
            yield return fileName;
            yield return Path.Combine(gamePath, "Data", "SFSE", "Plugins", fileName);
        }

        private static string GetVersion(string path, bool isMsStoreVersion)
        {
            var version = VersionHelper.GetProductVersion(path);
            return string.IsNullOrEmpty(version) == false || isMsStoreVersion == false
                ? version
                : GetMsStoreVersion(path);
        }

        private static string GetMsStoreVersion(string path)
        {
            // TODO(gibbed): extract version from the path instead of loading MicrosoftGame.Config
            var basePath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(basePath) == true)
            {
                return null;
            }
            var configPath = Path.Combine(basePath, "MicrosoftGame.Config");
            if (File.Exists(configPath) == false)
            {
                return null;
            }
            System.Xml.XPath.XPathDocument doc;
            try
            {
                using var input = File.OpenRead(configPath);
                doc = new(input);
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
            var nav = doc.CreateNavigator();
            return nav.SelectSingleNode("/Game/Identity/@Version").Value;
        }
    }
}
