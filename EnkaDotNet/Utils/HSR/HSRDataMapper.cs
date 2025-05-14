using System;
using System.Collections.Generic;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Components.HSR.EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Utils.HSR
{
    /// <summary>
    /// Maps raw API data to Honkai: Star Rail specific component models
    /// </summary>
    public class HSRDataMapper
    {
        private readonly IHSRAssets _assets;
        private readonly HSRStatCalculator _statCalculator;
        private readonly EnkaClientOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HSRDataMapper"/> class
        /// </summary>
        /// <param name="assets">The HSR assets provider</param>
        /// <param name="options">The client options</param>
        public HSRDataMapper(IHSRAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _statCalculator = new HSRStatCalculator(_assets, _options);
        }

        /// <summary>
        /// Maps the raw API response to HSR player information
        /// </summary>
        /// <param name="response">The raw API response</param>
        /// <returns>The mapped <see cref="HSRPlayerInfo"/> component model</returns>
        public HSRPlayerInfo MapPlayerInfo(HSRApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.DetailInfo == null) throw new ArgumentException("DetailInfo is null in the API response", nameof(response));

            var playerInfo = new HSRPlayerInfo
            {
                DisplayedCharacters = new List<HSRCharacter>(),
                Uid = response.Uid ?? response.DetailInfo.Uid.ToString(),
                TTL = response.Ttl.ToString(),
                Nickname = response.DetailInfo.Nickname ?? "Unknown",
                Level = response.DetailInfo.Level,
                Signature = response.DetailInfo.Signature ?? "",
                WorldLevel = response.DetailInfo.WorldLevel,
                FriendCount = response.DetailInfo.FriendCount,
                Platform = response.DetailInfo.Platform ?? "",
                IsDisplayAvatar = response.DetailInfo.IsDisplayAvatar,
                ProfilePictureId = response.DetailInfo.HeadIcon,
                ProfilePictureIcon = _assets.GetProfilePictureIconUrl(response.DetailInfo.HeadIcon)
            };

            if (response.DetailInfo.RecordInfo != null)
            {
                playerInfo.RecordInfo = new HSRRecordInfo
                {
                    AchievementCount = response.DetailInfo.RecordInfo.AchievementCount,
                    AvatarCount = response.DetailInfo.RecordInfo.AvatarCount,
                    LightConeCount = response.DetailInfo.RecordInfo.EquipmentCount,
                    RelicCount = response.DetailInfo.RecordInfo.RelicCount,
                    MemoryOfChaosScore = response.DetailInfo.RecordInfo.MaxRogueChallengeScore
                };
            }
            else
            {
                playerInfo.RecordInfo = new HSRRecordInfo();
            }

            if (response.DetailInfo.AvatarDetailList != null)
            {
                foreach (var avatarDetail in response.DetailInfo.AvatarDetailList)
                {
                    if (avatarDetail.AvatarId > 0)
                    {
                        var character = MapCharacter(avatarDetail);
                        playerInfo.DisplayedCharacters.Add(character);
                    }
                }
            }

            return playerInfo;
        }

        /// <summary>
        /// Maps a raw avatar detail model to an HSR character component model
        /// </summary>
        /// <param name="avatarDetail">The raw avatar detail model</param>
        /// <returns>The mapped <see cref="HSRCharacter"/> component model</returns>
        public HSRCharacter MapCharacter(HSRAvatarDetail avatarDetail)
        {
            if (avatarDetail == null) throw new ArgumentNullException(nameof(avatarDetail));
            if (avatarDetail.AvatarId <= 0) throw new ArgumentException("Invalid AvatarId provided", nameof(avatarDetail));

            var character = new HSRCharacter
            {
                SkillTreeList = new List<HSRSkillTree>(),
                RelicList = new List<HSRRelic>(),
                Stats = new Dictionary<string, HSRStatValue>(),
                Eidolons = new List<Eidolon>(),

                Id = avatarDetail.AvatarId,
                Level = avatarDetail.Level,
                Promotion = avatarDetail.Promotion,
                Rank = avatarDetail.Rank,
                Position = avatarDetail.Position,
                IsAssist = avatarDetail.IsAssist,

                Name = _assets.GetCharacterName(avatarDetail.AvatarId),
                Element = _assets.GetCharacterElement(avatarDetail.AvatarId),
                Path = _assets.GetCharacterPath(avatarDetail.AvatarId),
                Rarity = _assets.GetCharacterRarity(avatarDetail.AvatarId),
                IconUrl = _assets.GetCharacterIconUrl(avatarDetail.AvatarId),
                AvatarIconUrl = _assets.GetCharacterAvatarIconUrl(avatarDetail.AvatarId),
                Options = this._options
            };

            character.SetAssets(_assets);

            var characterInfo = _assets.GetCharacterInfo(character.Id.ToString());
            var unlockedEidolonIds = new List<int>();
            if (characterInfo?.RankIDList != null)
            {
                for (int i = 0; i < characterInfo.RankIDList.Count; i++)
                {
                    int eidolonId = characterInfo.RankIDList[i];
                    bool unlocked = i + 1 <= character.Rank;
                    character.Eidolons.Add(new Eidolon
                    {
                        Id = eidolonId,
                        Icon = _assets.GetEidolonIconUrl(eidolonId),
                        Unlocked = unlocked
                    });
                    if (unlocked)
                    {
                        unlockedEidolonIds.Add(eidolonId);
                    }
                }
            }

            if (avatarDetail.Equipment != null)
            {
                character.Equipment = MapLightCone(avatarDetail.Equipment);
                if (character.Equipment != null) character.Equipment.Options = this._options;
            }

            if (avatarDetail.SkillTreeList != null)
            {
                foreach (var skillTreeModel in avatarDetail.SkillTreeList)
                {
                    var skillTree = new HSRSkillTree
                    {
                        PointId = skillTreeModel.PointId,
                        Level = skillTreeModel.Level,
                        BaseLevel = skillTreeModel.Level,
                        Options = this._options
                    };

                    var pointInfo = _assets.GetSkillTreePointInfo(skillTree.PointId.ToString());
                    if (pointInfo != null)
                    {
                        skillTree.Anchor = pointInfo.Anchor ?? string.Empty;
                        skillTree.TraceType = (TraceType)(pointInfo.PointType);
                        skillTree.MaxLevel = pointInfo.MaxLevel;
                        skillTree.Icon = _assets.GetSkillTreeIconUrl(skillTree.PointId);
                        skillTree.SkillIds = pointInfo.SkillIds ?? new List<int>();
                        skillTree.Name = _assets.GetSkillTreePointName(skillTree.PointId.ToString());
                        skillTree.Description = _assets.GetSkillTreePointDescription(skillTree.PointId.ToString());

                        bool boostApplied = false;
                        foreach (int eidolonId in unlockedEidolonIds)
                        {
                            var eidolonInfo = _assets.GetEidolonInfo(eidolonId.ToString());
                            if (eidolonInfo?.SkillAddLevelList != null)
                            {
                                foreach (int skillId in skillTree.SkillIds)
                                {
                                    string skillIdStr = skillId.ToString();
                                    if (eidolonInfo.SkillAddLevelList.TryGetValue(skillIdStr, out int levelBoost))
                                    {
                                        skillTree.Level += levelBoost;
                                        skillTree.IsBoosted = true;
                                        boostApplied = true;
                                        break;
                                    }
                                }
                            }
                            if (boostApplied) break;
                        }
                    }
                    else
                    {
                        skillTree.Name = $"Trace_{skillTree.PointId}";
                        skillTree.Icon = _assets.GetSkillTreeIconUrl(skillTree.PointId);
                    }

                    character.SkillTreeList.Add(skillTree);
                }
            }

            if (avatarDetail.RelicList != null)
            {
                foreach (var relicModel in avatarDetail.RelicList)
                {
                    if (relicModel.Id > 0)
                    {
                        var relic = MapRelicModelToRelic(relicModel);
                        relic.Options = this._options;
                        if (relic.MainStat != null) relic.MainStat.Options = this._options;
                        foreach (var subStat in relic.SubStats) subStat.Options = this._options;
                        character.RelicList.Add(relic);
                    }
                }
            }

            character.Stats = _statCalculator.CalculateCharacterStats(character);

            foreach (var statVal in character.Stats.Values)
            {
                if (statVal != null) statVal.Options = this._options;
            }
            return character;
        }

        /// <summary>
        /// Maps a raw equipment model to an HSR light cone component model
        /// </summary>
        /// <param name="equipment">The raw equipment model</param>
        /// <returns>The mapped <see cref="HSRLightCone"/> component model</returns>
        public HSRLightCone MapLightCone(HSREquipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));
            if (equipment.Id <= 0) throw new ArgumentException("Invalid Equipment ID provided", nameof(equipment));

            var lightCone = new HSRLightCone
            {
                Properties = new List<HSRStatProperty>(),
                Id = equipment.Id,
                Name = _assets.GetLightConeName(equipment.Id),
                Level = equipment.Level,
                Promotion = equipment.Promotion,
                Rank = equipment.Rank,
                Path = _assets.GetLightConePath(equipment.Id),
                Rarity = _assets.GetLightConeRarity(equipment.Id),
                IconUrl = _assets.GetLightConeIconUrl(equipment.Id),
                Options = this._options
            };

            if (equipment.Flat?.Props != null)
            {
                foreach (var prop in equipment.Flat.Props)
                {
                    if (string.IsNullOrEmpty(prop.Type)) continue;
                    bool isPercentage = HSRStatPropertyUtils.IsPercentageType(prop.Type);
                    if (prop.Type == "BaseHP") lightCone.BaseHP = Math.Floor(prop.Value);
                    else if (prop.Type == "BaseAttack") lightCone.BaseAttack = Math.Floor(prop.Value);
                    else if (prop.Type == "BaseDefence") lightCone.BaseDefense = Math.Floor(prop.Value);
                    lightCone.Properties.Add(new HSRStatProperty
                    {
                        Type = prop.Type,
                        PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(prop.Type),
                        Value = prop.Value,
                        IsPercentage = isPercentage,
                        Options = this._options
                    });
                }
            }
            return lightCone;
        }

        private string GetLocalizedRelicSetName(string setNameHash, int setId)
        {
            if (!string.IsNullOrEmpty(setNameHash) && setNameHash != setId.ToString())
            {
                string localizedName = _assets.GetLocalizedText(setNameHash);
                if (localizedName != setNameHash)
                {
                    return localizedName;
                }
            }
            string nameFromAssets = _assets.GetRelicSetName(setId);
            return !string.IsNullOrEmpty(nameFromAssets) ? nameFromAssets : $"Set_{setId}";
        }

        /// <summary>
        /// Maps a raw relic model to an HSR relic component model
        /// </summary>
        /// <param name="relicModel">The raw relic model</param>
        /// <returns>The mapped <see cref="HSRRelic"/> component model</returns>
        public HSRRelic MapRelicModelToRelic(HSRRelicModel relicModel)
        {
            if (relicModel == null) throw new ArgumentNullException(nameof(relicModel));
            if (relicModel.Id <= 0) throw new ArgumentException("Invalid Relic ID provided", nameof(relicModel));

            var relic = new HSRRelic
            {
                SubStats = new List<HSRStatProperty>(),
                Id = relicModel.Id,
                Level = relicModel.Level,
                Type = relicModel.Type,
                RelicType = _assets.GetRelicType(relicModel.Id),
                SetId = relicModel.Flat?.SetId ?? 0,
                SetName = GetLocalizedRelicSetName(relicModel.Flat?.SetName, relicModel.Flat?.SetId ?? 0),
                Rarity = _assets.GetRelicRarity(relicModel.Id),
                IconUrl = _assets.GetRelicIconUrl(relicModel.Id),
                Options = this._options
            };

            if (relicModel.Flat?.Props != null && relicModel.Flat.Props.Count > 0)
            {
                var mainProp = relicModel.Flat.Props[0];
                if (!string.IsNullOrEmpty(mainProp.Type))
                {
                    bool isPercentage = HSRStatPropertyUtils.IsPercentageType(mainProp.Type);
                    relic.MainStat = new HSRStatProperty
                    {
                        Type = mainProp.Type,
                        PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(mainProp.Type),
                        Value = mainProp.Value,
                        BaseValue = mainProp.Value,
                        IsPercentage = isPercentage,
                        Options = this._options
                    };
                }
                else
                {
                    relic.MainStat = new HSRStatProperty { Type = "None", PropertyType = StatPropertyType.None, Options = this._options };
                }
            }
            else
            {
                relic.MainStat = new HSRStatProperty { Type = "None", PropertyType = StatPropertyType.None, Options = this._options };
            }

            if (relicModel.Flat?.Props != null && relicModel.Flat.Props.Count > 1)
            {
                for (int i = 1; i < relicModel.Flat.Props.Count; i++)
                {
                    var subProp = relicModel.Flat.Props[i];
                    if (!string.IsNullOrEmpty(subProp.Type))
                    {
                        bool isPercentage = HSRStatPropertyUtils.IsPercentageType(subProp.Type);
                        relic.SubStats.Add(new HSRStatProperty
                        {
                            Type = subProp.Type,
                            PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(subProp.Type),
                            Value = subProp.Value,
                            BaseValue = subProp.Value,
                            IsPercentage = isPercentage,
                            Options = this._options
                        });
                    }
                }
            }
            return relic;
        }
    }
}
