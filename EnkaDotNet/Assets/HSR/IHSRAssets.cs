using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.HSR.Models;
using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Assets.HSR
{
    public interface IHSRAssets : IAssets
    {
        string GetLocalizedText(string key);
        string GetCharacterName(int characterId);
        string GetCharacterIconUrl(int characterId);
        string GetCharacterAvatarIconUrl(int characterId);
        ElementType GetCharacterElement(int characterId);
        PathType GetCharacterPath(int characterId);
        int GetCharacterRarity(int characterId);
        HSRCharacterAssetInfo GetCharacterInfo(string characterId);
        string GetLightConeName(int lightConeId);
        string GetLightConeIconUrl(int lightConeId);
        PathType GetLightConePath(int lightConeId);
        int GetLightConeRarity(int lightConeId);
        HSRLightConeAssetInfo GetLightConeInfo(string lightConeId);
        string GetRelicSetName(int setId);
        string GetRelicIconUrl(int relicId);
        int GetRelicRarity(int relicId);
        int GetRelicSetId(int relicId);
        RelicType GetRelicType(int relicId);
        string GetPropertyName(int propertyId);
        string FormatPropertyValue(int propertyId, double value);
        string GetNameCardIconUrl(int nameCardId);
        string GetProfilePictureIconUrl(int profilePictureId);
        string GetSkillIconUrl(int characterId, SkillType skillType);
        string GetEidolonIconUrl(int eidolonId);
        HSREidolonAssetInfo GetEidolonInfo(string eidolonId);
        HSRRelicSetInfo GetRelicSetInfo(string setId);
        Dictionary<string, HSRRelicSetInfo> GetAllRelicSets();
        Dictionary<string, double> GetCharacterBaseStats(int characterId, int promotionLevel);
        Dictionary<string, double> GetLightConeBaseStats(int lightConeId, int promotionLevel);
        Dictionary<string, double> GetLightConeSkillEffects(int lightConeId, int rank);
        Dictionary<string, double> GetRelicMainStatInfo(int mainAffixGroup, int mainAffixId, int level);
        Dictionary<string, double> GetTraceEffects(int traceId, int level);
        Dictionary<string, double> GetRelicSetEffects(int setId, int pieceCount);
        HSRAvatarMetaStats GetAvatarStats(string avatarId, int promotion);
        HSREquipmentMetaStats GetEquipmentStats(string equipmentId, int promotion);
        Dictionary<string, double> GetEquipmentSkillProps(string skillId, int rank);
        Dictionary<string, double> GetSkillTreeProps(string pointId, int level);
        string GetSkillTreeIconUrl(int pointId);
        HSRRelicMainAffixInfo GetRelicMainAffixInfo(int groupId, int affixId);
        HSRRelicSubAffixInfo GetRelicSubAffixInfo(int groupId, int affixId);
        Dictionary<string, double> GetRelicMainAffixValueAtLevel(int groupId, int affixId, int level);
        Dictionary<string, double> GetRelicSubAffixValueAtStep(int groupId, int affixId, int step);
        HSRSkillTreePointInfo GetSkillTreePointInfo(string pointId);
        string GetSkillTreePointDescription(string pointId);
        string GetSkillTreePointName(string pointId);
    }
}
