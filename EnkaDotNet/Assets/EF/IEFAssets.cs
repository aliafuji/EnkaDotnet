using System.Collections.Generic;
using EnkaDotNet.Assets.EF.Models;

namespace EnkaDotNet.Assets.EF
{
    public interface IEFAssets : IAssets
    {
        string GetLocalizedText(string key);
        EFAvatarAssetInfo GetAvatarInfo(int operatorId);
        EFAvatarAssetInfo GetAvatarInfo(string operatorId);
        string GetOperatorName(int operatorId);
        string GetOperatorIconUrl(int operatorId);
        string GetOperatorIconUrl(string strId);
        string GetOperatorRoundIconUrl(int operatorId);
        string GetOperatorSplashArtUrl(int operatorId);
        string GetOperatorSilhouetteUrl(int operatorId);
        string GetProfessionIconUrl(string profession);
        string GetWeaponName(int weaponId);
        string GetWeaponIconUrl(int weaponId);
        EFWeaponAssetInfo GetWeaponInfo(int weaponId);
        string GetEquipIconUrl(int equipId);
        EFEquipItemInfo GetEquipItem(int equipId);
        string GetSuitName(string suitId);
        string GetSuitIconUrl(string suitId);
        EFEquipSuitInfo GetSuitInfo(string suitId);
        EFSkillAssetInfo GetSkillProp(int skillId);
        double GetWeaponAtk(string levelTemplateId, int weaponLevel);
        IReadOnlyList<EFSkillLevelBound> GetBreakBounds(string breakthroughTemplateId, int breakthroughLevel);
        EFGemTermInfo GetGemTermTag(int termNumId);
        string GetGemIconUrl(int gemTemplateId);
        string GetGemTermIconUrl(string tagIconPath);
        string GetProfilePictureIconUrl(int profilePictureId);
        string GetNameCardIconUrl(int nameCardId);
        string GetMedalName(int medalId);
        string GetMedalIconUrl(int medalId, int level, bool isPlated = false);
        string GetSkillIconUrl(int operatorId, string skillId);
        string GetNodeSkillIconUrl(int operatorId, string nodeId);
        EFAvatarNodeSkillInfo GetNodeSkillInfo(int operatorId, string nodeId);
        int? ResolveStrIdToTemplateId(string strId);
        EFWeaponMetaData GetWeaponMeta();
    }
}
