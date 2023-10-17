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

namespace DumpReflection
{
    internal class AttributeFactory
    {
        public static IAttribute Create(string name) => name switch
        {
            "BSReflection::Metadata::Alias" => new AliasAttribute(),
            "BSReflection::Metadata::AllowDataBinding" => new AllowDataBindingAttribute(),
            "BSReflection::Metadata::AllowNoneOption" => new AllowNoneOptionAttribute(),
            "BSReflection::Metadata::AllowOpenFormButton" => new AllowOpenFormButtonAttribute(),
            "BSReflection::Metadata::AtmosphereCoefficentType" => new AtmosphereCoefficentTypeAttribute(),
            "BSReflection::Metadata::Attribute" => new AttributeAttribute(),
            "BSReflection::Metadata::BindingFilterFunction" => new BindingFilterFunctionAttribute(),
            "BSReflection::Metadata::BindingResource" => new BindingResourceAttribute(),
            "BSReflection::Metadata::Category" => new CategoryAttribute(),
            "BSReflection::Metadata::Color" => new ColorAttribute(),
            "BSReflection::Metadata::Component" => new ComponentAttribute(),
            "BSReflection::Metadata::ConditionalDisplayName" => new ConditionalDisplayNameAttribute(),
            "BSReflection::Metadata::ConditionalRange" => new ConditionalRangeAttribute(),
            "BSReflection::Metadata::CorrespondingType" => new CorrespondingTypeAttribute(),
            "BSReflection::Metadata::CPUCost" => new CPUCostAttribute(),
            "BSReflection::Metadata::DBObjectType" => new DBObjectTypeAttribute(),
            "BSReflection::Metadata::DefaultTrackRequirement" => new DefaultTrackRequirementAttribute(),
            "BSReflection::Metadata::Deprecated" => new DeprecatedAttribute(),
            "BSReflection::Metadata::DisplayName" => new DisplayNameAttribute(),
            "BSReflection::Metadata::DontDeriveChildren" => new DontDeriveChildrenAttribute(),
            "BSReflection::Metadata::EditInline" => new EditInlineAttribute(),
            "BSReflection::Metadata::EditWidget" => new EditWidgetAttribute(),
            "BSReflection::Metadata::FilterFunction" => new FilterFunctionAttribute(),
            "BSReflection::Metadata::Flags" => new FlagsAttribute(),
            "BSReflection::Metadata::FormReference" => new FormReferenceAttribute(),
            "BSReflection::Metadata::FormType" => new FormTypeAttribute(),
            "BSReflection::Metadata::FunctionName" => new FunctionNameAttribute(),
            "BSReflection::Metadata::Hidden" => new HiddenAttribute(),
            "BSReflection::Metadata::HideChildItems" => new HideChildItemsAttribute(),
            "BSReflection::Metadata::IconPath" => new IconPathAttribute(),
            "BSReflection::Metadata::LiveEditable" => new LiveEditableAttribute(),
            "BSReflection::Metadata::MaterialBinding" => new MaterialBindingAttribute(),
            "BSReflection::Metadata::MaterialBindingFilterAttribute" => new MaterialBindingFilterAttribute(),
            "BSReflection::Metadata::MaterialTextureSelectionWidgetSetup" => new MaterialTextureSelectionWidgetSetupAttribute(),
            "BSReflection::Metadata::NameWidget" => new NameWidgetAttribute(),
            "BSReflection::Metadata::NotCompared" => new NotComparedAttribute(),
            "BSReflection::Metadata::NotDiffed" => new NotDiffedAttribute(),
            "BSReflection::Metadata::Nullable" => new NullableAttribute(),
            "BSReflection::Metadata::OnEdit" => new OnEditAttribute(),
            "BSReflection::Metadata::PersistentDBEdge" => new PersistentDBEdgeAttribute(),
            "BSReflection::Metadata::Range" => new RangeAttribute(),
            "BSReflection::Metadata::ReadOnly" => new ReadOnlyAttribute(),
            "BSReflection::Metadata::ResourceFileInfo" => new ResourceFileInfoAttribute(),
            "BSReflection::Metadata::ResourceFileNameFilter" => new ResourceFileNameFilterAttribute(),
            "BSReflection::Metadata::RestrictNodeType" => new RestrictNodeTypeAttribute(),
            "BSReflection::Metadata::Serializable" => new SerializableAttribute(),
            "BSReflection::Metadata::StaticSize" => new StaticSizeAttribute(),
            "BSReflection::Metadata::ToolTip" => new ToolTipAttribute(),
            "BSReflection::Metadata::TrackRequirement" => new TrackRequirementAttribute(),
            "BSReflection::Metadata::Transient" => new TransientAttribute(),
            "BSReflection::Metadata::Unique" => new UniqueAttribute(),
            "BSReflection::Metadata::Unsorted" => new UnsortedAttribute(),
            "BSReflection::Metadata::ValidatedBy" => new ValidatedByAttribute(),
            "BSReflection::Metadata::VectorAxis" => new VectorAxisAttribute(),
            "BSReflection::Metadata::Volatile" => new VolatileAttribute(),
            _ => throw new System.NotSupportedException($"unsupported attribute type '{name}'"),
        };
    }
}
