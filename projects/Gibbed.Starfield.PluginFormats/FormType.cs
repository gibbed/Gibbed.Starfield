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

namespace Gibbed.Starfield.PluginFormats
{
    public enum FormType : uint
    {
        NONE = 0x454E4F4Eu, // 0
        TES4 = 0x34534554u, // 1
        GRUP = 0x50555247u, // 2
        GMST = 0x54534D47u, // 3
        KYWD = 0x4457594Bu, // 4   BGSKeyword
        FFKW = 0x574B4646u, // 5   BGSKeyword BGSFormFolderKeywordList
        LCRT = 0x5452434Cu, // 6   BGSLocationRefType
        AACT = 0x54434141u, // 7   BGSAction
        TRNS = 0x534E5254u, // 8   BGSTransform
        TXST = 0x54535854u, // 9   BGSTextureSet
        GLOB = 0x424F4C47u, // 10  TESGlobal
        DMGT = 0x54474D44u, // 11  BGSDamageType
        CLAS = 0x53414C43u, // 12  TESClass
        FACT = 0x54434146u, // 13  TESFaction
        AFFE = 0x45464641u, // 14  BGSAffinityEvent
        HDPT = 0x54504448u, // 15  BGSHeadPart
        EYES = 0x53455945u, // 16  TESEyes
        RACE = 0x45434152u, // 17  TESRace
        SOUN = 0x4E554F53u, // 18  TESSound
        SECH = 0x48434553u, // 19  BGSSoundEcho
        ASPC = 0x43505341u, // 20  BGSAcousticSpace
        AOPF = 0x46504F41u, // 21  BGSAudioOcclusionPrimitive
        SKIL = 0x4C494B53u, // 22
        MGEF = 0x4645474Du, // 23  EffectSetting
        SCPT = 0x54504353u, // 24  Script
        LTEX = 0x5845544Cu, // 25  TESLandTexture
        PDCL = 0x4C434450u, // 26  BGSProjectedDecal
        ENCH = 0x48434E45u, // 27  EnchantmentItem
        SPEL = 0x4C455053u, // 28  SpellItem
        SCRL = 0x4C524353u, // 29  ScrollItem
        ACTI = 0x49544341u, // 30  TESObjectACTI
        TACT = 0x54434154u, // 31  BGSTalkingActivator
        CURV = 0x56525543u, // 32  BGSCurveForm
        CUR3 = 0x33525543u, // 33  BGSCurve3DForm
        ARMO = 0x4F4D5241u, // 34  TESObjectARMO
        BOOK = 0x4B4F4F42u, // 35  TESObjectBOOK
        CONT = 0x544E4F43u, // 36  TESObjectCONT
        DOOR = 0x524F4F44u, // 37  TESObjectDOOR
        INGR = 0x52474E49u, // 38  IngredientItem
        LIGH = 0x4847494Cu, // 39  TESObjectLIGH
        MISC = 0x4353494Du, // 40  TESObjectMISC
        STAT = 0x54415453u, // 41  TESObjectSTAT
        SCOL = 0x4C4F4353u, // 42  BGSStaticCollection
        PKIN = 0x4E494B50u, // 43  BGSPackIn
        MSTT = 0x5454534Du, // 44  BGSMovableStatic
        GRAS = 0x53415247u, // 45  TESGrass
        FLOR = 0x524F4C46u, // 46  TESFlora
        FURN = 0x4E525546u, // 47  TESFurniture
        WEAP = 0x50414557u, // 48  TESObjectWEAP
        AMMO = 0x4F4D4D41u, // 49  TESAmmo
        NPC_ = 0x5F43504Eu, // 50  TESNPC
        LVLN = 0x4E4C564Cu, // 51  TESLevCharacter
        LVLP = 0x504C564Cu, // 52  BGSLevPackIn
        KEYM = 0x4D59454Bu, // 53  TESKey
        ALCH = 0x48434C41u, // 54  AlchemyItem
        IDLM = 0x4D4C4449u, // 55  BGSIdleMarker
        BMMO = 0x4F4D4D42u, // 56  BGSBiomeMarkerObject
        NOTE = 0x45544F4Eu, // 57  BGSNote
        PROJ = 0x4A4F5250u, // 58  BGSProjectile
        HAZD = 0x445A4148u, // 59  BGSHazard
        BNDS = 0x53444E42u, // 60  BGSBendableSpline
        SLGM = 0x4D474C53u, // 61  TESSoulGem
        TERM = 0x4D524554u, // 62  BGSTerminal
        LVLI = 0x494C564Cu, // 63  TESLevItem
        GBFT = 0x54464247u, // 64  BGSGenericBaseFormTemplate
        GBFM = 0x4D464247u, // 65  BGSGenericBaseForm
        LVLB = 0x424C564Cu, // 66  BGSLevGenericBaseForm
        WTHR = 0x52485457u, // 67  TESWeather
        WTHS = 0x53485457u, // 68  BGSWeatherSettingsForm
        CLMT = 0x544D4C43u, // 69  TESClimate
        SPGD = 0x44475053u, // 70  BGSShaderParticleGeometryData
        REGN = 0x4E474552u, // 71  TESRegion
        NAVI = 0x4956414Eu, // 72  NavMeshInfoMap
        CELL = 0x4C4C4543u, // 73  TESObjectCELL
        REFR = 0x52464552u, // 74  TESObjectREFR
        ACHR = 0x52484341u, // 75  Actor
        PMIS = 0x53494D50u, // 76  MissileProjectile
        PARW = 0x57524150u, // 77  ArrowProjectile
        PGRE = 0x45524750u, // 78  GrenadeProjectile
        PBEA = 0x41454250u, // 79  BeamProjectile
        PFLA = 0x414C4650u, // 80  FlameProjectile
        PCON = 0x4E4F4350u, // 81  ConeProjectile
        PPLA = 0x414C5050u, // 82  PlasmaProjectile
        PBAR = 0x52414250u, // 83  BarrierProjectile
        PEMI = 0x494D4550u, // 84  EmitterProjectile
        PHZD = 0x445A4850u, // 85  Hazard
        WRLD = 0x444C5257u, // 86  TESWorldSpace
        NAVM = 0x4D56414Eu, // 87  NavMesh
        TLOD = 0x444F4C54u, // 88
        DIAL = 0x4C414944u, // 89  TESTopic
        INFO = 0x4F464E49u, // 90  TESTopicInfo
        QUST = 0x54535551u, // 91  TESQuest
        IDLE = 0x454C4449u, // 92  TESIdleForm
        PACK = 0x4B434150u, // 93  TESPackage
        CSTY = 0x59545343u, // 94  TESCombatStyle
        LSCR = 0x5243534Cu, // 95  TESLoadScreen
        LVSP = 0x5053564Cu, // 96  TESLevSpell
        ANIO = 0x4F494E41u, // 97  TESObjectANIO
        WATR = 0x52544157u, // 98  TESWaterForm
        EFSH = 0x48534645u, // 99  TESEffectShader
        TOFT = 0x54464F54u, // 100
        EXPL = 0x4C505845u, // 101 BGSExplosion
        DEBR = 0x52424544u, // 102 BGSDebris
        IMGS = 0x53474D49u, // 103 TESImageSpace
        IMAD = 0x44414D49u, // 104 TESImageSpaceModifier
        FLST = 0x54534C46u, // 105 BGSListForm
        PERK = 0x4B524550u, // 106 BGSPerk
        BPTD = 0x44545042u, // 107 BGSBodyPartData
        ADDN = 0x4E444441u, // 108 BGSAddonNode
        AVIF = 0x46495641u, // 109 ActorValueInfo
        CAMS = 0x534D4143u, // 110 BGSCameraShot
        CPTH = 0x48545043u, // 111 BGSCameraPath
        VTYP = 0x50595456u, // 112 BGSVoiceType
        MATT = 0x5454414Du, // 113 BGSMaterialType
        IPCT = 0x54435049u, // 114 BGSImpactData
        IPDS = 0x53445049u, // 115 BGSImpactDataSet
        ARMA = 0x414D5241u, // 116 TESObjectARMA
        LCTN = 0x4E54434Cu, // 117 BGSLocation
        MESG = 0x4753454Du, // 118 BGSMessage
        RGDL = 0x4C444752u, // 119
        DOBJ = 0x4A424F44u, // 120
        DFOB = 0x424F4644u, // 121 BGSDefaultObject
        LGTM = 0x4D54474Cu, // 122 BGSLightingTemplate
        MUSC = 0x4353554Du, // 123 BGSMusicType
        FSTP = 0x50545346u, // 124 BGSFootstep
        FSTS = 0x53545346u, // 125 BGSFootstepSet
        SMBN = 0x4E424D53u, // 126 BGSStoryManagerBranchNode
        SMQN = 0x4E514D53u, // 127 BGSStoryManagerQuestNode
        SMEN = 0x4E454D53u, // 128 BGSStoryManagerEventNode
        DLBR = 0x52424C44u, // 129 BGSDialogueBranch
        MUST = 0x5453554Du, // 130 BGSMusicTrackFormWrapper
        DLVW = 0x57564C44u, // 131
        WOOP = 0x504F4F57u, // 132 TESWordOfPower
        SHOU = 0x554F4853u, // 133 TESShout
        EQUP = 0x50555145u, // 134 BGSEquipSlot
        RELA = 0x414C4552u, // 135 BGSRelationship
        SCEN = 0x4E454353u, // 136 BGSScene
        ASTP = 0x50545341u, // 137 BGSAssociationType
        OTFT = 0x5446544Fu, // 138 BGSOutfit
        ARTO = 0x4F545241u, // 139 BGSArtObject
        MATO = 0x4F54414Du, // 140
        MOVT = 0x54564F4Du, // 141 BGSMovementType
        DUAL = 0x4C415544u, // 142 BGSDualCastData
        COLL = 0x4C4C4F43u, // 143 BGSCollisionLayer
        CLFM = 0x4D464C43u, // 144 BGSColorForm
        REVB = 0x42564552u, // 145 BGSReverbParameters
        RFGP = 0x50474652u, // 146 BGSReferenceGroup
        AMDL = 0x4C444D41u, // 147 BGSAimModel
        AAMD = 0x444D4141u, // 148 BGSAimAssistModel
        MAAM = 0x4D41414Du, // 149 BGSMeleeAimAssistModel
        LAYR = 0x5259414Cu, // 150
        COBJ = 0x4A424F43u, // 151 BGSConstructibleObject
        OMOD = 0x444F4D4Fu, // 152 BGSMod::Attachment::Mod
        ZOOM = 0x4D4F4F5Au, // 153 BGSAimDownSightModel
        INNR = 0x524E4E49u, // 154 BGSInstanceNamingRules
        KSSM = 0x4D53534Bu, // 155 BGSSoundKeywordMapping
        SCCO = 0x4F434353u, // 156
        AORU = 0x55524F41u, // 157 BGSAttractionRule
        STAG = 0x47415453u, // 158 BGSSoundTagSet
        IRES = 0x53455249u, // 159 BGSResource
        BIOM = 0x4D4F4942u, // 160 BGSBiome
        NOCM = 0x4D434F4Eu, // 161
        LENS = 0x534E454Cu, // 162 BGSLensFlare
        LSPR = 0x5250534Cu, // 163
        OVIS = 0x5349564Fu, // 164
        DLYR = 0x52594C44u, // 165
        STND = 0x444E5453u, // 166 BGSSnapTemplateNode
        STMP = 0x504D5453u, // 167 BGSSnapTemplate
        GCVR = 0x52564347u, // 168 BGSGroundCover
        MRPH = 0x4850524Du, // 169 BGSMorphableObject
        TRAV = 0x56415254u, // 170 BGSTraversal
        RSGD = 0x44475352u, // 171 BGSResourceGenerationData
        OSWP = 0x5057534Fu, // 172 BGSObjectSwap
        ATMO = 0x4F4D5441u, // 173 BGSAtmosphere
        LVSC = 0x4353564Cu, // 174 BGSLevSpaceCell
        SPCH = 0x48435053u, // 175 BGSSpeechChallengeObject
        RESO = 0x4F534552u, // 176
        AAPD = 0x44504141u, // 177 BGSAimAssistPoseData
        VOLI = 0x494C4F56u, // 178 BGSVolumetricLighting
        SFBK = 0x4B424653u, // 179 BGSSurface::Block
        SFPC = 0x43504653u, // 180
        SFPT = 0x54504653u, // 181 BGSSurface::Pattern
        SFTR = 0x52544653u, // 182 BGSSurface::Tree
        PCMT = 0x544D4350u, // 183 BGSPlanetContentManagerTree
        BMOD = 0x444F4D42u, // 184 BGSBoneModifier
        STBH = 0x48425453u, // 185 BGSSnapBehavior
        PNDT = 0x54444E50u, // 186 BGSPlanet::PlanetData
        IUTF = 0x46545549u, // 187
        CNDF = 0x46444E43u, // 188 BGSConditionForm
        PCBN = 0x4E424350u, // 189 BGSPlanetContentManagerBranchNode
        PCCN = 0x4E434350u, // 190 BGSPlanetContentManagerContentNode
        STDT = 0x54445453u, // 191 BSGalaxy::BGSStar
        WWED = 0x44455757u, // 192 BGSWwiseEventForm
        RSPJ = 0x4A505352u, // 193 BGSResearchProjectForm
        AOPS = 0x53504F41u, // 194 BGSAimOpticalSightModel
        AMBS = 0x53424D41u, // 195 BGSAmbienceSet
        WBAR = 0x52414257u, // 196 BGSWeaponBarrelModel
        PTST = 0x54535450u, // 197 BGSSurface::PatternStyle
        LMSW = 0x57534D4Cu, // 198 BGSLayeredMaterialSwap
        FORC = 0x43524F46u, // 199 BGSForceData
        TMLM = 0x4D4C4D54u, // 200 BGSTerminalMenu
        EFSQ = 0x51534645u, // 201 BGSEffectSequenceForm
        SDLT = 0x544C4453u, // 202 BGSSecondaryDamageList
        MTPT = 0x5450544Du, // 203 BGSMaterialPathForm
        CLDF = 0x46444C43u, // 204 BGSCloudForm
        FOGV = 0x56474F46u, // 205 BGSFogVolumeForm
        WKMF = 0x464D4B57u, // 206 BGSWwiseKeywordMapping
        LGDI = 0x4944474Cu, // 207 BGSLegendaryItem
        PSDC = 0x43445350u, // 208 BGSParticleSystemDefineCollection
        SUNP = 0x504E5553u, // 209 BSGalaxy::BGSSunPresetForm
        PMFT = 0x54464D50u, // 210 BGSPhotoModeFeature
        TODD = 0x44444F54u, // 211 BGSTimeOfDayData
        AVMD = 0x444D5641u, // 212 BGSAVMData
        PERS = 0x53524550u, // 213 TESDataHandlerPersistentCreatedUtil::BGSPersistentIDsForm
        CHAL = 0x4C414843u, // 214 BGSChallengeForm
    }
}
