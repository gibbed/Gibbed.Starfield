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
using DumpReflection.Attributes;

namespace DumpReflection
{
    internal class AttributeFactory
    {
        public static IAttribute Create(string name) => name switch
        {
            "BSReflection::Metadata::Alias" => new AliasAttribute(),
            "BSReflection::Metadata::AllowDataBinding" => new AllowDataBindingAttribute(),
            "BSReflection::Metadata::Attribute" => new AttributeAttribute(),
            "BSReflection::Metadata::Color" => new ColorAttribute(),
            "BSReflection::Metadata::Component" => new ComponentAttribute(),
            "BSReflection::Metadata::CorrespondingType" => new CorrespondingTypeAttribute(),
            "BSReflection::Metadata::DBObjectType" => new DBObjectTypeAttribute(),
            "BSReflection::Metadata::DefaultTrackRequirement" => new DefaultTrackRequirementAttribute(),
            "BSReflection::Metadata::Deprecated" => new DeprecatedAttribute(),
            "BSReflection::Metadata::DisplayName" => new DisplayNameAttribute(),
            "BSReflection::Metadata::DontDeriveChildren" => new DontDeriveChildrenAttribute(),
            "BSReflection::Metadata::EditInline" => new EditInlineAttribute(),
            "BSReflection::Metadata::EditWidget" => new EditWidgetAttribute(),
            "BSReflection::Metadata::Flags" => new Attributes.FlagsAttribute(),
            "BSReflection::Metadata::Hidden" => new HiddenAttribute(),
            "BSReflection::Metadata::IconPath" => new IconPathAttribute(),
            "BSReflection::Metadata::NameWidget" => new NameWidgetAttribute(),
            "BSReflection::Metadata::NotDiffed" => new NotDiffedAttribute(),
            "BSReflection::Metadata::PersistentDBEdge" => new PersistentDBEdgeAttribute(),
            "BSReflection::Metadata::Serializable" => new Attributes.SerializableAttribute(),
            "BSReflection::Metadata::ToolTip" => new ToolTipAttribute(),
            "BSReflection::Metadata::TrackRequirement" => new TrackRequirementAttribute(),
            "BSReflection::Metadata::Transient" => new TransientAttribute(),
            "BSReflection::Metadata::Unique" => new UniqueAttribute(),
            "BSReflection::Metadata::Unsorted" => new UnsortedAttribute(),
            _ => throw new NotSupportedException($"unsupported attribute type '{name}'"),
        };
    }
}
