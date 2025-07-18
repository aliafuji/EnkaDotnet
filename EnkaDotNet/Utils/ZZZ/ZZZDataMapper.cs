using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public class ZZZDataMapper
    {
        private readonly IZZZAssets _assets;
        private readonly ZZZStatsCalculator _statsCalculator;
        private readonly EnkaClientOptions _options;

        public ZZZDataMapper(IZZZAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _statsCalculator = new ZZZStatsCalculator(assets);
        }

        public ZZZPlayerInfo MapPlayerInfo(ZZZApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.PlayerInfo == null) throw new ArgumentException("PlayerInfo is null", nameof(response));

            var profileDetail = response.PlayerInfo.SocialDetail?.ProfileDetail;
            var socialDetail = response.PlayerInfo.SocialDetail;
            if (profileDetail == null) throw new ArgumentException("ProfileDetail is null in both PlayerInfo and SocialDetail", nameof(response));

            string nickname = profileDetail.Nickname ?? "Unknown";
            int level = profileDetail.Level;
            int profileId = profileDetail.ProfileId;
            int callingCardId = profileDetail.CallingCardId;
            int avatarId = profileDetail.AvatarId;
            long uidFromProfile = profileDetail.Uid;

            int titleId = 0;
            if (socialDetail?.TitleInfo != null) titleId = socialDetail.TitleInfo.Title;
            else if (profileDetail.TitleInfo != null) titleId = profileDetail.TitleInfo.Title;
            else if (profileDetail.Title != 0) titleId = profileDetail.Title;
            else if (socialDetail != null && socialDetail.Title != 0) titleId = socialDetail.Title;

            var medals = new List<ZZZMedal>();
            if (response.PlayerInfo.SocialDetail?.MedalList != null)
            {
                foreach (var medalModel in response.PlayerInfo.SocialDetail.MedalList)
                {
                    medals.Add(new ZZZMedal
                    {
                        Type = (MedalType)medalModel.MedalType,
                        Value = medalModel.Value,
                        Icon = _assets.GetMedalIconUrl(medalModel.MedalIcon),
                        Name = _assets.GetMedalName(medalModel.MedalIcon)
                    });
                }
            }

            var showcaseAgents = new List<ZZZAgent>();
            if (response.PlayerInfo.ShowcaseDetail?.AvatarList != null)
            {
                foreach (var avatarModel in response.PlayerInfo.ShowcaseDetail.AvatarList)
                {
                    var agent = MapAgent(avatarModel);
                    showcaseAgents.Add(agent);
                }
            }

            return new ZZZPlayerInfo
            {
                Uid = response.Uid ?? uidFromProfile.ToString(),
                TTL = response.Ttl.ToString(),
                Nickname = nickname,
                Level = level,
                Signature = socialDetail?.Desc ?? response.PlayerInfo.Desc ?? "",
                ProfilePictureId = profileId,
                ProfilePictureIcon = _assets.GetProfilePictureIconUrl(profileId),
                TitleId = titleId,
                TitleText = _assets.GetTitleText(titleId),
                NameCardId = callingCardId,
                NameCardIcon = _assets.GetNameCardIconUrl(callingCardId),
                MainCharacterId = avatarId,
                Medals = medals,
                ShowcaseAgents = showcaseAgents
            };
        }

        public ZZZAgent MapAgent(ZZZAvatarModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var coreSkillEnhancements = new List<int>();
            if (model.CoreSkillEnhancement > 0)
            {
                for (int i = 0; i < model.CoreSkillEnhancement; i++)
                {
                    coreSkillEnhancements.Add(i);
                }
            }

            var talentToggles = new List<int>();
            if (model.TalentToggleList != null)
            {
                for (int i = 0; i < model.TalentToggleList.Count; i++)
                {
                    if (model.TalentToggleList[i])
                    {
                        talentToggles.Add(i);
                    }
                }
            }

            var claimedRewards = new List<int>();
            if (model.ClaimedRewardList != null)
            {
                foreach (var reward in model.ClaimedRewardList)
                {
                    claimedRewards.Add((int)reward);
                }
            }

            ZZZWEngine weapon = null;
            if (model.Weapon != null)
            {
                weapon = MapWeapon(model.Weapon);
                weapon.Options = this._options;
            }

            var skillLevels = new ConcurrentDictionary<SkillType, int>();
            if (model.SkillLevelList != null)
            {
                foreach (var skillLevel in model.SkillLevelList)
                {
                    if (Enum.IsDefined(typeof(SkillType), skillLevel.Index))
                    {
                        int baseLevel = skillLevel.Level;
                        
                        int finalLevel = 0;

                        if ((SkillType)skillLevel.Index != SkillType.CoreSkill)
                        {
                            finalLevel = baseLevel;
                        } 
                        else
                        {
                            int mindscapeBonus = CalculateMindscapeSkillBonus(model.TalentLevel);
                            finalLevel = baseLevel + mindscapeBonus;
                        }

                            skillLevels.TryAdd((SkillType)skillLevel.Index, finalLevel);
                    }
                }
            }

            var equippedDiscs = new List<ZZZDriveDisc>();
            if (model.EquippedList != null)
            {
                foreach (var equippedItem in model.EquippedList)
                {
                    if (equippedItem.Equipment != null)
                    {
                        var disc = MapDriveDisc(equippedItem.Equipment, equippedItem.Slot);
                        if (disc != null)
                        {
                            disc.Options = this._options;
                            equippedDiscs.Add(disc);
                        }
                    }
                }
            }

            var agent = new ZZZAgent
            {
                Id = model.Id,
                Name = _assets.GetAgentName(model.Id),
                Level = model.Level,
                PromotionLevel = model.PromotionLevel,
                TalentLevel = model.TalentLevel,
                CoreSkillEnhancement = model.CoreSkillEnhancement,
                Skins = (model.SkinId != 0)
                    ? new ConcurrentDictionary<string, Skin>(new[] { new KeyValuePair<string, Skin>(model.SkinId.ToString(), _assets.GetAgentSkin(model.Id.ToString(), model.SkinId.ToString())) })
                    : new ConcurrentDictionary<string, Skin>(),
                WeaponEffectState = (WEngineEffectState)model.WeaponEffectState,
                IsHidden = model.IsHidden,
                ObtainmentTimestamp = DateTimeOffset.FromUnixTimeSeconds(model.ObtainmentTimestamp),
                ImageUrl = _assets.GetAgentIconUrl(model.Id),
                CircleIconUrl = _assets.GetAgentCircleIconUrl(model.Id),
                Rarity = (Rarity)_assets.GetAgentRarity(model.Id),
                ProfessionType = _assets.GetAgentProfessionType(model.Id),
                ElementTypes = new List<ElementType>(FilterUnknownElements(_assets.GetAgentElements(model.Id))),
                Colors = new List<Assets.ZZZ.Models.ZZZAvatarColors>(_assets.GetAvatarColors(model.Id)),
                Options = this._options,
                Assets = this._assets,
                Stats = new ConcurrentDictionary<StatType, double>(_statsCalculator.CalculateAgentBaseStats(model.Id, model.Level, model.PromotionLevel, model.CoreSkillEnhancement)),
                CoreSkillEnhancements = coreSkillEnhancements,
                TalentToggles = talentToggles,
                ClaimedRewards = claimedRewards,
                Weapon = weapon,
                SkillLevels = skillLevels,
                EquippedDiscs = equippedDiscs
            };

            return agent;
        }

        private int CalculateMindscapeSkillBonus(int mindscape)
        {
            int bonus = 0;

            if (mindscape >= 3)
            {
                bonus += 2;
            }

            if (mindscape >= 5)
            {
                bonus += 2;
            }

            return bonus;
        }

        public ZZZWEngine MapWeapon(ZZZWeaponModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var (mainStat, secondaryStat) = _statsCalculator.CalculateWeaponStats(model.Id, model.Level, model.BreakLevel);

            return new ZZZWEngine
            {
                Uid = model.Uid.ToString(),
                Id = model.Id,
                Level = model.Level,
                BreakLevel = model.BreakLevel,
                UpgradeLevel = model.UpgradeLevel,
                IsAvailable = model.IsAvailable,
                IsLocked = model.IsLocked,
                Name = _assets.GetWeaponName(model.Id),
                Rarity = (Rarity)_assets.GetWeaponRarity(model.Id),
                ProfessionType = _assets.GetWeaponType(model.Id),
                ImageUrl = _assets.GetWeaponIconUrl(model.Id),
                Options = this._options,
                MainStat = mainStat,
                SecondaryStat = secondaryStat
            };
        }

        public ZZZDriveDisc MapDriveDisc(ZZZEquipmentModel model, int slot)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            int suitId = _assets.GetDriveDiscSuitId(model.Id);
            int rarityValue = _assets.GetDriveDiscRarity(model.Id);
            var rarity = (Rarity)rarityValue;

            ZZZStat mainStat;
            if (model.MainPropertyList != null && model.MainPropertyList.Count > 0)
            {
                var mainProperty = model.MainPropertyList[0];
                mainStat = _statsCalculator.CalculateDriveDiscMainStat(
                    mainProperty.PropertyId, mainProperty.PropertyValue, model.Level, mainProperty.PropertyLevel, rarity
                );
            }
            else
            {
                mainStat = new ZZZStat { Type = StatType.None };
            }

            var subStats = new List<ZZZStat>();
            if (model.RandomPropertyList != null)
            {
                foreach (var property in model.RandomPropertyList)
                {
                    var subStat = _statsCalculator.CreateStatWithProperScaling(
                        property.PropertyId, property.PropertyValue, property.PropertyLevel
                    );
                    subStats.Add(subStat);
                }
            }

            return new ZZZDriveDisc
            {
                Uid = model.Uid.ToString(),
                Id = model.Id,
                Level = model.Level,
                BreakLevel = model.BreakLevel,
                IsLocked = model.IsLocked,
                IsAvailable = model.IsAvailable,
                IsTrash = model.IsTrash,
                Slot = (DriveDiscSlot)slot,
                Rarity = rarity,
                SuitId = suitId,
                SuitName = _assets.GetDriveDiscSuitName(suitId),
                IconUrl = _assets.GetDriveDiscSuitIconUrl(suitId),
                Options = this._options,
                MainStat = mainStat,
                SubStatsRaw = subStats
            };
        }

        private List<ElementType> FilterUnknownElements(IEnumerable<ElementType> elements)
        {
            if (elements == null)
            {
                return new List<ElementType> { ElementType.Unknown };
            }

            var uniqueValidElements = new HashSet<ElementType>();
            foreach (var element in elements)
            {
                if (element != ElementType.Unknown)
                {
                    uniqueValidElements.Add(element);
                }
            }

            if (uniqueValidElements.Count > 0)
            {
                var result = new List<ElementType>();
                foreach (var element in uniqueValidElements)
                {
                    result.Add(element);
                }
                return result;
            }

            return new List<ElementType> { ElementType.Unknown };
        }
    }
}
