using System;
using System.Collections.Generic;
using System.Linq;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Models.Genshin;

namespace EnkaDotNet.Mapping
{
    public class GenshinMapper
    {
        private readonly IGenshinAssets _assets;

        public GenshinMapper(IGenshinAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        // Update to handle either PlayerInfo or PlayerProfile
        public PlayerInfo MapPlayerInfo(PlayerInfoModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var playerInfo = new PlayerInfo
            {
                Nickname = model.Nickname ?? "Unknown",
                Level = model.Level,
                Signature = model.Signature ?? "",
                WorldLevel = model.WorldLevel,
                FinishedAchievements = model.FinishAchievementNum,
                TowerFloor = model.TowerFloorIndex,
                TowerChamber = model.TowerLevelIndex,
                NameCardId = model.NameCardId,
                NameCardIcon = _assets.GetNameCardIconUrl(model.NameCardId),
                ShowcaseCharacterIds = model.ShowAvatarInfoList?.Select(a => a.AvatarId).ToList() ?? new List<int>(),
                ShowcaseNameCardIds = model.ShowNameCardIdList ?? new List<int>(),
                ShowcaseNameCardIcons = model.ShowNameCardIdList?.Select(id => _assets.GetNameCardIconUrl(id)).ToList() ?? new List<string>(),
                ProfilePictureCharacterId = model.ProfilePicture?.AvatarId ?? 0
            };

            // Map profile picture
            if (model.ProfilePicture?.AvatarId > 0)
            {
                string pfpIcon = _assets.GetPfpIconUrl(model.ProfilePicture.AvatarId);
                if (!string.IsNullOrEmpty(pfpIcon))
                {
                    playerInfo.ProfilePicture.Add(new Profile
                    {
                        Id = model.ProfilePicture.AvatarId,
                        Icon = pfpIcon
                    });
                }
            }

            return playerInfo;
        }

        // Create extension method to convert PlayerInfo to PlayerShowcaseCard
        public PlayerShowcaseCard ToShowcaseCard(PlayerInfo playerInfo)
        {
            var characters = playerInfo.ShowcaseCharacterIds
                .Select(id => new CharacterSummary
                {
                    Id = id,
                    Name = _assets.GetCharacterName(id),
                    IconUrl = _assets.GetCharacterIconUrl(id),
                    Element = _assets.GetCharacterElement(id)
                })
                .ToList();

            return new PlayerShowcaseCard(playerInfo, characters);
        }

        // Modified to handle ShowAvatarInfoModel instead of AvatarInfoModel
        public List<CharacterSummary> MapCharacterSummaries(List<ShowAvatarInfoModel>? avatars)
        {
            if (avatars == null || avatars.Count == 0)
                return new List<CharacterSummary>();

            return avatars
                .Select(a => new CharacterSummary
                {
                    Id = a.AvatarId,
                    Name = _assets.GetCharacterName(a.AvatarId),
                    IconUrl = _assets.GetCharacterIconUrl(a.AvatarId),
                    Element = _assets.GetCharacterElement(a.AvatarId)
                })
                .ToList();
        }

        public Character MapCharacter(AvatarInfoModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            try
            {
                ElementType element = _assets.GetCharacterElement(model.AvatarId);

                var character = new Character
                {
                    Id = model.AvatarId,
                    Name = _assets.GetCharacterName(model.AvatarId),
                    IconUrl = _assets.GetCharacterIconUrl(model.AvatarId),
                    Level = ParsePropValue(model.PropMap, "4001"),
                    Ascension = ParsePropValue(model.PropMap, "1002"),
                    Friendship = model.FetterInfo?.ExpLevel ?? 0,
                    CostumeId = model.CostumeId ?? 0,
                    Element = element,
                    Stats = MapStats(model.FightPropMap),
                    UnlockedConstellationIds = model.TalentIdList ?? new List<int>()
                };

                // Map constellations
                character.ConstellationLevel = character.UnlockedConstellationIds.Count;
                character.Constellations = character.UnlockedConstellationIds
                    .Select((id, index) => new Constellation
                    {
                        Id = id,
                        Name = _assets.GetConstellationName(id),
                        IconUrl = _assets.GetConstellationIconUrl(id),
                        Position = index + 1,
                    })
                    .ToList();

                // Map talents
                character.Talents = MapTalents(model.SkillLevelMap, model.ProudSkillExtraLevelMap);

                // Map equipment
                if (model.EquipList != null)
                {
                    character.Weapon = model.EquipList
                        .Select(MapEquipment)
                        .OfType<Weapon>()
                        .FirstOrDefault();

                    character.Artifacts = model.EquipList
                        .Select(MapEquipment)
                        .OfType<Artifact>()
                        .ToList();
                }

                return character;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Mapper] Error mapping character: {ex.Message}");
                throw;
            }
        }

        // Helper methods
        private EquipmentBase? MapEquipment(EquipModel model)
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

        private Dictionary<StatType, double> MapStats(Dictionary<string, double>? fightPropMap)
        {
            var stats = new Dictionary<StatType, double>();
            if (fightPropMap == null) return stats;

            foreach (var kvp in fightPropMap)
            {
                StatType statType = MapStatTypeKey(kvp.Key);
                if (statType != StatType.None)
                {
                    stats[statType] = kvp.Value;
                }
            }

            return stats;
        }

        private StatType MapStatTypeKey(string key)
        {
            return key switch
            {
                "2000" => StatType.HP,
                "2001" => StatType.Attack,
                "2002" => StatType.Defense,
                "20" => StatType.CriticalRate,
                "22" => StatType.CriticalDamage,
                "23" => StatType.EnergyRecharge,
                "28" => StatType.ElementalMastery,
                "26" => StatType.HealingBonus,
                "27" => StatType.IncomingHealingBonus,
                "30" => StatType.PhysicalDamageBonus,
                "40" => StatType.PyroDamageBonus,
                "41" => StatType.ElectroDamageBonus,
                "42" => StatType.HydroDamageBonus,
                "43" => StatType.DendroDamageBonus,
                "44" => StatType.AnemoDamageBonus,
                "45" => StatType.GeoDamageBonus,
                "46" => StatType.CryoDamageBonus,
                "1" => StatType.BaseHP,
                "4" => StatType.BaseAttack,
                "7" => StatType.BaseDefense,
                "2" => StatType.HPPercentage,
                "5" => StatType.AttackPercentage,
                "8" => StatType.DefensePercentage,
                _ => StatType.None
            };
        }

        private StatProperty? MapStatProperty(StatPropertyModel? model)
        {
            if (model == null) return null;

            string? propId = model.MainPropId ?? model.AppendPropId;
            if (string.IsNullOrEmpty(propId)) return null;

            StatType type = _assets.GetPropTypeFromString(propId);
            if (type == StatType.None) return null;

            return new StatProperty { Type = type, Value = model.StatValue };
        }

        private List<Talent> MapTalents(Dictionary<string, int>? skillLevelMap, Dictionary<string, int>? proudSkillExtraLevelMap)
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
    }
}