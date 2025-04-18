﻿using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public class ZZZDataMapper
    {
        private readonly IZZZAssets _assets;
        private readonly ZZZStatsCalculator _statsCalculator;

        public ZZZDataMapper(IZZZAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _statsCalculator = new ZZZStatsCalculator(assets);
        }

        public ZZZPlayerInfo MapPlayerInfo(ZZZApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.PlayerInfo == null) throw new ArgumentException("PlayerInfo is null", nameof(response));

            var profileDetail = response.PlayerInfo.SocialDetail?.ProfileDetail;
            if (profileDetail == null) throw new ArgumentException("ProfileDetail is null", nameof(response));

            var playerInfo = new ZZZPlayerInfo
            {
                Uid = response.Uid ?? profileDetail.Uid.ToString(),
                TTL = response.Ttl.ToString(),
                Nickname = profileDetail.Nickname ?? "Unknown",
                Level = profileDetail.Level,
                Signature = response.PlayerInfo.SocialDetail?.Desc ?? "",
                ProfilePictureId = profileDetail.ProfileId,
                ProfilePictureIcon = _assets.GetProfilePictureIconUrl(profileDetail.ProfileId),
                TitleId = profileDetail.Title,
                TitleText = _assets.GetTitleText(profileDetail.Title),
                NameCardId = profileDetail.CallingCardId,
                NameCardIcon = _assets.GetNameCardIconUrl(profileDetail.CallingCardId),
                MainCharacterId = profileDetail.AvatarId
            };

            if (response.PlayerInfo.SocialDetail?.MedalList != null)
            {
                foreach (var medalModel in response.PlayerInfo.SocialDetail.MedalList)
                {
                    playerInfo.Medals.Add(new ZZZMedal
                    {
                        Type = (MedalType)medalModel.MedalType,
                        Value = medalModel.Value,
                        Icon = _assets.GetMedalIconUrl(medalModel.MedalIcon),
                        Name = _assets.GetMedalName(medalModel.MedalIcon)
                    });
                }
            }

            if (response.PlayerInfo.ShowcaseDetail?.AvatarList != null)
            {
                foreach (var avatarModel in response.PlayerInfo.ShowcaseDetail.AvatarList)
                {
                    var agent = MapAgent(avatarModel);
                    playerInfo.ShowcaseAgents.Add(agent);
                }
            }

            return playerInfo;
        }

        public ZZZAgent MapAgent(ZZZAvatarModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var agent = new ZZZAgent
            {
                Id = model.Id,
                Name = _assets.GetAgentName(model.Id),
                Level = model.Level,
                PromotionLevel = model.PromotionLevel,
                TalentLevel = model.TalentLevel,
                CoreSkillEnhancement = model.CoreSkillEnhancement,
                SkinId = model.SkinId,
                WeaponEffectState = (WEngineEffectState)model.WeaponEffectState,
                IsHidden = model.IsHidden,
                ObtainmentTimestamp = DateTimeOffset.FromUnixTimeSeconds(model.ObtainmentTimestamp),
                ImageUrl = _assets.GetAgentIconUrl(model.Id),
                CircleIconUrl = _assets.GetAgentCircleIconUrl(model.Id),
                Rarity = (Rarity)_assets.GetAgentRarity(model.Id),
                ProfessionType = _assets.GetAgentProfessionType(model.Id),
                ElementTypes = FilterUnknownElements(_assets.GetAgentElements(model.Id))
            };

            agent.Stats = _statsCalculator.CalculateAgentBaseStats(
                model.Id,
                model.Level,
                model.PromotionLevel,
                model.CoreSkillEnhancement
            );

            agent.CoreSkillEnhancements.Clear();
            if (model.CoreSkillEnhancement > 0)
            {
                for (int i = 0; i < model.CoreSkillEnhancement; i++)
                {
                    agent.CoreSkillEnhancements.Add(i);
                }
            }


            agent.TalentToggles.Clear();
            if (model.TalentToggleList != null)
            {
                for (int i = 0; i < model.TalentToggleList.Count; i++)
                {
                    if (model.TalentToggleList[i])
                    {
                        agent.TalentToggles.Add(i);
                    }
                }
            }

            agent.ClaimedRewards.Clear();
            if (model.ClaimedRewardList != null)
            {
                agent.ClaimedRewards.AddRange(model.ClaimedRewardList);
            }

            if (model.Weapon != null)
            {
                agent.Weapon = MapWeapon(model.Weapon);
            }

            agent.SkillLevels.Clear();
            if (model.SkillLevelList != null)
            {
                foreach (var skillLevel in model.SkillLevelList)
                {
                    if (Enum.IsDefined(typeof(SkillType), skillLevel.Index))
                    {
                        agent.SkillLevels[(SkillType)skillLevel.Index] = skillLevel.Level;
                    }
                }
            }

            agent.EquippedDiscs.Clear();
            if (model.EquippedList != null)
            {
                foreach (var equippedItem in model.EquippedList)
                {
                    if (equippedItem.Equipment != null)
                    {
                        var driveDisc = MapDriveDisc(equippedItem.Equipment, equippedItem.Slot);
                        agent.EquippedDiscs.Add(driveDisc);
                    }
                }
            }


            return agent;
        }

        public ZZZWEngine MapWeapon(ZZZWeaponModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var weapon = new ZZZWEngine
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
                ImageUrl = _assets.GetWeaponIconUrl(model.Id)
            };

            var (mainStat, secondaryStat) = _statsCalculator.CalculateWeaponStats(model.Id, model.Level, model.BreakLevel);
            weapon.MainStat = mainStat;
            weapon.SecondaryStat = secondaryStat;

            return weapon;
        }

        public ZZZDriveDisc MapDriveDisc(ZZZEquipmentModel model, int slot)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            int suitId = _assets.GetDriveDiscSuitId(model.Id);
            int rarityValue = _assets.GetDriveDiscRarity(model.Id);
            Rarity rarity = (Rarity)rarityValue;

            var driveDisc = new ZZZDriveDisc
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
                IconUrl = _assets.GetDriveDiscSuitIconUrl(suitId)
            };

            if (model.MainPropertyList != null && model.MainPropertyList.Count > 0)
            {
                var mainProperty = model.MainPropertyList[0];
                driveDisc.MainStat = _statsCalculator.CalculateDriveDiscMainStat(
                    mainProperty.PropertyId,
                    mainProperty.PropertyValue,
                    model.Level,
                    mainProperty.PropertyLevel,
                    driveDisc.Rarity
                );
            }
            else
            {
                driveDisc.MainStat = new ZZZStat { Type = StatType.None };
            }


            driveDisc.SubStats.Clear();
            if (model.RandomPropertyList != null)
            {
                foreach (var property in model.RandomPropertyList)
                {
                    var subStat = _statsCalculator.CreateStatWithProperScaling(
                        property.PropertyId,
                        property.PropertyValue,
                        property.PropertyLevel
                    );
                    driveDisc.SubStats.Add(subStat);
                }
            }

            return driveDisc;
        }

        private List<ElementType> FilterUnknownElements(List<ElementType> elements)
        {
            if (elements == null || elements.Count == 0)
                return new List<ElementType> { ElementType.Unknown };

            var validElements = elements.Where(e => e != ElementType.Unknown).Distinct().ToList();

            return validElements.Count > 0 ? validElements : elements;
        }
    }
}