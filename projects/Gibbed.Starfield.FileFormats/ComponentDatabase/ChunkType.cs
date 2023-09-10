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
    internal enum ChunkType : uint
    {
        STRT = 0x54525453u,
        TYPE = 0x45505954u,
        CLAS = 0x53414C43u,
        OBJT = 0x544A424Fu,
        DIFF = 0x46464944u,
        USER = 0x52455355u,
        USRD = 0x44525355u,
        MAPC = 0x4350414Du,
        LIST = 0x5453494Cu,
    };
}
