using System.Collections.Generic;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Assets.ZZZ
{
    /// <summary>
    /// Interface for accessing Zenless Zone Zero specific game assets
    /// </summary>
    public interface IZZZAssets : IAssets
    {
        /// <summary>
        /// Gets the localized text for a given key
        /// </summary>
        /// <param name="key">The key for the text</param>
        /// <returns>The localized text</returns>
        string GetLocalizedText(string key);

        /// <summary>
        /// Gets the name of an agent
        /// </summary>
        string GetAgentName(int agentId);
        /// <summary>
        /// Gets the icon URL for an agent
        /// </summary>
        string GetAgentIconUrl(int agentId);
        /// <summary>
        /// Gets the circular icon URL for an agent
        /// </summary>
        string GetAgentCircleIconUrl(int agentId);
        /// <summary>
        /// Gets the element types of an agent
        /// </summary>
        List<ElementType> GetAgentElements(int agentId);
        /// <summary>
        /// Gets the profession type of an agent
        /// </summary>
        ProfessionType GetAgentProfessionType(int agentId);
        /// <summary>
        /// Gets the rarity of an agent
        /// </summary>
        int GetAgentRarity(int agentId);
        /// <summary>
        /// Gets asset information for an agent (avatar)
        /// </summary>
        ZZZAvatarAssetInfo GetAvatarInfo(string agentId);
        /// <summary>
        /// Gets the colors associated with an agent's assets
        /// </summary>
        List<ZZZAvatarColors> GetAvatarColors(int agentId);

        /// <summary>
        /// Gets the name of a W-Engine (weapon)
        /// </summary>
        string GetWeaponName(int weaponId);
        /// <summary>
        /// Gets the icon URL for a W-Engine (weapon)
        /// </summary>
        string GetWeaponIconUrl(int weaponId);
        /// <summary>
        /// Gets the profession type compatible with a W-Engine (weapon)
        /// </summary>
        ProfessionType GetWeaponType(int weaponId);
        /// <summary>
        /// Gets the rarity of a W-Engine (weapon)
        /// </summary>
        int GetWeaponRarity(int weaponId);
        /// <summary>
        /// Gets asset information for a W-Engine (weapon)
        /// </summary>
        ZZZWeaponAssetInfo GetWeaponInfo(string weaponId);

        /// <summary>
        /// Gets the name of a Drive Disc suit (set)
        /// </summary>
        string GetDriveDiscSuitName(int suitId);
        /// <summary>
        /// Gets the icon URL for a Drive Disc suit (set)
        /// </summary>
        string GetDriveDiscSuitIconUrl(int suitId);
        /// <summary>
        /// Gets the rarity of a Drive Disc
        /// </summary>
        int GetDriveDiscRarity(int discId);
        /// <summary>
        /// Gets the suit (set) ID of a Drive Disc
        /// </summary>
        int GetDriveDiscSuitId(int discId);
        /// <summary>
        /// Gets information for a Drive Disc set
        /// </summary>
        ZZZEquipmentSuitInfo GetDiscSetInfo(string suitId);
        /// <summary>
        /// Gets all loaded Drive Disc sets
        /// </summary>
        Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets();

        /// <summary>
        /// Gets the name of a stat property
        /// </summary>
        string GetPropertyName(int propertyId);
        /// <summary>
        /// Formats the value of a stat property for display
        /// </summary>
        string FormatPropertyValue(int propertyId, double value);

        /// <summary>
        /// Gets the text for a player title
        /// </summary>
        string GetTitleText(int titleId);
        /// <summary>
        /// Gets the name of a medal
        /// </summary>
        string GetMedalName(int medalId);
        /// <summary>
        /// Gets the icon URL for a medal
        /// </summary>
        string GetMedalIconUrl(int medalId);
        /// <summary>
        /// Gets the icon URL for a name card
        /// </summary>
        string GetNameCardIconUrl(int nameCardId);
        /// <summary>
        /// Gets the icon URL for a profile picture
        /// </summary>
        string GetProfilePictureIconUrl(int profilePictureId);

        /// <summary>
        /// Gets the icon URL for an agent's skill (placeholder, may not be available)
        /// </summary>
        string GetSkillIconUrl(int agentId, SkillType skillType);

        /// <summary>
        /// Gets the leveling data for Drive Discs (equipment)
        /// </summary>
        List<ZZZEquipmentLevelItem> GetEquipmentLevelData();
        /// <summary>
        /// Gets the leveling data for W-Engines (weapons)
        /// </summary>
        List<ZZZWeaponLevelItem> GetWeaponLevelData();
        /// <summary>
        /// Gets the star/ascension data for W-Engines (weapons)
        /// </summary>
        List<ZZZWeaponStarItem> GetWeaponStarData();
    }
}
