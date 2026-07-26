using System.Collections.Generic;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Assets.ZZZ
{
    public interface IZZZAssets : IAssets
    {
        string GetLocalizedText(string key);
        ZZZAvatarAssetInfo GetAvatarInfo(string agentId);
        ZZZWeaponAssetInfo GetWeaponInfo(string weaponId);
        IReadOnlyList<ZZZAvatarColors> GetAvatarColors(int agentId);
        string GetAgentName(int agentId);
        string GetAgentIconUrl(int agentId);
        string GetAgentCircleIconUrl(int agentId);
        IReadOnlyList<ElementType> GetAgentElements(int agentId);
        ProfessionType GetAgentProfessionType(int agentId);
        int GetAgentRarity(int agentId);
        string GetWeaponName(int weaponId);
        string GetWeaponIconUrl(int weaponId);
        ProfessionType GetWeaponType(int weaponId);
        int GetWeaponRarity(int weaponId);
        string GetDriveDiscSuitName(int suitId);
        string GetDriveDiscSuitIconUrl(int suitId);
        int GetDriveDiscRarity(int discId);
        int GetDriveDiscSuitId(int discId);
        string GetPropertyName(int propertyId);
        string FormatPropertyValue(int propertyId, double value);
        string GetTitleText(int titleId);
        string GetTitleColorA(int titleId);
        string GetTitleColorB(int titleId);
        string GetMedalName(int medalId);
        string GetMedalIconUrl(int medalId);
        string GetNameCardIconUrl(int nameCardId);
        string GetProfilePictureIconUrl(int profilePictureId);
        /// <summary>
        /// Always returns <see cref="string.Empty"/>. The upstream Zenless Zone Zero asset feed
        /// (<c>avatars.json</c>) carries no per skill icon paths, so there is nothing to resolve.
        /// Kept for interface compatibility until the asset source exposes skill icons.
        /// </summary>
        string GetSkillIconUrl(int agentId, SkillType skillType);
        ZZZEquipmentSuitInfo GetDiscSetInfo(string suitId);
        Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets();
        IReadOnlyList<ZZZEquipmentLevelItem> GetEquipmentLevelData();
        IReadOnlyList<ZZZWeaponLevelItem> GetWeaponLevelData();
        IReadOnlyList<ZZZWeaponStarItem> GetWeaponStarData();
        IReadOnlyDictionary<string, Skin> GetAgentSkins(string agentId);
        Skin GetAgentSkin(string agentId, string skinId);
    }
}