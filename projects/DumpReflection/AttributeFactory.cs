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

using DumpReflection.Attributes;
using DumpReflection.Reflection;

namespace DumpReflection
{
    internal class AttributeFactory
    {
        public static IAttribute Create(IType type)
        {
            return type.Name switch
            {
                "BSReflection::Metadata::Alias" => new AliasAttribute(type),
                "BSReflection::Metadata::AllowDataBinding" => new AllowDataBindingAttribute(type),
                "BSReflection::Metadata::AllowNoneOption" => new AllowNoneOptionAttribute(type),
                "BSReflection::Metadata::AllowOpenFormButton" => new AllowOpenFormButtonAttribute(type),
                "BSReflection::Metadata::AtmosphereCoefficentType" => new AtmosphereCoefficentTypeAttribute(type),
                "BSReflection::Metadata::Attribute" => new AttributeAttribute(type),
                "BSReflection::Metadata::BindingFilterFunction" => new BindingFilterFunctionAttribute(type),
                "BSReflection::Metadata::BindingResource" => new BindingResourceAttribute(type),
                "BSReflection::Metadata::Category" => new CategoryAttribute(type),
                "BSReflection::Metadata::Color" => new ColorAttribute(type),
                "BSReflection::Metadata::Component" => new ComponentAttribute(type),
                "BSReflection::Metadata::ConditionalDisplayName" => new ConditionalDisplayNameAttribute(type),
                "BSReflection::Metadata::ConditionalRange" => new ConditionalRangeAttribute(type),
                "BSReflection::Metadata::CorrespondingType" => new CorrespondingTypeAttribute(type),
                "BSReflection::Metadata::CPUCost" => new CPUCostAttribute(type),
                "BSReflection::Metadata::DBObjectType" => new DBObjectTypeAttribute(type),
                "BSReflection::Metadata::DefaultTrackRequirement" => new DefaultTrackRequirementAttribute(type),
                "BSReflection::Metadata::Deprecated" => new DeprecatedAttribute(type),
                "BSReflection::Metadata::DisplayName" => new DisplayNameAttribute(type),
                "BSReflection::Metadata::DontDeriveChildren" => new DontDeriveChildrenAttribute(type),
                "BSReflection::Metadata::EditInline" => new EditInlineAttribute(type),
                "BSReflection::Metadata::EditWidget" => new EditWidgetAttribute(type),
                "BSReflection::Metadata::FilterFunction" => new FilterFunctionAttribute(type),
                "BSReflection::Metadata::Flags" => new FlagsAttribute(type),
                "BSReflection::Metadata::FormReference" => new FormReferenceAttribute(type),
                "BSReflection::Metadata::FormType" => new FormTypeAttribute(type),
                "BSReflection::Metadata::FunctionName" => new FunctionNameAttribute(type),
                "BSReflection::Metadata::Hidden" => new HiddenAttribute(type),
                "BSReflection::Metadata::HideChildItems" => new HideChildItemsAttribute(type),
                "BSReflection::Metadata::IconPath" => new IconPathAttribute(type),
                "BSReflection::Metadata::LiveEditable" => new LiveEditableAttribute(type),
                "BSReflection::Metadata::MaterialBinding" => new MaterialBindingAttribute(type),
                "BSReflection::Metadata::MaterialBindingFilterAttribute" => new MaterialBindingFilterAttribute(type),
                "BSReflection::Metadata::MaterialTextureSelectionWidgetSetup" => new MaterialTextureSelectionWidgetSetupAttribute(type),
                "BSReflection::Metadata::NameWidget" => new NameWidgetAttribute(type),
                "BSReflection::Metadata::NotCompared" => new NotComparedAttribute(type),
                "BSReflection::Metadata::NotDiffed" => new NotDiffedAttribute(type),
                "BSReflection::Metadata::Nullable" => new NullableAttribute(type),
                "BSReflection::Metadata::OnEdit" => new OnEditAttribute(type),
                "BSReflection::Metadata::PersistentDBEdge" => new PersistentDBEdgeAttribute(type),
                "BSReflection::Metadata::Range" => new RangeAttribute(type),
                "BSReflection::Metadata::ReadOnly" => new ReadOnlyAttribute(type),
                "BSReflection::Metadata::ResourceFileInfo" => new ResourceFileInfoAttribute(type),
                "BSReflection::Metadata::ResourceFileNameFilter" => new ResourceFileNameFilterAttribute(type),
                "BSReflection::Metadata::RestrictNodeType" => new RestrictNodeTypeAttribute(type),
                "BSReflection::Metadata::Serializable" => new SerializableAttribute(type),
                "BSReflection::Metadata::StaticSize" => new StaticSizeAttribute(type),
                "BSReflection::Metadata::ToolTip" => new ToolTipAttribute(type),
                "BSReflection::Metadata::TrackRequirement" => new TrackRequirementAttribute(type),
                "BSReflection::Metadata::Transient" => new TransientAttribute(type),
                "BSReflection::Metadata::Unique" => new UniqueAttribute(type),
                "BSReflection::Metadata::Unsorted" => new UnsortedAttribute(type),
                "BSReflection::Metadata::ValidatedBy" => new ValidatedByAttribute(type),
                "BSReflection::Metadata::VectorAxis" => new VectorAxisAttribute(type),
                "BSReflection::Metadata::Volatile" => new VolatileAttribute(type),
                _ => throw new System.NotSupportedException($"unsupported attribute type '{type.Name}'"),
            };
        }
    }
}
