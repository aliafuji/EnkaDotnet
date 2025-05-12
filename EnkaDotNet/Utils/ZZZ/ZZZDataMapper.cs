using System;
using System.Collections.Generic;
using System.Linq;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    /// <summary>
    /// Maps raw API data to Zenless Zone Zero specific component models
    /// </summary>
    public class ZZZDataMapper
    {
        private readonly IZZZAssets _assets;
        private readonly ZZZStatsCalculator _statsCalculator;
        private readonly EnkaClientOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZZZDataMapper"/> class
        /// </summary>
        /// <param name="assets">The ZZZ assets provider</param>
        /// <param name="options">The client options</param>
        public ZZZDataMapper(IZZZAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _statsCalculator = new ZZZStatsCalculator(assets);
        }

        /// <summary>
        /// Maps the raw API response to ZZZ player information
        /// </summary>
        /// <param name="response">The raw API response</param>
        /// <returns>The mapped <see cref="ZZZPlayerInfo"/> component model</returns>
        public ZZZPlayerInfo MapPlayerInfo(ZZZApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.PlayerInfo == null) throw new ArgumentException("PlayerInfo is null", nameof(response));

            var profileDetail = response.PlayerInfo.SocialDetail?.ProfileDetail;
            var socialDetail = response.PlayerInfo.SocialDetail;
            if (profileDetail == null && socialDetail?.ProfileDetail == null) throw new ArgumentException("ProfileDetail and SocialDetailProfileDetail are null", nameof(response));

            profileDetail = profileDetail ?? socialDetail.ProfileDetail;

            string nickname = profileDetail?.Nickname ?? "Unknown";
            int level = profileDetail?.Level ?? 0;
            int profileId = profileDetail?.ProfileId ?? 0;
            int callingCardId = profileDetail?.CallingCardId ?? 0;
            int avatarId = profileDetail?.AvatarId ?? 0;
            long uidFromProfile = profileDetail?.Uid ?? 0;

            int titleId = 0;
            if (socialDetail?.TitleInfo != null) titleId = socialDetail.TitleInfo.Title;
            else if (profileDetail?.TitleInfo != null) titleId = profileDetail.TitleInfo.Title;
            else if (profileDetail != null && profileDetail.Title != 0) titleId = profileDetail.Title;
            else if (socialDetail != null && socialDetail.Title != 0) titleId = socialDetail.Title;


            var playerInfo = new ZZZPlayerInfo
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
                MainCharacterId = avatarId
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

        /// <summary>
        /// Maps a raw avatar model to a ZZZ agent component model
        /// </summary>
        /// <param name="model">The raw avatar model</param>
        /// <returns>The mapped <see cref="ZZZAgent"/> component model</returns>
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
                ElementTypes = FilterUnknownElements(_assets.GetAgentElements(model.Id)),
                Colors = _assets.GetAvatarColors(model.Id),
                Options = this._options,
                Assets = this._assets
            };

            agent.Stats = _statsCalculator.CalculateAgentBaseStats(
                model.Id, model.Level, model.PromotionLevel, model.CoreSkillEnhancement
            );

            agent.CoreSkillEnhancements.Clear();
            if (model.CoreSkillEnhancement > 0) for (int i = 0; i < model.CoreSkillEnhancement; i++) agent.CoreSkillEnhancements.Add(i);

            agent.TalentToggles.Clear();
            if (model.TalentToggleList != null) for (int i = 0; i < model.TalentToggleList.Count; i++) if (model.TalentToggleList[i]) agent.TalentToggles.Add(i);

            agent.ClaimedRewards.Clear();
            if (model.ClaimedRewardList != null) agent.ClaimedRewards.AddRange(model.ClaimedRewardList);

            if (model.Weapon != null)
            {
                agent.Weapon = MapWeapon(model.Weapon);
                if (agent.Weapon != null) agent.Weapon.Options = this._options;
            }


            agent.SkillLevels.Clear();
            if (model.SkillLevelList != null) foreach (var skillLevel in model.SkillLevelList) if (Enum.IsDefined(typeof(SkillType), skillLevel.Index)) agent.SkillLevels[(SkillType)skillLevel.Index] = skillLevel.Level;

            agent.EquippedDiscs.Clear();
            if (model.EquippedList != null)
            {
                foreach (var equippedItem in model.EquippedList)
                {
                    if (equippedItem.Equipment != null)
                    {
                        var disc = MapDriveDisc(equippedItem.Equipment, equippedItem.Slot);
                        if (disc != null) disc.Options = this._options;
                        agent.EquippedDiscs.Add(disc);
                    }
                }
            }

            return agent;
        }

        /// <summary>
        /// Maps a raw weapon model to a ZZZ W-Engine component model
        /// </summary>
        /// <param name="model">The raw weapon model</param>
        /// <returns>The mapped <see cref="ZZZWEngine"/> component model</returns>
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
                ImageUrl = _assets.GetWeaponIconUrl(model.Id),
                Options = this._options
            };

            var (mainStat, secondaryStat) = _statsCalculator.CalculateWeaponStats(model.Id, model.Level, model.BreakLevel);
            weapon.MainStat = mainStat;
            weapon.SecondaryStat = secondaryStat;

            return weapon;
        }

        /// <summary>
        /// Maps a raw equipment model to a ZZZ Drive Disc component model
        /// </summary>
        /// <param name="model">The raw equipment model</param>
        /// <param name="slot">The slot the Drive Disc is equipped in</param>
        /// <returns>The mapped <see cref="ZZZDriveDisc"/> component model</returns>
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
                IconUrl = _assets.GetDriveDiscSuitIconUrl(suitId),
                Options = this._options
            };

            if (model.MainPropertyList != null && model.MainPropertyList.Count > 0)
            {
                var mainProperty = model.MainPropertyList[0];
                driveDisc.MainStat = _statsCalculator.CalculateDriveDiscMainStat(
                    mainProperty.PropertyId, mainProperty.PropertyValue, model.Level, mainProperty.PropertyLevel, driveDisc.Rarity
                );
            }
            else driveDisc.MainStat = new ZZZStat { Type = StatType.None };

            driveDisc.SubStatsRaw.Clear();
            if (model.RandomPropertyList != null)
            {
                foreach (var property in model.RandomPropertyList)
                {
                    var subStat = _statsCalculator.CreateStatWithProperScaling(
                        property.PropertyId, property.PropertyValue, property.PropertyLevel
                    );
                    driveDisc.SubStatsRaw.Add(subStat);
                }
            }
            return driveDisc;
        }

        private List<ElementType> FilterUnknownElements(List<ElementType> elements)
        {
            if (elements == null || elements.Count == 0) return new List<ElementType> { ElementType.Unknown };
            var validElements = elements.Where(e => e != ElementType.Unknown).Distinct().ToList();
            return validElements.Count > 0 ? validElements : new List<ElementType> { ElementType.Unknown };
        }
    }
}
