using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EnkaDotNet.Assets.Genshin
{
    public class GenshinAssets : BaseAssets, IGenshinAssets
    {
        private readonly Dictionary<int, CharacterAssetInfo> _characters = new Dictionary<int, CharacterAssetInfo>();
        private readonly Dictionary<int, WeaponAssetInfo> _weapons = new Dictionary<int, WeaponAssetInfo>();
        private readonly Dictionary<int, ArtifactAssetInfo> _artifacts = new Dictionary<int, ArtifactAssetInfo>();
        private readonly Dictionary<int, TalentAssetInfo> _talents = new Dictionary<int, TalentAssetInfo>();
        private readonly Dictionary<string, ConstellationAssetInfo> _constellations = new Dictionary<string, ConstellationAssetInfo>();
        private readonly Dictionary<string, NameCardAssetInfo> _namecards = new Dictionary<string, NameCardAssetInfo>();
        private readonly Dictionary<string, PfpAssetInfo> _pfps = new Dictionary<string, PfpAssetInfo>();

        public GenshinAssets(string language, HttpClient httpClient, ILogger<GenshinAssets> logger)
            : base(language, GameType.Genshin, httpClient, logger)
        {
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
                LoadCharacters(),
                LoadWeapons(),
                LoadArtifacts(),
                LoadTalents(),
                LoadConstellations(),
                LoadNamecards(),
                LoadPfps()
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private string GetStringFromHashJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
            if (element.ValueKind == JsonValueKind.Number)
            {
                if (element.TryGetInt64(out long longValue))
                {
                    return longValue.ToString();
                }
                return element.GetRawText();
            }
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            return element.ToString();
        }

        private async Task LoadCharacters()
        {
            _characters.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, CharacterAssetInfo>>("characters.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int charId))
                            _characters[charId] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact characters.json asset.");
                throw new InvalidOperationException("Failed to load essential Genshin Impact character data.", ex);
            }
        }

        private async Task LoadWeapons()
        {
            _weapons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, WeaponAssetInfo>>("weapons.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int weaponId))
                            _weapons[weaponId] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact weapons.json asset.");
            }
        }

        private async Task LoadArtifacts()
        {
            _artifacts.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ArtifactAssetInfo>>("artifacts.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int artifactId))
                            _artifacts[artifactId] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact artifacts.json asset.");
            }
        }


        private async Task LoadPfps()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, PfpAssetInfo>>("pfps.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        _pfps[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact pfps.json asset.");
                throw new InvalidOperationException("Failed to load essential Genshin Impact profile picture data.", ex);
            }
        }

        private async Task LoadTalents()
        {
            _talents.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, TalentAssetInfo>>("talents.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        _talents[int.Parse(kvp.Key)] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact talents.json asset.");
                throw new InvalidOperationException("Failed to load essential Genshin Impact talent data.", ex);
            }
        }

        private async Task LoadConstellations()
        {
            _constellations.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ConstellationAssetInfo>>("consts.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (kvp.Value != null && kvp.Value.NameTextMapHash.ValueKind != JsonValueKind.Undefined && kvp.Value.NameTextMapHash.ValueKind != JsonValueKind.Null)
                        {
                            _constellations[kvp.Key] = kvp.Value;
                        }
                        else if (kvp.Value != null)
                        {
                            _constellations[kvp.Key] = kvp.Value;
                            _logger.LogWarning($"Constellation '{kvp.Key}' has a null or undefined NameTextMapHash.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact consts.json asset.");
                throw new InvalidOperationException("Failed to load essential Genshin Impact constellation data.", ex);
            }
        }

        private async Task LoadNamecards()
        {
            _namecards.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, NameCardAssetInfo>>("namecards.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        _namecards[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact namecards.json asset.");
                throw new InvalidOperationException("Failed to load essential Genshin Impact namecard data.", ex);
            }
        }

        public string GetCharacterName(int characterId)
        {
            if (_characters.TryGetValue(characterId, out var charInfo))
            {
                string hashValue = GetStringFromHashJsonElement(charInfo.NameTextMapHash);
                if (hashValue != null)
                    return GetText(hashValue);
            }
            return $"Character_{characterId}";
        }

        public string GetCharacterIconUrl(int characterId)
        {
            if (_characters.TryGetValue(characterId, out var charInfo))
            {
                if (!string.IsNullOrEmpty(charInfo.SideIconName))
                {
                    string iconName = charInfo.SideIconName.Replace("UI_AvatarIcon_Side_", "UI_AvatarIcon_");
                    return $"{Constants.GetAssetCdnBaseUrl(GameType)}{iconName}.png";
                }
            }
            return string.Empty;
        }

        public ElementType GetCharacterElement(int characterId) => _characters.TryGetValue(characterId, out var charInfo) && charInfo.Element != null ? MapElementNameToEnum(charInfo.Element) : ElementType.Unknown;

        public string GetWeaponName(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId, out var weaponInfo))
            {
                string hashValue = GetStringFromHashJsonElement(weaponInfo.NameTextMapHash);
                if (hashValue != null)
                    return GetText(hashValue);
            }
            return $"Weapon_{weaponId}";
        }

        public string GetWeaponNameFromHash(string nameHash) => GetText(nameHash);
        public string GetWeaponIconUrl(int weaponId) => _weapons.TryGetValue(weaponId, out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.Icon) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{weaponInfo.Icon}.png" : string.Empty;
        public string GetWeaponIconUrlFromIconName(string iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{iconName}.png" : string.Empty;
        public WeaponType GetWeaponType(int weaponId) => _weapons.TryGetValue(weaponId, out var weaponInfo) && weaponInfo.WeaponType != null ? MapWeaponTypeNameToEnum(weaponInfo.WeaponType) : WeaponType.Unknown;

        public string GetArtifactName(int artifactId)
        {
            if (_artifacts.TryGetValue(artifactId, out var artifactInfo))
            {
                string hashValue = GetStringFromHashJsonElement(artifactInfo.NameTextMapHash);
                if (hashValue != null)
                    return GetText(hashValue);
            }
            return $"Artifact_{artifactId}";
        }
        public string GetArtifactNameFromHash(string nameHash) => GetText(nameHash);
        public string GetArtifactSetNameFromHash(string setNameHash) => GetText(setNameHash);
        public string GetArtifactIconUrl(int artifactId) => _artifacts.TryGetValue(artifactId, out var artifactInfo) && !string.IsNullOrEmpty(artifactInfo.Icon) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{artifactInfo.Icon}.png" : string.Empty;
        public string GetArtifactIconUrlFromIconName(string iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{iconName}.png" : string.Empty;

        public string GetTalentName(int talentId)
        {
            if (_talents.TryGetValue(talentId, out var talentInfo))
            {
                string nameHashValue = GetStringFromHashJsonElement(talentInfo.NameTextMapHash);
                if (!string.IsNullOrEmpty(nameHashValue))
                {
                    string nameFromHash = GetText(nameHashValue);
                    if (!string.IsNullOrEmpty(nameFromHash) && nameFromHash != nameHashValue)
                    {
                        return nameFromHash;
                    }
                }
                if (!string.IsNullOrEmpty(talentInfo.Name))
                {
                    string nameFromDirectProperty = GetText(talentInfo.Name);
                    if (!string.IsNullOrEmpty(nameFromDirectProperty) && nameFromDirectProperty != talentInfo.Name)
                    {
                        return nameFromDirectProperty;
                    }
                    return talentInfo.Name;
                }
            }
            return $"Talent_{talentId}";
        }


        public string GetProfilePictureIconUrl(int characterId)
        {
            string key = characterId.ToString();
            if (_pfps.TryGetValue(key, out var pfpInfo))
            {
                if (!string.IsNullOrEmpty(pfpInfo.IconPath))
                {
                    string adjustedIconPath = pfpInfo.IconPath.Replace("_Circle", "");
                    return $"{Constants.GetAssetCdnBaseUrl(GameType)}{adjustedIconPath}.png";
                }
            }
            return GetCharacterIconUrl(characterId);
        }

        public string GetNameCardIconUrl(int nameCardId) => _namecards.TryGetValue(nameCardId.ToString(), out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{nameCardInfo.Icon}.png" : string.Empty;

        public string GetConstellationName(int constellationId)
        {
            if (_constellations.TryGetValue(constellationId.ToString(), out var constellationInfo))
            {
                string hashValue = GetStringFromHashJsonElement(constellationInfo.NameTextMapHash);
                if (hashValue != null)
                    return GetText(hashValue);
            }
            return $"Constellation_{constellationId}";
        }

        public string GetTalentIconUrl(int talentId)
        {
            _talents.TryGetValue(talentId, out var talentInfo);
            if (talentInfo != null && !string.IsNullOrEmpty(talentInfo.Icon))
            {
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{talentInfo.Icon}.png";
            }

            return string.Empty;
        }

        public string GetConstellationIconUrl(int constellationId) => _constellations.TryGetValue(constellationId.ToString(), out var constellationInfo) && !string.IsNullOrEmpty(constellationInfo.Icon) ? $"{Constants.GetAssetCdnBaseUrl(GameType)}{constellationInfo.Icon}.png" : string.Empty;


        private ElementType MapElementNameToEnum(string elementName)
        {
            if (elementName == null) return ElementType.Unknown;
            switch (elementName.ToUpperInvariant())
            {
                case "FIRE": return ElementType.Pyro;
                case "WATER": return ElementType.Hydro;
                case "WIND": return ElementType.Anemo;
                case "ELECTRO": case "ELECTRIC": return ElementType.Electro;
                case "GRASS": return ElementType.Dendro;
                case "ICE": return ElementType.Cryo;
                case "ROCK": return ElementType.Geo;
                default: return ElementType.Unknown;
            }
        }

        private WeaponType MapWeaponTypeNameToEnum(string weaponTypeName)
        {
            if (weaponTypeName == null) return WeaponType.Unknown;
            switch (weaponTypeName.ToUpperInvariant())
            {
                case "WEAPON_SWORD_ONE_HAND": return WeaponType.Sword;
                case "WEAPON_CLAYMORE": return WeaponType.Claymore;
                case "WEAPON_POLE": return WeaponType.Polearm;
                case "WEAPON_BOW": return WeaponType.Bow;
                case "WEAPON_CATALYST": return WeaponType.Catalyst;
                default: return WeaponType.Unknown;
            }
        }
    }
}
