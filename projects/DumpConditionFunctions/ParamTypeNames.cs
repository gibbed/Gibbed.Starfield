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

namespace DumpConditionFunctions
{
    internal static class ParamTypeNames
    {
        public static string Get(uint id) => id switch
        {
            0x00 => "String",
            0x01 => "Integer",
            0x02 => "Float",
            0x03 => "InventoryObject03", // !unused! ... a whole lotta stuff ...
            0x04 => "ObjectReference", // TESObjectREFR to Actor or "map marker" or "container reference"
            0x05 => "ActorValue",
            0x06 => "Actor",
            0x07 => "SpellItem",
            0x08 => "Axis", // Axis (X,Y,Z) required for parameter %s.
            0x09 => "Cell",
            0x0A => "AnimationGroup", // !unused! Animation group \"%s\" not found for parameter %s.
            0x0B => "MagicItem",
            0x0D => "Topic", // TESTopic
            0x0E => "Quest", // TESQuest
            0x0F => "Race", // TESRace
            0x10 => "Class", // TESClass
            0x11 => "Faction", // TESFaction
            0x12 => "Sex", // Sex (Male, Female) required for parameter %s.
            0x13 => "Global", // TESGlobal
            0x14 => "Furniture", // ... a whole lotta stuff ... -- Invalid furniture object/list '%s' for parameter %s.
            0x15 => "Object", // TESObject
            0x17 => "Stage", // parses as Integer
            0x19 => "ActorBase", // TESActorBase
            0x1A => "ObjectReference", // !unused! TESObjectREFR
            0x1B => "WorldSpaceList", // TESWorldSpace or BGSListForm
            0x1C => "CrimeType", // Invalid crime type '%s' for parameter %s.  Crime type must be a numeric value from 0-%d.
            0x1D => "Package", // TESPackage
            0x1E => "CombatStyle", // !unused! TESCombatStyle
            0x1F => "EffectSetting", // EffectSetting
            0x20 => "FormType",
            0x21 => "Weather", // BGSWeatherSettingsForm
            0x22 => "NPC", // TESNPC
            0x23 => "Owner", // TESNPC or TESFaction
            0x24 => "EffectShader", // TESEffectShader
            0x25 => "FormList", // BGSListForm
            0x27 => "Perk", // BGSPerk
            0x28 => "Note", // !unused! BGSNote
            0x2A => "ImageSpaceModifier", // !unused! TESImageSpaceModifier
            0x2B => "ImageSpace", // !unused! TESImageSpace
            0x29 => "MiscellaneousStat",
            0x2E => "EventFunction",
            0x2F => "EventMember",
            0x30 => "Data",
            0x31 => "VoiceType", // BGSVoiceType or BGSListForm
            0x32 => "IdleForm", // TESIdleForm
            0x33 => "Message", // !unused! BGSMessage
            0x34 => "InventoryObject", // ... a whole lotta stuff ...
            0x35 => "Alignment", // Invalid alignment '%s' for parameter %s.
            0x36 => "EquipSlot", // BGSEquipSlot
            0x37 => "ObjectID", // [!!!] BGSListForm
            0x38 => "MusicType", // BGSMusicType
            0x39 => "CriticalStage", // Invalid CriticalStage '%s' for parameter %s.
            0x3A => "Keyword", // BGSKeyword
            0x3B => "LocationRefType", // BGSLocationRefType
            0x3C => "Location", // BGSLocation
            0x3D => "Form",
            0x3E => "QuestAlias", // Invalid Alias '%s' for parameter %s in owner quest '%s'.
            0x3F => "Primitive", // ??? - Invalid primitive keyword/list '%s' for parameter %s.
            0x41 => "RelationshipRank", // !unused! - Invalid Relationship Rank '%s' for parameter %s.
            0x42 => "Scene", // BGSScene
            0x43 => "CastingSource", // Invalid Casting Source '%s' for parameter %s.
            0x44 => "AssociationType", // BGSAssociationType
            0x45 => "WardState", // Invalid Ward State '%s' for parameter %s.
            0x46 => "PackageData_PossiblyNull",
            0x47 => "PackageData_Numeric",
            0x49 => "PapyrusVariableName",
            0x4A => "ArtObject", // !unused! BGSArtObject
            0x4B => "PackageData_Location",
            0x4D => "KnowableForm",
            0x4E => "Region", // TESRegion
            0x4F => "Action", // !unused! BGSAction
            0x50 => "MovementSelectIdleFromState", // Movement Select Idle From State "%s" not found for parameter %s.
            0x51 => "MovementSelectIdleToState", // Movement Select Idle To State "%s" not found for parameter %s.
            0x52 => "PapyrusScript",
            0x53 => "DamageType", // BGSDamageType
            0x54 => "Action2", // parses as Integer
            0x55 => "KeywordList", // BGSKeyword or BGSListForm
            0x56 => "FurnitureEntryType", // Invalid Furniture Entry Type '%s' for parameter %s.
            0x57 => "SpeechChallenge", // BGSSpeechChallengeObject
            0x58 => "AcousticSpace", // BGSAcousticSpace
            0x59 => "MovementType", // !unused! BGSMovementType
            0x5A => "Condition", // BGSConditionForm
            0x5B => "SnapTemplateNode", // BGSSnapTemplateNode
            0x5C => "Planet", // BGSPlanet::PlanetData -- Invalid Planet '%s' for parameter %s.
            0x5D => "BiomeMask", // Invalid biome mask '%s' for parameter %s.
            0x5E => "ResearchProject", // BGSResearchProjectForm
            0x5F => "PerkCategory", // Invalid perk category '%s' for parameter %s.
            0x60 => "PerkSkillGroup", // Invalid perk skill group '%s' for parameter %s.
            0x61 => "PerkSkillGroupComparison", // Invalid perk skill group comparison '%s' for parameter %s.
            0x62 => "DamageCauseType", // Invalid damage cause type '%s' for parameter %s.
            0x63 => "ReverbType", // !unused! BGSReverbParameters
            0x64 => "ReactionType", // Unknown variable '%s' for parameter %s. / parameter %s is out of range for ACTOR_RADIUS_REACTION_TYPE.
            0x65 => "LimbCategory",
            0x66 => "Pronoun", // Pronoun (Unselected, He_Him, She_Her, They_Them) required for parameter %s.
            0x67 => "Resource", // BGSResource
            _ => null,
        };
    }
}
