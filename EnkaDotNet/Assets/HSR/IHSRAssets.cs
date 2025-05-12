using System.Collections.Generic;
using EnkaDotNet.Assets.HSR.Models;
using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Assets.HSR
{
    /// <summary>
    /// Interface for accessing Honkai: Star Rail specific game assets
    /// </summary>
    public interface IHSRAssets : IAssets
    {
        /// <summary>
        /// Gets the localized text for a given key
        /// </summary>
        /// <param name="key">The key for the text</param>
        /// <returns>The localized text</returns>
        string GetLocalizedText(string key);

        /// <summary>
        /// Gets the name of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's name</returns>
        string GetCharacterName(int characterId);

        /// <summary>
        /// Gets the icon URL for a character's full image
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's icon URL</returns>
        string GetCharacterIconUrl(int characterId);

        /// <summary>
        /// Gets the icon URL for a character's avatar
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's avatar icon URL</returns>
        string GetCharacterAvatarIconUrl(int characterId);

        /// <summary>
        /// Gets the element type of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's element type</returns>
        ElementType GetCharacterElement(int characterId);

        /// <summary>
        /// Gets the path type of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's path type</returns>
        PathType GetCharacterPath(int characterId);

        /// <summary>
        /// Gets the rarity of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's rarity</returns>
        int GetCharacterRarity(int characterId);

        /// <summary>
        /// Gets asset information for a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's asset information</returns>
        HSRCharacterAssetInfo GetCharacterInfo(string characterId);

        /// <summary>
        /// Gets the name of a light cone
        /// </summary>
        /// <param name="lightConeId">The ID of the light cone</param>
        /// <returns>The light cone's name</returns>
        string GetLightConeName(int lightConeId);

        /// <summary>
        /// Gets the icon URL for a light cone
        /// </summary>
        /// <param name="lightConeId">The ID of the light cone</param>
        /// <returns>The light cone's icon URL</returns>
        string GetLightConeIconUrl(int lightConeId);

        /// <summary>
        /// Gets the path type of a light cone
        /// </summary>
        /// <param name="lightConeId">The ID of the light cone</param>
        /// <returns>The light cone's path type</returns>
        PathType GetLightConePath(int lightConeId);

        /// <summary>
        /// Gets the rarity of a light cone
        /// </summary>
        /// <param name="lightConeId">The ID of the light cone</param>
        /// <returns>The light cone's rarity</returns>
        int GetLightConeRarity(int lightConeId);

        /// <summary>
        /// Gets asset information for a light cone
        /// </summary>
        /// <param name="lightConeId">The ID of the light cone</param>
        /// <returns>The light cone's asset information</returns>
        HSRLightConeAssetInfo GetLightConeInfo(string lightConeId);

        /// <summary>
        /// Gets the name of a relic set
        /// </summary>
        /// <param name="setId">The ID of the relic set</param>
        /// <returns>The relic set's name</returns>
        string GetRelicSetName(int setId);

        /// <summary>
        /// Gets the icon URL for a relic piece
        /// </summary>
        /// <param name="relicId">The ID of the relic piece</param>
        /// <returns>The relic's icon URL</returns>
        string GetRelicIconUrl(int relicId);

        /// <summary>
        /// Gets the rarity of a relic piece
        /// </summary>
        /// <param name="relicId">The ID of the relic piece</param>
        /// <returns>The relic's rarity</returns>
        int GetRelicRarity(int relicId);

        /// <summary>
        /// Gets the set ID of a relic piece
        /// </summary>
        /// <param name="relicId">The ID of the relic piece</param>
        /// <returns>The relic's set ID</returns>
        int GetRelicSetId(int relicId);

        /// <summary>
        /// Gets the type (slot) of a relic piece
        /// </summary>
        /// <param name="relicId">The ID of the relic piece</param>
        /// <returns>The relic's type</returns>
        RelicType GetRelicType(int relicId);

        /// <summary>
        /// Gets the display name of a stat property
        /// </summary>
        /// <param name="propertyId">The ID of the property (can be an enum value)</param>
        /// <returns>The property's display name</returns>
        string GetPropertyName(int propertyId);

        /// <summary>
        /// Formats the value of a stat property for display
        /// </summary>
        /// <param name="propertyId">The ID of the property</param>
        /// <param name="value">The raw value of the property</param>
        /// <returns>The formatted property value string</returns>
        string FormatPropertyValue(int propertyId, double value);

        /// <summary>
        /// Gets the icon URL for a profile picture (avatar)
        /// </summary>
        /// <param name="profilePictureId">The ID of the profile picture</param>
        /// <returns>The profile picture's icon URL</returns>
        string GetProfilePictureIconUrl(int profilePictureId);

        /// <summary>
        /// Gets the icon URL for a character's skill
        /// </summary>
        /// <param name="skillId">The ID of the skill</param>
        /// <returns>The skill's icon URL</returns>
        string GetSkillIconUrl(int skillId);

        /// <summary>
        /// Gets the icon URL for an eidolon
        /// </summary>
        /// <param name="eidolonId">The ID of the eidolon</param>
        /// <returns>The eidolon's icon URL</returns>
        string GetEidolonIconUrl(int eidolonId);

        /// <summary>
        /// Gets asset information for an eidolon
        /// </summary>
        /// <param name="eidolonId">The ID of the eidolon</param>
        /// <returns>The eidolon's asset information</returns>
        HSREidolonAssetInfo GetEidolonInfo(string eidolonId);

        /// <summary>
        /// Gets information for a relic set
        /// </summary>
        /// <param name="setId">The ID of the set</param>
        /// <returns>The relic set's information</returns>
        HSRRelicSetInfo GetRelicSetInfo(string setId);

        /// <summary>
        /// Gets all loaded relic sets
        /// </summary>
        /// <returns>A dictionary of all relic sets</returns>
        Dictionary<string, HSRRelicSetInfo> GetAllRelicSets();

        /// <summary>
        /// Gets the base stats for a character at a specific promotion level
        /// </summary>
        HSRAvatarMetaStats GetAvatarStats(string avatarId, int promotion);

        /// <summary>
        /// Gets the base stats for a light cone at a specific promotion level
        /// </summary>
        HSREquipmentMetaStats GetEquipmentStats(string equipmentId, int promotion);

        /// <summary>
        /// Gets the properties of a light cone's skill at a specific rank
        /// </summary>
        Dictionary<string, double> GetEquipmentSkillProps(string skillId, int rank);

        /// <summary>
        /// Gets the properties of a skill tree node (trace) at a specific level
        /// </summary>
        Dictionary<string, double> GetSkillTreeProps(string pointId, int level);

        /// <summary>
        /// Gets the icon URL for a skill tree node (trace)
        /// </summary>
        string GetSkillTreeIconUrl(int pointId);

        /// <summary>
        /// Gets information for a relic's main affix
        /// </summary>
        HSRRelicMainAffixInfo GetRelicMainAffixInfo(int groupId, int affixId);

        /// <summary>
        /// Gets information for a relic's sub affix
        /// </summary>
        HSRRelicSubAffixInfo GetRelicSubAffixInfo(int groupId, int affixId);

        /// <summary>
        /// Gets information for a skill tree point
        /// </summary>
        HSRSkillTreePointInfo GetSkillTreePointInfo(string pointId);

        /// <summary>
        /// Gets the localized description for a skill tree point
        /// </summary>
        string GetSkillTreePointDescription(string pointId);

        /// <summary>
        /// Gets the localized name for a skill tree point
        /// </summary>
        string GetSkillTreePointName(string pointId);

        /// <summary>
        /// Gets the effects for a relic set based on the set ID and number of pieces equipped
        /// </summary>
        /// <param name="setId">The ID of the relic set</param>
        /// <param name="pieceCount">The number of pieces from the set that are equipped (e.g, 2 or 4)</param>
        /// <returns>A dictionary of property types to their values for the set bonus</returns>
        Dictionary<string, double> GetRelicSetEffects(int setId, int pieceCount);
    }
}
