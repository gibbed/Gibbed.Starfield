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
using Newtonsoft.Json;

namespace DumpReflection
{
    internal static class JsonHelpers
    {
        public static void WriteValueFlags<T>(this JsonWriter writer, T flags, params T[] ignoreValues)
            where T : struct, Enum
        {
            var oldFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                // TODO(gibbed): jank, but okay
                if (Array.IndexOf(ignoreValues, value) >= 0)
                {
                    continue;
                }
                if (flags.HasFlag(value) == false)
                {
                    continue;
                }
                writer.WriteValue(Enum.GetName(typeof(T), value));
            }
            writer.WriteEndArray();
            writer.Formatting = oldFormatting;
        }
    }
}
