using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Models.Genshin;

namespace EnkaDotNet.Utils.Genshin
{
    public class DataMapper
    {
        private readonly IGenshinAssets _assets;
        private readonly EnkaClientOptions _options;

        public DataMapper(IGenshinAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public PlayerInfo MapPlayerInfo(PlayerInfoModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var showcaseCharacterIds = new List<int>();
            if (model.ShowAvatarInfoList != null)
            {
                foreach (var a in model.ShowAvatarInfoList)
                {
                    showcaseCharacterIds.Add(a.AvatarId);
                }
            }

            var showcaseNameCards = new List<NameCard>();
            if (model.ShowNameCardIdList != null)
            {
                foreach (var id in model.ShowNameCardIdList)
                {
                    string iconUrl = _assets?.GetNameCardIconUrl(id) ?? string.Empty;
                    showcaseNameCards.Add(new NameCard(id, iconUrl));
                }
            }

            var playerInfo = new PlayerInfo
            {
                Nickname = model.Nickname ?? "Unknown",
                Level = model.Level,
                Signature = model.Signature ?? "",
                IconUrl = _assets?.GetProfilePictureIconUrl(model.ProfilePicture?.Id ?? 0) ?? string.Empty,
                WorldLevel = model.WorldLevel,
                NameCardId = model.NameCardId,
                FinishedAchievements = model.FinishAchievementNum,
                ShowcaseCharacterIds = showcaseCharacterIds,
                ShowcaseNameCards = showcaseNameCards,
                ProfilePictureCharacterId = model.ProfilePicture?.Id ?? 0,
                NameCardIcon = _assets?.GetNameCardIconUrl(model.NameCardId) ?? string.Empty
            };

            playerInfo.Challenge = new ChallengeData();
            playerInfo.Challenge.SpiralAbyss = new ChallengeData.SpiralAbyssData
            {
                Floor = model.TowerFloorIndex,
                Chamber = model.TowerLevelIndex,
                Star = model.TowerStarIndex
            };
            playerInfo.Challenge.Theater = new ChallengeData.TheatreData
            {
                Act = model.TheaterActIndex,
                Star = model.TheaterStarIndex
            };

            return playerInfo;
        }

        public IReadOnlyList<Character> MapCharacters(List<AvatarInfoModel> modelList)
        {
            if (modelList == null) return new List<Character>();

            var characters = new List<Character>();
            foreach (var model in modelList)
            {
                characters.Add(MapCharacter(model));
            }
            return characters;
        }

        public Character MapCharacter(AvatarInfoModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            ElementType element = _assets?.GetCharacterElement(model.AvatarId) ?? ElementType.Unknown;
            if (element == ElementType.Unknown && _assets != null)
            {
                element = MapElementFallback(model.SkillDepotId, model.AvatarId);
            }

            Weapon weapon = null;
            var artifacts = new List<Artifact>();
            if (model.EquipList != null)
            {
                foreach (var equipModel in model.EquipList)
                {
                    var equipment = MapEquipment(equipModel);
                    if (equipment is Weapon w)
                    {
                        weapon = w;
                    }
                    else if (equipment is Artifact a)
                    {
                        artifacts.Add(a);
                    }
                }
            }

            var unlockedConstellationIds = new List<int>(model.TalentIdList ?? new List<int>());
            var constellations = new List<Constellation>();
            int index = 0;
            foreach (var id in unlockedConstellationIds)
            {
                constellations.Add(new Constellation
                {
                    Id = id,
                    Name = _assets?.GetConstellationName(id) ?? $"Constellation_{id}",
                    IconUrl = _assets?.GetConstellationIconUrl(id) ?? string.Empty,
                    Position = index + 1,
                    Options = this._options
                });
                index++;
            }

            var character = new Character
            {
                Id = model.AvatarId,
                Level = ParsePropValue(model.PropMap, "4001"),
                Ascension = ParsePropValue(model.PropMap, "1002"),
                Friendship = model.FetterInfo?.ExpLevel ?? 0,
                CostumeId = model.CostumeId,
                Element = element,
                Stats = new ConcurrentDictionary<StatType, double>(MapStats(model.FightPropMap)),
                UnlockedConstellationIds = unlockedConstellationIds,
                Talents = MapTalents(model.SkillLevelMap, model.ProudSkillExtraLevelMap),
                Weapon = weapon,
                Artifacts = artifacts,
                Name = _assets?.GetCharacterName(model.AvatarId) ?? $"Character_{model.AvatarId}",
                IconUrl = _assets?.GetCharacterIconUrl(model.AvatarId) ?? string.Empty,
                Options = this._options,
                ConstellationLevel = unlockedConstellationIds.Count,
                Constellations = constellations
            };

            if (character.Weapon != null) character.Weapon.Options = this._options;
            foreach (var artifact in character.Artifacts) artifact.Options = this._options;
            foreach (var talent in character.Talents) talent.Options = this._options;

            return character;
        }

        public EquipmentBase MapEquipment(EquipModel model)
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
            StatPropertyModel baseAtkProp = null;
            StatPropertyModel secondaryStatPropModel = null;

            if (flat.WeaponStats != null)
            {
                foreach (var s in flat.WeaponStats)
                {
                    if (s.AppendPropId == "FIGHT_PROP_BASE_ATTACK")
                    {
                        baseAtkProp = s;
                    }
                    else
                    {
                        secondaryStatPropModel = s;
                    }
                }
            }

            var secondaryStat = secondaryStatPropModel != null ? MapStatProperty(secondaryStatPropModel) : null;
            if (secondaryStat != null) secondaryStat.Options = this._options;

            int refinementValue = -1;
            if (weapon.AffixMap != null)
            {
                foreach (var val in weapon.AffixMap.Values)
                {
                    refinementValue = val;
                    break;
                }
            }
            int refinementRank = refinementValue + 1;

            WeaponType weaponType = MapWeaponTypeFromIcon(flat.Icon);

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
                Name = _assets?.GetWeaponNameFromHash(flat.NameTextMapHash) ?? $"Weapon_{equip.ItemId}",
                IconUrl = _assets?.GetWeaponIconUrlFromIconName(flat.Icon) ?? string.Empty,
                Options = this._options
            };
        }

        private Artifact MapArtifact(EquipModel equip, ReliquaryModel reliquary, FlatDataModel flat)
        {
            var mainStat = MapStatProperty(flat.ReliquaryMainstat) ?? new StatProperty { Type = StatType.None, Value = 0, Options = this._options };
            if (mainStat != null) mainStat.Options = this._options;

            var subStats = new List<StatProperty>();
            if (flat.ReliquarySubstats != null)
            {
                foreach (var substatModel in flat.ReliquarySubstats)
                {
                    var stat = MapStatProperty(substatModel);
                    if (stat != null)
                    {
                        subStats.Add(stat);
                    }
                }
            }
            foreach (var subStat in subStats) subStat.Options = this._options;

            return new Artifact
            {
                Id = equip.ItemId,
                Level = reliquary.Level > 0 ? reliquary.Level - 1 : 0,
                Rarity = flat.RankLevel,
                Slot = MapArtifactSlot(flat.EquipType),
                MainStat = mainStat,
                SubStats = subStats,
                SetName = _assets?.GetArtifactSetNameFromHash(flat.SetNameTextMapHash) ?? "Unknown Set",
                Name = _assets?.GetArtifactNameFromHash(flat.NameTextMapHash) ?? $"Artifact_{equip.ItemId}",
                IconUrl = _assets?.GetArtifactIconUrlFromIconName(flat.Icon) ?? string.Empty,
                Options = this._options
            };
        }

        private ElementType MapElementFallback(int skillDepotId, int avatarId)
        {
            if (avatarId == 10000005 || avatarId == 10000007)
            {
                switch (skillDepotId)
                {
                    case 504: return ElementType.Anemo;
                    case 704: return ElementType.Geo;
                    case 506: return ElementType.Electro;
                    case 507: return ElementType.Dendro;
                    case 508: return ElementType.Hydro;
                    default: return ElementType.Unknown;
                }
            }
            switch (avatarId)
            {
                case 10000016: return ElementType.Geo;
                default: return ElementType.Unknown;
            }
        }

        private WeaponType MapWeaponTypeFromIcon(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName)) return WeaponType.Unknown;
            if (iconName.Contains("Sword")) return WeaponType.Sword;
            if (iconName.Contains("Claymore")) return WeaponType.Claymore;
            if (iconName.Contains("Pole")) return WeaponType.Polearm;
            if (iconName.Contains("Bow")) return WeaponType.Bow;
            if (iconName.Contains("Catalyst")) return WeaponType.Catalyst;
            return WeaponType.Unknown;
        }

        private ArtifactSlot MapArtifactSlot(string equipType)
        {
            switch (equipType)
            {
                case "EQUIP_BRACER": return ArtifactSlot.Flower;
                case "EQUIP_NECKLACE": return ArtifactSlot.Plume;
                case "EQUIP_SHOES": return ArtifactSlot.Sands;
                case "EQUIP_RING": return ArtifactSlot.Goblet;
                case "EQUIP_DRESS": return ArtifactSlot.Circlet;
                default: return ArtifactSlot.Unknown;
            }
        }

        private ConcurrentDictionary<StatType, double> MapStats(Dictionary<string, double> fightPropMap)
        {
            var stats = new ConcurrentDictionary<StatType, double>();
            if (fightPropMap == null) return stats;

            foreach (var kvp in fightPropMap)
            {
                StatType statType = MapStatTypeKey(kvp.Key);
                if (statType != StatType.None)
                {
                    if (!stats.ContainsKey(statType) || IsFinalStatKey(kvp.Key))
                    {
                        stats[statType] = kvp.Value;
                    }
                }
            }
            return stats;
        }

        private IReadOnlyList<Talent> MapTalents(Dictionary<string, int> skillLevelMap, Dictionary<string, int> proudSkillExtraLevelMap)
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
                    Name = _assets?.GetTalentName(skillId) ?? $"Talent_{skillId}",
                    IconUrl = _assets?.GetTalentIconUrl(skillId) ?? string.Empty,
                    Options = this._options
                });
            }
            return talents;
        }

        private StatProperty MapStatProperty(StatPropertyModel model)
        {
            if (model == null) return null;
            string propIdString = model.MainPropId ?? model.AppendPropId;
            if (string.IsNullOrEmpty(propIdString)) return null;
            StatType type = MapStatTypeValue(propIdString);
            if (type == StatType.None) return null;
            return new StatProperty { Type = type, Value = model.StatValue, Options = this._options };
        }

        private StatType MapStatTypeKey(string key)
        {
            switch (key)
            {
                case "1": return StatType.BaseHP;
                case "4": return StatType.BaseAttack;
                case "7": return StatType.BaseDefense;
                case "10": return StatType.BaseSpeed;
                case "2": return StatType.HPPercentage;
                case "5": return StatType.AttackPercentage;
                case "8": return StatType.DefensePercentage;
                case "11": return StatType.SpeedPercentage;
                case "20": return StatType.CriticalRate;
                case "22": return StatType.CriticalDamage;
                case "23": return StatType.EnergyRecharge;
                case "28": return StatType.ElementalMastery;
                case "26": return StatType.HealingBonus;
                case "27": return StatType.IncomingHealingBonus;
                case "29": return StatType.PhysicalResistance;
                case "50": return StatType.PyroResistance;
                case "51": return StatType.ElectroResistance;
                case "52": return StatType.HydroResistance;
                case "53": return StatType.DendroResistance;
                case "54": return StatType.AnemoResistance;
                case "55": return StatType.GeoResistance;
                case "56": return StatType.CryoResistance;
                case "30": return StatType.PhysicalDamageBonus;
                case "40": return StatType.PyroDamageBonus;
                case "41": return StatType.ElectroDamageBonus;
                case "42": return StatType.HydroDamageBonus;
                case "43": return StatType.DendroDamageBonus;
                case "44": return StatType.AnemoDamageBonus;
                case "45": return StatType.GeoDamageBonus;
                case "46": return StatType.CryoDamageBonus;
                case "70": return StatType.PyroEnergyCost;
                case "71": return StatType.ElectroEnergyCost;
                case "72": return StatType.HydroEnergyCost;
                case "73": return StatType.DendroEnergyCost;
                case "74": return StatType.AnemoEnergyCost;
                case "75": return StatType.CryoEnergyCost;
                case "76": return StatType.GeoEnergyCost;
                case "77": return StatType.MaxSpecialEnergy;
                case "78": return StatType.SpecialEnergyCost;
                case "1000": return StatType.CurrentPyroEnergy;
                case "1001": return StatType.CurrentElectroEnergy;
                case "1002": return StatType.CurrentHydroEnergy;
                case "1003": return StatType.CurrentDendroEnergy;
                case "1004": return StatType.CurrentAnemoEnergy;
                case "1005": return StatType.CurrentCryoEnergy;
                case "1006": return StatType.CurrentGeoEnergy;
                case "1007": return StatType.CurrentSpecialEnergy;
                case "1010": return StatType.CurrentHP;
                case "2000": return StatType.HP;
                case "2001": return StatType.Attack;
                case "2002": return StatType.Defense;
                case "2003": return StatType.Speed;
                case "80": return StatType.CooldownReduction;
                case "81": return StatType.ShieldStrength;
                case "3025": return StatType.ElementalReactionCritRate;
                case "3026": return StatType.ElementalReactionCritDamage;
                case "3045": return StatType.BaseElementalReactionCritRate;
                case "3046": return StatType.BaseElementalReactionCritDamage;
                default: return StatType.None;
            }
        }

        private StatType MapStatTypeValue(string propIdString)
        {
            switch (propIdString)
            {
                case "FIGHT_PROP_HP": return StatType.HP;
                case "FIGHT_PROP_ATTACK": return StatType.Attack;
                case "FIGHT_PROP_DEFENSE": return StatType.Defense;
                case "FIGHT_PROP_BASE_HP": return StatType.BaseHP;
                case "FIGHT_PROP_BASE_ATTACK": return StatType.BaseAttack;
                case "FIGHT_PROP_BASE_DEFENSE": return StatType.BaseDefense;
                case "FIGHT_PROP_HP_PERCENT": return StatType.HPPercentage;
                case "FIGHT_PROP_ATTACK_PERCENT": return StatType.AttackPercentage;
                case "FIGHT_PROP_DEFENSE_PERCENT": return StatType.DefensePercentage;
                case "FIGHT_PROP_CRITICAL": return StatType.CriticalRate;
                case "FIGHT_PROP_CRITICAL_HURT": return StatType.CriticalDamage;
                case "FIGHT_PROP_CHARGE_EFFICIENCY": return StatType.EnergyRecharge;
                case "FIGHT_PROP_ELEMENT_MASTERY": return StatType.ElementalMastery;
                case "FIGHT_PROP_HEAL_ADD": return StatType.HealingBonus;
                case "FIGHT_PROP_PHYSICAL_SUB_HURT": return StatType.PhysicalResistance;
                case "FIGHT_PROP_FIRE_SUB_HURT": return StatType.PyroResistance;
                case "FIGHT_PROP_ELEC_SUB_HURT": return StatType.ElectroResistance;
                case "FIGHT_PROP_WATER_SUB_HURT": return StatType.HydroResistance;
                case "FIGHT_PROP_GRASS_SUB_HURT": return StatType.DendroResistance;
                case "FIGHT_PROP_WIND_SUB_HURT": return StatType.AnemoResistance;
                case "FIGHT_PROP_ROCK_SUB_HURT": return StatType.GeoResistance;
                case "FIGHT_PROP_ICE_SUB_HURT": return StatType.CryoResistance;
                case "FIGHT_PROP_PHYSICAL_ADD_HURT": return StatType.PhysicalDamageBonus;
                case "FIGHT_PROP_FIRE_ADD_HURT": return StatType.PyroDamageBonus;
                case "FIGHT_PROP_ELEC_ADD_HURT": return StatType.ElectroDamageBonus;
                case "FIGHT_PROP_WATER_ADD_HURT": return StatType.HydroDamageBonus;
                case "FIGHT_PROP_GRASS_ADD_HURT": return StatType.DendroDamageBonus;
                case "FIGHT_PROP_WIND_ADD_HURT": return StatType.AnemoDamageBonus;
                case "FIGHT_PROP_ROCK_ADD_HURT": return StatType.GeoDamageBonus;
                case "FIGHT_PROP_ICE_ADD_HURT": return StatType.CryoDamageBonus;
                case "FIGHT_PROP_MAX_HP": return StatType.HP;
                case "FIGHT_PROP_CUR_ATTACK": return StatType.Attack;
                case "FIGHT_PROP_CUR_DEFENSE": return StatType.Defense;
                default: return StatType.None;
            }
        }

        private ItemType MapItemType(string itemTypeString)
        {
            switch (itemTypeString)
            {
                case "ITEM_WEAPON": return ItemType.Weapon;
                case "ITEM_RELIQUARY": return ItemType.Artifact;
                default: return ItemType.Unknown;
            }
        }

        private int ParsePropValue(Dictionary<string, PropValueModel> propMap, string key)
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
