using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Assets.Genshin
{
    /// <summary>
    /// Interface for accessing Genshin Impact specific game assets
    /// </summary>
    public interface IGenshinAssets : IAssets
    {
        /// <summary>
        /// Gets the name of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's name</returns>
        string GetCharacterName(int characterId);

        /// <summary>
        /// Gets the icon URL for a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's icon URL</returns>
        string GetCharacterIconUrl(int characterId);

        /// <summary>
        /// Gets the element type of a character
        /// </summary>
        /// <param name="characterId">The ID of the character</param>
        /// <returns>The character's element type</returns>
        ElementType GetCharacterElement(int characterId);

        /// <summary>
        /// Gets the name of a weapon from its name hash
        /// </summary>
        /// <param name="nameHash">The name hash of the weapon</param>
        /// <returns>The weapon's name</returns>
        string GetWeaponNameFromHash(string nameHash);

        /// <summary>
        /// Gets the icon URL for a weapon from its icon name
        /// </summary>
        /// <param name="iconName">The icon name of the weapon</param>
        /// <returns>The weapon's icon URL</returns>
        string GetWeaponIconUrlFromIconName(string iconName);

        /// <summary>
        /// Gets the name of an artifact from its name hash
        /// </summary>
        /// <param name="nameHash">The name hash of the artifact</param>
        /// <returns>The artifact's name</returns>
        string GetArtifactNameFromHash(string nameHash);

        /// <summary>
        /// Gets the name of an artifact set from its set name hash
        /// </summary>
        /// <param name="setNameHash">The set name hash of the artifact set</param>
        /// <returns>The artifact set's name</returns>
        string GetArtifactSetNameFromHash(string setNameHash);

        /// <summary>
        /// Gets the icon URL for an artifact from its icon name
        /// </summary>
        /// <param name="iconName">The icon name of the artifact</param>
        /// <returns>The artifact's icon URL</returns>
        string GetArtifactIconUrlFromIconName(string iconName);

        /// <summary>
        /// Gets the name of a talent
        /// </summary>
        /// <param name="talentId">The ID of the talent</param>
        /// <returns>The talent's name</returns>
        string GetTalentName(int talentId);

        /// <summary>
        /// Gets the icon URL for a talent
        /// </summary>
        /// <param name="talentId">The ID of the talent</param>
        /// <returns>The talent's icon URL</returns>
        string GetTalentIconUrl(int talentId);

        /// <summary>
        /// Gets the name of a constellation
        /// </summary>
        /// <param name="constellationId">The ID of the constellation</param>
        /// <returns>The constellation's name</returns>
        string GetConstellationName(int constellationId);

        /// <summary>
        /// Gets the icon URL for a constellation
        /// </summary>
        /// <param name="constellationId">The ID of the constellation</param>
        /// <returns>The constellation's icon URL</returns>
        string GetConstellationIconUrl(int constellationId);

        /// <summary>
        /// Gets the icon URL for a name card
        /// </summary>
        /// <param name="nameCardId">The ID of the name card</param>
        /// <returns>The name card's icon URL</returns>
        string GetNameCardIconUrl(int nameCardId);

        /// <summary>
        /// Gets the icon URL for a profile picture (avatar)
        /// </summary>
        /// <param name="profilePictureId">The ID of the profile picture, typically a character ID</param>
        /// <returns>The profile picture's icon URL</returns>
        string GetProfilePictureIconUrl(int profilePictureId);
    }
}
