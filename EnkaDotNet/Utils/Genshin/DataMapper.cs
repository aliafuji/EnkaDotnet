using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Enums.Genshin;
using System.Collections.Generic;
using System;

namespace EnkaDotNet.Utils.Genshin
{
    public class DataMapper
    {
        private readonly IGenshinAssets _assets;

        public DataMapper(IGenshinAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public PlayerInfo MapPlayerInfo(PlayerInfoModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var playerInfo = new PlayerInfo
            {
                Nickname = model.Nickname ?? "Unknown",
                Level = model.Level ?? 0,
                Signature = model.Signature ?? "",
                IconUrl = _assets.GetProfilePictureIconUrl(model.ProfilePicture?.Id ?? 0),
                WorldLevel = model.WorldLevel ?? 0,
                NameCardId = model.NameCardId ?? 0,
                FinishedAchievements = model.FinishAchievementNum ?? 0,
                ShowcaseCharacterIds = model.ShowAvatarInfoList?.Select(a => a.AvatarId).ToList() ?? new List<int>(),
                ShowcaseNameCardIds = model.ShowNameCardIdList ?? new List<int>(),
                ProfilePictureCharacterId = model.ProfilePicture?.Id ?? 0,
                ShowcaseNameCardIcons = model.ShowNameCardIdList?.Select(id => _assets.GetNameCardIconUrl(id)).ToList() ?? new List<string>(),
                NameCardIcon = _assets.GetNameCardIconUrl(model.NameCardId ?? 0),
                Challenge = new ChallengeData
                {
                    SpiralAbyss = new ChallengeData.SpiralAbyssData
                    {
                        Floor = model.TowerFloorIndex ?? 0,
                        Chamber = model.TowerLevelIndex ?? 0,
                        Star = model.TowerStarIndex ?? 0
                    },
                    Theater = new ChallengeData.TheatreData
                    {
                        Act = model.TheaterActIndex ?? 0,
                        Star = model.TheaterStarIndex ?? 0
                    }
                }
            };
            return playerInfo;
        }

        public List<Character> MapCharacters(List<AvatarInfoModel>? modelList)
        {
            if (modelList == null) return new List<Character>();
            return modelList.Select(model => MapCharacter(model)).ToList();
        }

        public Character MapCharacter(AvatarInfoModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            ElementType element = _assets.GetCharacterElement(model.AvatarId);
            if (element == ElementType.Unknown)
            {
                element = MapElementFallback(model.SkillDepotId, model.AvatarId);
            }

            var character = new Character
            {
                Id = model.AvatarId,
                Level = ParsePropValue(model.PropMap, "4001"),
                Ascension = ParsePropValue(model.PropMap, "1002"),
                Friendship = model.FetterInfo?.ExpLevel ?? 0,
                CostumeId = model.CostumeId ?? 0,
                Element = element,
                Stats = MapStats(model.FightPropMap),
                UnlockedConstellationIds = model.TalentIdList ?? new List<int>(),
                Talents = MapTalents(model.SkillLevelMap, model.ProudSkillExtraLevelMap, model.SkillDepotId, model.AvatarId),
                Weapon = model.EquipList?.Select(equipModel => MapEquipment(equipModel)).OfType<Weapon>().FirstOrDefault(),
                Artifacts = model.EquipList?.Select(equipModel => MapEquipment(equipModel)).OfType<Artifact>().ToList() ?? new List<Artifact>(),
                Name = _assets.GetCharacterName(model.AvatarId),
                IconUrl = _assets.GetCharacterIconUrl(model.AvatarId),
            };

            character.ConstellationLevel = character.UnlockedConstellationIds.Count;
            character.Constellations = character.UnlockedConstellationIds
                .Select(id => new Constellation
                {
                    Id = id,
                    Name = _assets.GetConstellationName(id),
                    IconUrl = _assets.GetConstellationIconUrl(id),
                    Position = character.UnlockedConstellationIds.IndexOf(id) + 1,
                })
                .ToList();
            return character;
        }

        public EquipmentBase? MapEquipment(EquipModel model)
        {
            if (model?.Flat == null) return null;
            ItemType itemType = MapItemType(model.Flat.ItemType);

            if (itemType == ItemType.Weapon && model.Weapon != null)
            {
                return MapWeapon(model, model.Weapon, model.Flat);
            }
            else if (itemType == ItemType.Artifact && model.Reliquary != null)
            {
                return MapArtifact(model, model.Reliquary, model.Flat);
            }
            return null;
        }

        private Weapon MapWeapon(EquipModel equip, WeaponModel weapon, FlatDataModel flat)
        {
            var baseAtkProp = flat.WeaponStats?.FirstOrDefault(s => s.AppendPropId == "FIGHT_PROP_BASE_ATTACK");
            var secondaryStatPropModel = flat.WeaponStats?.FirstOrDefault(s => s.AppendPropId != "FIGHT_PROP_BASE_ATTACK");
            var secondaryStat = MapStatProperty(secondaryStatPropModel);
            int refinementRank = (weapon.AffixMap?.Values.FirstOrDefault() ?? -1) + 1;

            WeaponType weaponType = _assets.GetWeaponType(equip.ItemId);
            if (weaponType == WeaponType.Unknown)
            {
                weaponType = MapWeaponTypeFromIcon(flat.Icon);
            }

            return new Weapon
            {
                Id = equip.ItemId,
                Level = weapon.Level,
                Ascension = weapon.PromoteLevel,
                Refinement = refinementRank,
                Rarity = flat.RankLevel,
                BaseAttack = baseAtkProp?.StatValue ?? 0,
                SecondaryStat = secondaryStat,
                Type = weaponType,
                Name = _assets.GetWeaponNameFromHash(flat.NameTextMapHash) ?? $"Weapon_{equip.ItemId}",
                IconUrl = _assets.GetWeaponIconUrlFromIconName(flat.Icon)
            };
        }

        private Artifact MapArtifact(EquipModel equip, ReliquaryModel reliquary, FlatDataModel flat)
        {
            var mainStat = MapStatProperty(flat.ReliquaryMainstat);
            var subStats = flat.ReliquarySubstats?
                              .Select(substatModel => MapStatProperty(substatModel))
                              .Where(s => s != null)
                              .Select(s => s!)
                              .ToList() ?? new List<StatProperty>();

            return new Artifact
            {
                Id = equip.ItemId,
                Level = reliquary.Level - 1,
                Rarity = flat.RankLevel,
                Slot = MapArtifactSlot(flat.EquipType),
                MainStat = mainStat,
                SubStats = subStats,
                SetName = _assets.GetArtifactSetNameFromHash(flat.SetNameTextMapHash) ?? "Unknown Set",
                Name = _assets.GetArtifactNameFromHash(flat.NameTextMapHash) ?? $"Artifact_{equip.ItemId}",
                IconUrl = _assets.GetArtifactIconUrlFromIconName(flat.Icon)
            };
        }

        private ElementType MapElementFallback(int skillDepotId, int avatarId)
        {
            if (avatarId == 10000005 || avatarId == 10000007) // Lumine or Aether
            {
                switch (skillDepotId)
                {
                    case 504: return ElementType.Anemo;
                    case 507: return ElementType.Dendro;
                    case 704: return ElementType.Geo;
                    case 201: case 506: return ElementType.Electro;
                    case 508: return ElementType.Hydro;
                    default: return ElementType.Unknown;
                }
            }
            switch (avatarId)
            {
                case 10000016: return ElementType.Geo;
                case 10000002: return ElementType.Cryo;
                case 10000046: return ElementType.Hydro;
                case 10000052: return ElementType.Electro;
                case 10000030: return ElementType.Electro;
                case 10000041: return ElementType.Cryo;
                case 10000043: return ElementType.Anemo;
                case 10000058: return ElementType.Anemo;
                case 10000069: return ElementType.Dendro;
                default: return ElementType.Unknown;
            }
        }

        private WeaponType MapWeaponTypeFromIcon(string? iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName)) return WeaponType.Unknown;
            if (iconName.Contains("Sword")) return WeaponType.Sword;
            if (iconName.Contains("Claymore")) return WeaponType.Claymore;
            if (iconName.Contains("Pole")) return WeaponType.Polearm;
            if (iconName.Contains("Bow")) return WeaponType.Bow;
            if (iconName.Contains("Catalyst")) return WeaponType.Catalyst;
            return WeaponType.Unknown;
        }

        private ArtifactSlot MapArtifactSlot(string? equipType)
        {
            return equipType switch
            {
                "EQUIP_BRACER" => ArtifactSlot.Flower,
                "EQUIP_NECKLACE" => ArtifactSlot.Plume,
                "EQUIP_SHOES" => ArtifactSlot.Sands,
                "EQUIP_RING" => ArtifactSlot.Goblet,
                "EQUIP_DRESS" => ArtifactSlot.Circlet,
                _ => ArtifactSlot.Unknown
            };
        }

        private Dictionary<StatType, double> MapStats(Dictionary<string, double>? fightPropMap)
        {
            var stats = new Dictionary<StatType, double>();
            if (fightPropMap == null) return stats;
            foreach (var kvp in fightPropMap)
            {
                StatType statType = MapStatTypeKey(kvp.Key);
                if (statType != StatType.None && (!stats.ContainsKey(statType) || IsFinalStatKey(kvp.Key)))
                {
                    stats[statType] = kvp.Value;
                }
            }
            return stats;
        }

        private List<Talent> MapTalents(Dictionary<string, int>? skillLevelMap, Dictionary<string, int>? proudSkillExtraLevelMap, int skillDepotId, int avatarId)
        {
            var talents = new List<Talent>();
            if (skillLevelMap == null) return talents;

            foreach (var kvp in skillLevelMap)
            {
                if (!int.TryParse(kvp.Key, out int skillId)) continue;

                int baseLevel = kvp.Value;
                int extraLevel = 0;
                proudSkillExtraLevelMap?.TryGetValue(kvp.Key, out extraLevel);

                talents.Add(new Talent
                {
                    Id = skillId,
                    BaseLevel = baseLevel,
                    ExtraLevel = extraLevel,
                    Level = baseLevel + extraLevel,
                    Name = _assets.GetTalentName(skillId),
                    IconUrl = _assets.GetTalentIconUrl(skillId)
                });
            }
            return talents;
        }

        private StatProperty? MapStatProperty(StatPropertyModel? model)
        {
            if (model == null) return null;

            string? propId = model.MainPropId ?? model.AppendPropId;

            if (string.IsNullOrEmpty(propId)) return null;

            StatType type = MapStatTypeValue(propId);
            if (type == StatType.None) return null;

            return new StatProperty { Type = type, Value = model.StatValue };
        }

        private StatType MapStatTypeKey(string key)
        {
            return key switch
            {
                "1" => StatType.BaseHP,
                "4" => StatType.BaseAttack,
                "7" => StatType.BaseDefense,
                "10" => StatType.BaseSpeed,
                "2" => StatType.HPPercentage,
                "5" => StatType.AttackPercentage,
                "8" => StatType.DefensePercentage,
                "11" => StatType.SpeedPercentage,
                "20" => StatType.CriticalRate,
                "22" => StatType.CriticalDamage,
                "23" => StatType.EnergyRecharge,
                "28" => StatType.ElementalMastery,
                "26" => StatType.HealingBonus,
                "27" => StatType.IncomingHealingBonus,
                "29" => StatType.PhysicalResistance,
                "50" => StatType.PyroResistance,
                "51" => StatType.ElectroResistance,
                "52" => StatType.HydroResistance,
                "53" => StatType.DendroResistance,
                "54" => StatType.AnemoResistance,
                "55" => StatType.GeoResistance,
                "56" => StatType.CryoResistance,
                "30" => StatType.PhysicalDamageBonus,
                "40" => StatType.PyroDamageBonus,
                "41" => StatType.ElectroDamageBonus,
                "42" => StatType.HydroDamageBonus,
                "43" => StatType.DendroDamageBonus,
                "44" => StatType.AnemoDamageBonus,
                "45" => StatType.GeoDamageBonus,
                "46" => StatType.CryoDamageBonus,
                "70" => StatType.PyroEnergyCost,
                "71" => StatType.ElectroEnergyCost,
                "72" => StatType.HydroEnergyCost,
                "73" => StatType.DendroEnergyCost,
                "74" => StatType.AnemoEnergyCost,
                "75" => StatType.CryoEnergyCost,
                "76" => StatType.GeoEnergyCost,
                "77" => StatType.MaxSpecialEnergy,
                "78" => StatType.SpecialEnergyCost,
                "1000" => StatType.CurrentPyroEnergy,
                "1001" => StatType.CurrentElectroEnergy,
                "1002" => StatType.CurrentHydroEnergy,
                "1003" => StatType.CurrentDendroEnergy,
                "1004" => StatType.CurrentAnemoEnergy,
                "1005" => StatType.CurrentCryoEnergy,
                "1006" => StatType.CurrentGeoEnergy,
                "1007" => StatType.CurrentSpecialEnergy,
                "1010" => StatType.CurrentHP,
                "2000" => StatType.HP,
                "2001" => StatType.Attack,
                "2002" => StatType.Defense,
                "2003" => StatType.Speed,
                "80" => StatType.CooldownReduction,
                "81" => StatType.ShieldStrength,
                "3025" => StatType.ElementalReactionCritRate,
                "3026" => StatType.ElementalReactionCritDamage,
                "3027" => StatType.OverloadedCritRate,
                "3028" => StatType.OverloadedCritDamage,
                "3029" => StatType.SwirlCritRate,
                "3030" => StatType.SwirlCritDamage,
                "3031" => StatType.ElectroChargedCritRate,
                "3032" => StatType.ElectroChargedCritDamage,
                "3033" => StatType.SuperconductCritRate,
                "3034" => StatType.SuperconductCritDamage,
                "3035" => StatType.BurnCritRate,
                "3036" => StatType.BurnCritDamage,
                "3037" => StatType.ShatteredCritRate,
                "3038" => StatType.ShatteredCritDamage,
                "3039" => StatType.BloomCritRate,
                "3040" => StatType.BloomCritDamage,
                "3041" => StatType.BurgeonCritRate,
                "3042" => StatType.BurgeonCritDamage,
                "3043" => StatType.HyperbloomCritRate,
                "3044" => StatType.HyperbloomCritDamage,
                "3045" => StatType.BaseElementalReactionCritRate,
                "3046" => StatType.BaseElementalReactionCritDamage,
                _ => StatType.None
            };
        }

        private StatType MapStatTypeValue(string propIdString)
        {
            return propIdString switch
            {
                "FIGHT_PROP_HP" => StatType.BaseHP,
                "FIGHT_PROP_ATTACK" => StatType.BaseAttack,
                "FIGHT_PROP_DEFENSE" => StatType.BaseDefense,
                "FIGHT_PROP_SPEED" => StatType.BaseSpeed,
                "FIGHT_PROP_BASE_HP" => StatType.BaseHP,
                "FIGHT_PROP_BASE_ATTACK" => StatType.BaseAttack,
                "FIGHT_PROP_BASE_DEFENSE" => StatType.BaseDefense,
                "FIGHT_PROP_BASE_SPEED" => StatType.BaseSpeed,
                "FIGHT_PROP_HP_PERCENT" => StatType.HPPercentage,
                "FIGHT_PROP_ATTACK_PERCENT" => StatType.AttackPercentage,
                "FIGHT_PROP_DEFENSE_PERCENT" => StatType.DefensePercentage,
                "FIGHT_PROP_SPEED_PERCENT" => StatType.SpeedPercentage,
                "FIGHT_PROP_CRITICAL" => StatType.CriticalRate,
                "FIGHT_PROP_CRITICAL_HURT" => StatType.CriticalDamage,
                "FIGHT_PROP_CHARGE_EFFICIENCY" => StatType.EnergyRecharge,
                "FIGHT_PROP_ELEMENT_MASTERY" => StatType.ElementalMastery,
                "FIGHT_PROP_HEAL_ADD" => StatType.HealingBonus,
                "FIGHT_PROP_HEALED_ADD" => StatType.IncomingHealingBonus,
                "FIGHT_PROP_PHYSICAL_SUB_HURT" => StatType.PhysicalResistance,
                "FIGHT_PROP_FIRE_SUB_HURT" => StatType.PyroResistance,
                "FIGHT_PROP_ELEC_SUB_HURT" => StatType.ElectroResistance,
                "FIGHT_PROP_WATER_SUB_HURT" => StatType.HydroResistance,
                "FIGHT_PROP_GRASS_SUB_HURT" => StatType.DendroResistance,
                "FIGHT_PROP_WIND_SUB_HURT" => StatType.AnemoResistance,
                "FIGHT_PROP_ROCK_SUB_HURT" => StatType.GeoResistance,
                "FIGHT_PROP_ICE_SUB_HURT" => StatType.CryoResistance,
                "FIGHT_PROP_PHYSICAL_ADD_HURT" => StatType.PhysicalDamageBonus,
                "FIGHT_PROP_FIRE_ADD_HURT" => StatType.PyroDamageBonus,
                "FIGHT_PROP_ELEC_ADD_HURT" => StatType.ElectroDamageBonus,
                "FIGHT_PROP_WATER_ADD_HURT" => StatType.HydroDamageBonus,
                "FIGHT_PROP_GRASS_ADD_HURT" => StatType.DendroDamageBonus,
                "FIGHT_PROP_WIND_ADD_HURT" => StatType.AnemoDamageBonus,
                "FIGHT_PROP_ROCK_ADD_HURT" => StatType.GeoDamageBonus,
                "FIGHT_PROP_ICE_ADD_HURT" => StatType.CryoDamageBonus,
                "FIGHT_PROP_FIRE_ENERGY_COST" => StatType.PyroEnergyCost,
                "FIGHT_PROP_ELEC_ENERGY_COST" => StatType.ElectroEnergyCost,
                "FIGHT_PROP_WATER_ENERGY_COST" => StatType.HydroEnergyCost,
                "FIGHT_PROP_GRASS_ENERGY_COST" => StatType.DendroEnergyCost,
                "FIGHT_PROP_WIND_ENERGY_COST" => StatType.AnemoEnergyCost,
                "FIGHT_PROP_ICE_ENERGY_COST" => StatType.CryoEnergyCost,
                "FIGHT_PROP_ROCK_ENERGY_COST" => StatType.GeoEnergyCost,
                "FIGHT_PROP_MAX_SP" => StatType.MaxSpecialEnergy,
                "FIGHT_PROP_SP_COST" => StatType.SpecialEnergyCost,
                "FIGHT_PROP_CUR_FIRE_ENERGY" => StatType.CurrentPyroEnergy,
                "FIGHT_PROP_CUR_ELEC_ENERGY" => StatType.CurrentElectroEnergy,
                "FIGHT_PROP_CUR_WATER_ENERGY" => StatType.CurrentHydroEnergy,
                "FIGHT_PROP_CUR_GRASS_ENERGY" => StatType.CurrentDendroEnergy,
                "FIGHT_PROP_CUR_WIND_ENERGY" => StatType.CurrentAnemoEnergy,
                "FIGHT_PROP_CUR_ICE_ENERGY" => StatType.CurrentCryoEnergy,
                "FIGHT_PROP_CUR_ROCK_ENERGY" => StatType.CurrentGeoEnergy,
                "FIGHT_PROP_CUR_SP" => StatType.CurrentSpecialEnergy,
                "FIGHT_PROP_CUR_HP" => StatType.CurrentHP,
                "FIGHT_PROP_MAX_HP" => StatType.HP,
                "FIGHT_PROP_CUR_ATTACK" => StatType.Attack,
                "FIGHT_PROP_CUR_DEFENSE" => StatType.Defense,
                "FIGHT_PROP_CUR_SPEED" => StatType.Speed,
                "FIGHT_PROP_SKILL_CD_MINUS_RATIO" => StatType.CooldownReduction,
                "FIGHT_PROP_SHIELD_COST_MINUS_RATIO" => StatType.ShieldStrength,
                "FIGHT_PROP_ELEMENT_REACTION_CRIT_RATE" => StatType.ElementalReactionCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_CRIT_DMG" => StatType.ElementalReactionCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_OVERLOADED_CRIT_RATE" => StatType.OverloadedCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_OVERLOADED_CRIT_DMG" => StatType.OverloadedCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_SWIRL_CRIT_RATE" => StatType.SwirlCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_SWIRL_CRIT_DMG" => StatType.SwirlCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_ELECTRIC_CRIT_RATE" => StatType.ElectroChargedCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_ELECTRIC_CRIT_DMG" => StatType.ElectroChargedCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_SUPERCONDUCT_CRIT_RATE" => StatType.SuperconductCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_SUPERCONDUCT_CRIT_DMG" => StatType.SuperconductCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_BURN_CRIT_RATE" => StatType.BurnCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_BURN_CRIT_DMG" => StatType.BurnCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_FROZEN_CRIT_RATE" => StatType.ShatteredCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_FROZEN_CRIT_DMG" => StatType.ShatteredCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_BLOOM_CRIT_RATE" => StatType.BloomCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_BLOOM_CRIT_DMG" => StatType.BloomCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_BURGEON_CRIT_RATE" => StatType.BurgeonCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_BURGEON_CRIT_DMG" => StatType.BurgeonCritDamage,
                "FIGHT_PROP_ELEMENT_REACTION_HYPERBLOOM_CRIT_RATE" => StatType.HyperbloomCritRate,
                "FIGHT_PROP_ELEMENT_REACTION_HYPERBLOOM_CRIT_DMG" => StatType.HyperbloomCritDamage,
                "FIGHT_PROP_BASE_ELEMENT_REACTION_CRIT_RATE" => StatType.BaseElementalReactionCritRate,
                "FIGHT_PROP_BASE_ELEMENT_REACTION_CRIT_DMG" => StatType.BaseElementalReactionCritDamage,
                _ => StatType.None
            };
        }

        private ItemType MapItemType(string? itemTypeString)
        {
            return itemTypeString switch
            {
                "ITEM_WEAPON" => ItemType.Weapon,
                "ITEM_RELIQUARY" => ItemType.Artifact,
                _ => ItemType.Unknown
            };
        }

        private int ParsePropValue(Dictionary<string, PropValueModel>? propMap, string key)
        {
            if (propMap != null && propMap.TryGetValue(key, out var propValue) && int.TryParse(propValue?.Val, out int result))
            {
                return result;
            }
            return 0;
        }

        private bool IsFinalStatKey(string key) => int.TryParse(key, out int id) && id >= 2000 && id < 3000;
    }
}