using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Assets.ZZZ
{
    public interface IZZZAssets : IAssets
    {
        string GetLocalizedText(string key);

        string GetAgentName(int agentId);
        string GetAgentIconUrl(int agentId);
        string GetAgentCircleIconUrl(int agentId);
        List<ElementType> GetAgentElements(int agentId);
        ProfessionType GetAgentProfessionType(int agentId);
        int GetAgentRarity(int agentId);

        ZZZAvatarAssetInfo? GetAvatarInfo(string agentId);

        string GetWeaponName(int weaponId);
        string GetWeaponIconUrl(int weaponId);
        ProfessionType GetWeaponType(int weaponId);
        int GetWeaponRarity(int weaponId);

        ZZZWeaponAssetInfo? GetWeaponInfo(string weaponId);

        string GetDriveDiscSuitName(int suitId);
        string GetDriveDiscSuitIconUrl(int suitId);
        int GetDriveDiscRarity(int discId);
        int GetDriveDiscSuitId(int discId);

        string GetPropertyName(int propertyId);
        string FormatPropertyValue(int propertyId, double value);

        string GetTitleText(int titleId);
        string GetMedalName(int medalId);
        string GetMedalIconUrl(int medalId);

        string GetNameCardIconUrl(int nameCardId);
        string GetProfilePictureIconUrl(int profilePictureId);
        string GetSkillIconUrl(int agentId, SkillType skillType);

        ZZZEquipmentSuitInfo? GetDiscSetInfo(string suitId);
        Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets();
    }
}