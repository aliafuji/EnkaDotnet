using EnkaSharp.Enums.Genshin;

namespace EnkaSharp.Assets.Genshin
{
    public interface IGenshinAssets : IAssets
    {
        string GetCharacterName(int characterId);
        string GetCharacterIconUrl(int characterId);
        ElementType GetCharacterElement(int characterId);
        string GetWeaponName(int weaponId);
        string GetWeaponNameFromHash(string? nameHash);
        string GetWeaponIconUrl(int weaponId);
        string GetWeaponIconUrlFromIconName(string? iconName);
        WeaponType GetWeaponType(int weaponId);
        string GetArtifactName(int artifactId);
        string GetArtifactNameFromHash(string? nameHash);
        string GetArtifactSetNameFromHash(string? setNameHash);
        string GetArtifactIconUrl(int artifactId);
        string GetArtifactIconUrlFromIconName(string? iconName);
        string GetTalentName(int talentId);
        string GetTalentIconUrl(int talentId);
        string GetConstellationName(int constellationId);
        string GetConstellationIconUrl(int constellationId);
        string GetNameCardIconUrl(int nameCardId);
    }
}