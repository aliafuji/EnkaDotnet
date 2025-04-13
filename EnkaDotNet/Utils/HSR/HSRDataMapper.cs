using EnkaDotNet.Models.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Utils.HSR;
using EnkaDotNet.Enums.HSR;
using System;
using System.Collections.Generic;

namespace EnkaDotNet.Utils.HSR
{
    public class HSRDataMapper
    {
        private readonly IHSRAssets _assets;
        private readonly HSRStatCalculator _statCalculator;

        public HSRDataMapper(IHSRAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _statCalculator = new HSRStatCalculator(_assets);
        }

        public HSRPlayerInfo MapPlayerInfo(HSRApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.DetailInfo == null) throw new ArgumentException("DetailInfo is null in the API response.", nameof(response));

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

        public HSRCharacter MapCharacter(HSRAvatarDetail avatarDetail)
        {
            if (avatarDetail == null) throw new ArgumentNullException(nameof(avatarDetail));
            if (avatarDetail.AvatarId <= 0) throw new ArgumentException("Invalid AvatarId provided.", nameof(avatarDetail));

            var character = new HSRCharacter
            {
                SkillTreeList = new List<HSRSkillTree>(),
                RelicList = new List<HSRRelic>(),
                Stats = new Dictionary<string, HSRStatValue>(),

                Id = avatarDetail.AvatarId,
                Name = _assets.GetCharacterName(avatarDetail.AvatarId),
                Level = avatarDetail.Level,
                Promotion = avatarDetail.Promotion,
                Rank = avatarDetail.Rank,
                Position = avatarDetail.Position,
                IsAssist = avatarDetail.IsAssist,

                Element = _assets.GetCharacterElement(avatarDetail.AvatarId),
                Path = _assets.GetCharacterPath(avatarDetail.AvatarId),
                Rarity = _assets.GetCharacterRarity(avatarDetail.AvatarId),
                IconUrl = _assets.GetCharacterIconUrl(avatarDetail.AvatarId),
                AvatarIconUrl = _assets.GetCharacterAvatarIconUrl(avatarDetail.AvatarId)
            };

            if (avatarDetail.Equipment != null)
            {
                character.Equipment = MapLightCone(avatarDetail.Equipment);
            }

            if (avatarDetail.SkillTreeList != null)
            {
                foreach (var skillTree in avatarDetail.SkillTreeList)
                {
                    character.SkillTreeList.Add(new HSRSkillTree
                    {
                        PointId = skillTree.PointId,
                        Level = skillTree.Level,
                        Icon = _assets.GetSkillTreeIconUrl(skillTree.PointId),
                    });
                }
            }

            var characterInfo = _assets.GetCharacterInfo(character.Id.ToString());
            if (characterInfo?.RankIDList != null)
            {
                for (int i = 0; i < characterInfo.RankIDList.Count; i++)
                {
                    int eidolonId = characterInfo.RankIDList[i];
                    character.Eidolons.Add(new Eidolon
                    {
                        Id = eidolonId,
                        Icon = _assets.GetEidolonIconUrl(eidolonId),
                        Unlocked = i + 1 <= character.Rank
                    });
                }
            }

            if (avatarDetail.RelicList != null)
            {
                foreach (var relicModel in avatarDetail.RelicList)
                {
                    if (relicModel.Id > 0)
                    {
                        character.RelicList.Add(MapRelicModelToRelic(relicModel));
                    }
                }
            }

            character.Stats = _statCalculator.CalculateCharacterStats(character);

            return character;
        }

        public HSRLightCone MapLightCone(HSREquipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));
            if (equipment.Id <= 0) throw new ArgumentException("Invalid Equipment ID provided.", nameof(equipment));

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
                IconUrl = _assets.GetLightConeIconUrl(equipment.Id)
            };

            if (equipment.Flat?.Props != null)
            {
                foreach (var prop in equipment.Flat.Props)
                {
                    if (string.IsNullOrEmpty(prop.Type)) continue;

                    bool isPercentage = HSRStatPropertyUtils.IsPercentageType(prop.Type);

                    if (prop.Type == "BaseHP")
                        lightCone.BaseHP = prop.Value;
                    else if (prop.Type == "BaseAttack")
                        lightCone.BaseAttack = prop.Value;
                    else if (prop.Type == "BaseDefence")
                        lightCone.BaseDefense = prop.Value;

                    lightCone.Properties.Add(new HSRStatProperty
                    {
                        Type = prop.Type,
                        PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(prop.Type),
                        Value = prop.Value,
                        IsPercentage = isPercentage
                    });
                }
            }

            return lightCone;
        }

        private string GetLocalizedRelicSetName(string? setNameHash, int setId)
        {
            if (!string.IsNullOrEmpty(setNameHash) && setNameHash != setId.ToString())
            {
                string localizedName = _assets.GetLocalizedText(setNameHash);
                if (localizedName != setNameHash)
                {
                    return localizedName;
                }
            }
            return _assets.GetRelicSetName(setId);
        }

        public HSRRelic MapRelicModelToRelic(HSRRelicModel relicModel)
        {
            if (relicModel == null) throw new ArgumentNullException(nameof(relicModel));
            if (relicModel.Id <= 0) throw new ArgumentException("Invalid Relic ID provided.", nameof(relicModel));

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
                IconUrl = _assets.GetRelicIconUrl(relicModel.Id)
            };

            if (relicModel.Flat?.Props != null && relicModel.Flat.Props.Count > 0)
            {
                var mainProp = relicModel.Flat.Props[0];
                if (!string.IsNullOrEmpty(mainProp.Type))
                {
                    bool isPercentage = HSRStatPropertyUtils.IsPercentageType(mainProp.Type);
                    double scaledValue = mainProp.Value * HSRStatPropertyUtils.GetPropertyScalingFactor(mainProp.Type);


                    relic.MainStat = new HSRStatProperty
                    {
                        Type = mainProp.Type,
                        PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(mainProp.Type),
                        Value = scaledValue,
                        BaseValue = mainProp.Value,
                        IsPercentage = isPercentage
                    };
                }
                else
                {
                    relic.MainStat = new HSRStatProperty { Type = "None", PropertyType = StatPropertyType.None };
                }
            }
            else
            {
                relic.MainStat = new HSRStatProperty { Type = "None", PropertyType = StatPropertyType.None };
            }

            if (relicModel.Flat?.Props != null && relicModel.Flat.Props.Count > 1)
            {
                for (int i = 1; i < relicModel.Flat.Props.Count; i++)
                {
                    var subProp = relicModel.Flat.Props[i];
                    if (!string.IsNullOrEmpty(subProp.Type))
                    {
                        bool isPercentage = HSRStatPropertyUtils.IsPercentageType(subProp.Type);
                        double scaledValue = subProp.Value * HSRStatPropertyUtils.GetPropertyScalingFactor(subProp.Type);

                        relic.SubStats.Add(new HSRStatProperty
                        {
                            Type = subProp.Type,
                            PropertyType = HSRStatPropertyUtils.MapToStatPropertyType(subProp.Type),
                            Value = scaledValue,
                            BaseValue = subProp.Value,
                            IsPercentage = isPercentage
                        });
                    }
                }
            }

            return relic;
        }
    }
}