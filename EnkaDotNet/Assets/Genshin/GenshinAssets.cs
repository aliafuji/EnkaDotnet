using System.Text.Json;
using EnkaDotNet.Assets.Genshin.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;

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

        public GenshinAssets(string assetsBasePath, string language = "en")
            : base(assetsBasePath, language, GameType.Genshin)
        {
        }

        protected override Dictionary<string, string> GetAssetUrls() => GenshinAssetUrls;

        protected override void LoadAssets()
        {
            LoadCharacters();
            LoadTalents();
            LoadConstellations();
            LoadNamecards();
        }

        private void LoadCharacters()
        {
            string filePath = Path.Combine(_gameSpecificPath, "characters.json");
            _characters.Clear();
            try
            {
                if (!File.Exists(filePath)) throw new FileNotFoundException($"characters.json file not found.", filePath);
                var jsonData = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonData)) throw new InvalidOperationException("characters.json file is empty.");

                var deserializedMap = JsonSerializer.Deserialize<Dictionary<string, CharacterAssetInfo>>(jsonData, GetJsonOptions());
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int charId)) _characters[charId] = kvp.Value;
                    }
                }
                Console.WriteLine($"[Assets] Loaded {_characters.Count} character assets for {GameType}");
            }
            catch (FileNotFoundException ex) { Console.WriteLine($"[Assets] Error: {ex.Message}"); throw new InvalidOperationException($"Failed to load essential characters.json", ex); }
            catch (JsonException ex) { Console.WriteLine($"[Assets] Error parsing characters.json JSON: {ex.Message}"); throw new InvalidOperationException($"Failed to parse essential characters.json", ex); }
            catch (Exception ex) { Console.WriteLine($"[Assets] Error loading characters.json: {ex.Message}"); throw new InvalidOperationException($"Failed to load essential characters.json", ex); }
        }

        private void LoadTalents()
        {
            string filePath = Path.Combine(_gameSpecificPath, "talents.json");
            _talents.Clear();
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Assets] Warning: talents.json not found at {filePath}. Talent data will be limited.");
                    return;
                }

                var jsonData = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine($"[Assets] Warning: talents.json file is empty.");
                    return;
                }

                var deserializedMap = JsonSerializer.Deserialize<Dictionary<string, TalentAssetInfo>>(jsonData, GetJsonOptions());
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int talentId)) _talents[talentId] = kvp.Value;
                    }
                }

                Console.WriteLine($"[Assets] Loaded {_talents.Count} talent assets for {GameType}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing talents.json JSON: {ex.Message}. Talent data may be incomplete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading talents.json: {ex.Message}. Talent data may be incomplete.");
            }
        }

        private void LoadConstellations()
        {
            string filePath = Path.Combine(_gameSpecificPath, "consts.json");
            _constellations.Clear();
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Assets] Warning: consts.json not found at {filePath}. Constellation data will be limited.");
                    return;
                }

                var jsonData = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine($"[Assets] Warning: consts.json file is empty.");
                    return;
                }

                var deserializedMap = JsonSerializer.Deserialize<Dictionary<string, ConstellationAssetInfo>>(jsonData, GetJsonOptions());
                if (deserializedMap != null)
                {
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

                Console.WriteLine($"[Assets] Loaded {_constellations.Count} constellations assets for {GameType}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing const.json JSON: {ex.Message}. Talent data may be incomplete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading const.json: {ex.Message}. Talent data may be incomplete.");
            }
        }

        public string LoadNamecards()
        {
            string filePath = Path.Combine(_gameSpecificPath, "namecards.json");
            _namecards.Clear();
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Assets] Warning: namecards.json not found at {filePath}. Namecard data will be limited.");
                    return string.Empty;
                }

                var jsonData = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine($"[Assets] Warning: namecards.json file is empty.");
                    return string.Empty;
                }

                var deserializedMap = JsonSerializer.Deserialize<Dictionary<string, NameCardAssetInfo>>(jsonData, GetJsonOptions());
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (int.TryParse(kvp.Key, out int namecardId)) _namecards[namecardId.ToString()] = kvp.Value;
                    }
                }

                Console.WriteLine($"[Assets] Loaded {_namecards.Count} namecard assets for {GameType}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing namecards.json JSON: {ex.Message}. Namecard data may be incomplete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading namecards.json: {ex.Message}. Namecard data may be incomplete.");
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