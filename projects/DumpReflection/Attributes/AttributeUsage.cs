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

namespace DumpReflection.Attributes
{
    [System.Flags]
    internal enum AttributeUsage : byte
    {
        None = 0,

        Unknown0 = 1 << 0,
        Class = 1 << 1,
        Property = 1 << 2,
        Enum = 1 << 3,
        EnumConstant = 1 << 4,
        ReleaseFinal = 1 << 5,
        Unknown6 = 1 << 6,
        Unknown7 = 1 << 7,

        ClassAndProperty = Class | Property,
        Type = Class | Property | Enum,
        
        All = Class | Property | Enum | EnumConstant,
    }
}
