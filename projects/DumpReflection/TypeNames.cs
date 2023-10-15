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

namespace DumpReflection
{
    internal static class TypeNames
    {
        public static IEnumerable<(ulong id, string name)> Get()
        {
            yield return (780674, "int16_t");
            yield return (780678, "int8_t");
            yield return (780680, "uint8_t");
            yield return (780684, "int64_t");
            yield return (780687, "double");
            yield return (780690, "uint16_t");
            yield return (780692, "int32_t");
            yield return (780696, "bool");
            yield return (780699, "float");
            yield return (780701, "uint64_t");
            yield return (780704, "uint32_t");

            yield return (780711, "BSFixedString");

            yield return (887377, "BSReflection::TESFormPointer<ActorValueInfo *>");
            yield return (887380, "BSReflection::TESFormPointer<TESObjectLIGH *>");
            yield return (887383, "BSReflection::TESFormPointer<BGSArtObject *>");

            yield return (772740, "BSTArray<TESForm *>");
            yield return (772741, "BSReflection::StdUniquePtrType<BSAttachConfig::IAttachableObject>");

            yield return (780005, "BSTArray<std::unique_ptr<BSSequence::Event>>");
        }
    }
}
