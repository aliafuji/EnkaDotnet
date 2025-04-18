﻿using System.Text.Json;
using EnkaDotNet.Assets.Genshin.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils;

namespace EnkaDotNet.Assets.Genshin
{
    public class GenshinAssets : BaseAssets, IGenshinAssets
    {
        private static readonly Dictionary<string, string> GenshinAssetUrls = new()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/text_map.json" },
            { "characters.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/characters.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/namecards.json" },
            { "consts.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/consts.json" },
            { "talents.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/talents.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/pfps.json" }
        };

        private readonly Dictionary<int, CharacterAssetInfo> _characters = new();
        private readonly Dictionary<int, WeaponAssetInfo> _weapons = new();
        private readonly Dictionary<int, ArtifactAssetInfo> _artifacts = new();
        private readonly Dictionary<string, ArtifactSetAssetInfo> _artifactSets = new();
        private readonly Dictionary<int, TalentAssetInfo> _talents = new();
        private readonly Dictionary<string, ConstellationAssetInfo> _constellations = new();
        private readonly Dictionary<string, NameCardAssetInfo> _namecards = new();
        private readonly Dictionary<string, PfpAssetInfo> _pfps = new();

        public GenshinAssets(string language = "en")
            : base(language, GameType.Genshin)
        {
        }

        protected override Dictionary<string, string> GetAssetUrls() => GenshinAssetUrls;

        protected override async Task LoadAssets()
        {
            var tasks = new List<Task>
            {
                LoadCharacters(),
                LoadTalents(),
                LoadConstellations(),
                LoadNamecards(),
                LoadPfps()
            };

            await Task.WhenAll(tasks);
        }

        private async Task LoadCharacters()
        {
            _characters.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, CharacterAssetInfo>>("characters.json");
                
                foreach (var kvp in deserializedMap)
                {
                    if (int.TryParse(kvp.Key, out int charId)) 
                        _characters[charId] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading characters: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential characters data", ex);
            }
        }

        private async Task LoadPfps()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, PfpAssetInfo>>("pfps.json");

                foreach (var kvp in deserializedMap)
                {
                    if (int.TryParse(kvp.Key, out int pfpId))
                        _pfps[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading pfps: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential pfps data", ex);
            }
        }

        private async Task LoadTalents()
        {
            _talents.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, TalentAssetInfo>>("talents.json");
                
                foreach (var kvp in deserializedMap)
                {
                    if (int.TryParse(kvp.Key, out int talentId)) 
                        _talents[talentId] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading talents: {ex.Message}. Talent data may be incomplete.");
            }
        }

        private async Task LoadConstellations()
        {
            _constellations.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ConstellationAssetInfo>>("consts.json");
                
                foreach (var kvp in deserializedMap)
                {
                    if (int.TryParse(kvp.Key, out int constellationId))
                    {
                        if (kvp.Value.NameTextMapHash != null)
                        {
                            _constellations[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading constellations: {ex.Message}. Constellation data may be incomplete.");
            }
        }

        private async Task<string> LoadNamecards()
        {
            _namecards.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, NameCardAssetInfo>>("namecards.json");
                
                foreach (var kvp in deserializedMap)
                {
                    if (int.TryParse(kvp.Key, out int namecardId)) 
                        _namecards[namecardId.ToString()] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading namecards: {ex.Message}. Namecard data may be incomplete.");
            }
            return string.Empty;
        }

        public string GetCharacterName(int characterId) => _characters.TryGetValue(characterId, out var charInfo) && charInfo.NameTextMapHash != null ? GetText(charInfo.NameTextMapHash) : $"Character_{characterId}";

        public string GetCharacterIconUrl(int characterId)
        {
            if (_characters.TryGetValue(characterId, out var charInfo))
            {
                if (!string.IsNullOrEmpty(charInfo.SideIconName))
                {
                    string iconName = charInfo.SideIconName.Replace("UI_AvatarIcon_Side_", "UI_AvatarIcon_");
                    return $"{Constants.GetAssetBaseUrl(GameType)}{iconName}.png";
                }
            }
            return string.Empty;
        }

        public ElementType GetCharacterElement(int characterId) => _characters.TryGetValue(characterId, out var charInfo) && charInfo.Element != null ? MapElementNameToEnum(charInfo.Element) : ElementType.Unknown;

        public string GetWeaponName(int weaponId) => _weapons.TryGetValue(weaponId, out var weaponInfo) && weaponInfo.NameTextMapHash != null ? GetText(weaponInfo.NameTextMapHash) : $"Weapon_{weaponId}";

        public string GetWeaponNameFromHash(string? nameHash) => GetText(nameHash);

        public string GetWeaponIconUrl(int weaponId) => _weapons.TryGetValue(weaponId, out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.Icon) ? $"{Constants.GetAssetBaseUrl(GameType)}{weaponInfo.Icon}.png" : string.Empty;

        public string GetWeaponIconUrlFromIconName(string? iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.GetAssetBaseUrl(GameType)}{iconName}.png" : string.Empty;

        public WeaponType GetWeaponType(int weaponId) => _weapons.TryGetValue(weaponId, out var weaponInfo) && weaponInfo.WeaponType != null ? MapWeaponTypeNameToEnum(weaponInfo.WeaponType) : WeaponType.Unknown;

        public string GetArtifactName(int artifactId) => _artifacts.TryGetValue(artifactId, out var artifactInfo) && artifactInfo.NameTextMapHash != null ? GetText(artifactInfo.NameTextMapHash) : $"Artifact_{artifactId}";

        public string GetArtifactNameFromHash(string? nameHash) => GetText(nameHash);

        public string GetArtifactSetNameFromHash(string? setNameHash) => GetText(setNameHash);

        public string GetArtifactIconUrl(int artifactId) => _artifacts.TryGetValue(artifactId, out var artifactInfo) && !string.IsNullOrEmpty(artifactInfo.Icon) ? $"{Constants.GetAssetBaseUrl(GameType)}{artifactInfo.Icon}.png" : string.Empty;

        public string GetArtifactIconUrlFromIconName(string? iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.GetAssetBaseUrl(GameType)}{iconName}.png" : string.Empty;

        public string GetTalentName(int talentId)
        {
            if (_talents.TryGetValue(talentId, out var talentInfo))
            {
                if (!string.IsNullOrEmpty(talentInfo.NameTextMapHash))
                {
                    string name = GetText(talentInfo.NameTextMapHash);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }

                if (!string.IsNullOrEmpty(talentInfo.Name))
                {
                    return talentInfo.Name;
                }
            }

            return $"Talent_{talentId}";
        }

        public string GetProfilePictureIconUrl(int characterId)
        {
            if (_pfps.TryGetValue(characterId.ToString(), out var pfpInfo))
            {
                if (!string.IsNullOrEmpty(pfpInfo.IconPath))
                {
                    string NewIcon = pfpInfo.IconPath.Replace("_Circle", "");
                    return $"{Constants.GetAssetBaseUrl(GameType)}{NewIcon}.png";
                }
            }
            return string.Empty;
        }

        public string GetNameCardIconUrl(int nameCardId) => _namecards.TryGetValue(nameCardId.ToString(), out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon) ? $"{Constants.GetAssetBaseUrl(GameType)}{nameCardInfo.Icon}.png" : string.Empty;

        public string GetConstellationName(int constellationId)
        {
            if (_constellations.TryGetValue(constellationId.ToString(), out var constellationInfo))
            {
                return GetText(constellationInfo.NameTextMapHash);
            }

            return $"Constellation_{constellationId}";
        }
        public string GetConstellationIconUrl(int constellationId) => _constellations.TryGetValue(constellationId.ToString(), out var constellationInfo) && !string.IsNullOrEmpty(constellationInfo.Icon) ? $"{Constants.GetAssetBaseUrl(GameType)}{constellationInfo.Icon}.png" : string.Empty;

        public string GetTalentIconUrl(int talentId) => _talents.TryGetValue(talentId, out var talentInfo) && !string.IsNullOrEmpty(talentInfo.Icon) ? $"{Constants.GetAssetBaseUrl(GameType)}{talentInfo.Icon}.png" : string.Empty;

        private ElementType MapElementNameToEnum(string elementName) => elementName?.ToUpperInvariant() switch
        {
            "FIRE" => ElementType.Pyro,
            "WATER" => ElementType.Hydro,
            "WIND" => ElementType.Anemo,
            "ELECTRO" => ElementType.Electro,
            "ELECTRIC" => ElementType.Electro,
            "GRASS" => ElementType.Dendro,
            "ICE" => ElementType.Cryo,
            "ROCK" => ElementType.Geo,
            _ => ElementType.Unknown
        };

        private WeaponType MapWeaponTypeNameToEnum(string weaponTypeName) => weaponTypeName?.ToUpperInvariant() switch
        {
            "WEAPON_SWORD_ONE_HAND" => WeaponType.Sword,
            "WEAPON_CLAYMORE" => WeaponType.Claymore,
            "WEAPON_POLE" => WeaponType.Polearm,
            "WEAPON_BOW" => WeaponType.Bow,
            "WEAPON_CATALYST" => WeaponType.Catalyst,
            _ => WeaponType.Unknown
        };
    }
}