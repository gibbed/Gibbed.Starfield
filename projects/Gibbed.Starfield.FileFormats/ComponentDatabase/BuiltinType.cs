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

namespace Gibbed.Starfield.FileFormats.ComponentDatabase
{
    public enum BuiltinType : uint
    {
        Null = 0xFFFFFF01u,
        String = 0xFFFFFF02u,
        List = 0xFFFFFF03u,
        Map = 0xFFFFFF04u,
        Ref = 0xFFFFFF05u,

        Int8 = 0xFFFFFF08u,
        UInt8 = 0xFFFFFF09u,
        Int16 = 0xFFFFFF0Au,
        UInt16 = 0xFFFFFF0Bu,
        Int32 = 0xFFFFFF0Cu,
        UInt32 = 0xFFFFFF0Du,
        Int64 = 0xFFFFFF0Eu,
        UInt64 = 0xFFFFFF0Fu,
        Bool = 0xFFFFFF10u,
        Float = 0xFFFFFF11u,
        Double = 0xFFFFFF12u,
    }
}
