using System.Text.Json;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.ZZZ;

namespace EnkaDotNet.Assets.ZZZ
{
    public class ZZZAssets : BaseAssets, IZZZAssets
    {
        private static readonly Dictionary<string, string> ZZZAssetUrls = new()
        {
            { "text_map.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/locs.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/avatars.json" },
            { "weapons.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/weapons.json" },
            { "equipments.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/equipments.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/pfps.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/namecards.json" },
            { "medals.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/medals.json" },
            { "titles.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/titles.json" },
            { "property.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/property.json" },
            { "equipment_level.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/equipment_level.json" },
            { "weapon_level.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/weapon_level.json" },
            { "weapon_star.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/weapon_star.json" }
        };

        private readonly Dictionary<string, ZZZAvatarAssetInfo> _avatars = new();
        private readonly Dictionary<string, ZZZWeaponAssetInfo> _weapons = new();
        private readonly Dictionary<string, ZZZPfpAssetInfo> _pfps = new();
        private readonly Dictionary<string, ZZZNameCardAssetInfo> _namecards = new();
        private readonly Dictionary<string, ZZZTitleAssetInfo> _titles = new();
        private readonly Dictionary<string, ZZZMedalAssetInfo> _medals = new();
        private readonly Dictionary<string, ZZZPropertyAssetInfo> _properties = new();
        private readonly Dictionary<string, ZZZEquipmentItemInfo> _equipmentItems = new();
        private readonly Dictionary<string, ZZZEquipmentSuitInfo> _equipmentSuits = new();
        private Dictionary<string, string>? _localization;

        private List<ZZZEquipmentLevelItem>? _equipmentLevelData;
        private List<ZZZWeaponLevelItem>? _weaponLevelData;
        private List<ZZZWeaponStarItem>? _weaponStarData;

        public ZZZAssets(string language = "en")
            : base(language, GameType.ZZZ)
        {
        }

        protected override Dictionary<string, string> GetAssetUrls() => ZZZAssetUrls;

        protected override async Task LoadAssets()
        {
            var tasks = new List<Task>
            {
                LoadLocalizations(),
                LoadAvatars(),
                LoadWeapons(),
                LoadEquipments(),
                LoadPfps(),
                LoadNamecards(),
                LoadMedals(),
                LoadTitles(),
                LoadProperties(),
                LoadEquipmentLevel(),
                LoadWeaponLevel(),
                LoadWeaponStar()
            };

            await Task.WhenAll(tasks);
        }

        private async Task LoadEquipmentLevel()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZEquipmentLevelData>("equipment_level.json");
                _equipmentLevelData = data?.Items ?? new List<ZZZEquipmentLevelItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading equipment_level: {ex.Message}");
                _equipmentLevelData = new List<ZZZEquipmentLevelItem>(); // Ensure it's initialized
                throw new InvalidOperationException($"Failed to load essential equipment level data", ex);
            }
        }

        private async Task LoadWeaponLevel()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZWeaponLevelData>("weapon_level.json");
                _weaponLevelData = data?.Items ?? new List<ZZZWeaponLevelItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading weapon_level: {ex.Message}");
                _weaponLevelData = new List<ZZZWeaponLevelItem>();
                throw new InvalidOperationException($"Failed to load essential weapon level data", ex);
            }
        }

        private async Task LoadWeaponStar()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZWeaponStarData>("weapon_star.json");
                _weaponStarData = data?.Items ?? new List<ZZZWeaponStarItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading weapon_star: {ex.Message}");
                _weaponStarData = new List<ZZZWeaponStarItem>();
                throw new InvalidOperationException($"Failed to load essential weapon star data", ex);
            }
        }


        private async Task LoadLocalizations()
        {
            try
            {
                var localizationData = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json");
                if (localizationData.TryGetValue(Language, out var langMap))
                {
                    _localization = langMap;
                }
                else if (localizationData.TryGetValue("en", out var enMap))
                {
                    _localization = enMap;
                    Console.WriteLine($"[Assets] Language '{Language}' not found, falling back to English");
                }
                else
                {
                    _localization = localizationData.FirstOrDefault().Value;
                    Console.WriteLine($"[Assets] English language not found, using first available language");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading localizations: {ex.Message}");
                _localization = new Dictionary<string, string>();
            }
        }

        private async Task LoadAvatars()
        {
            _avatars.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZAvatarAssetInfo>>("avatars.json");
                foreach (var kvp in deserializedMap)
                {
                    _avatars[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading avatars: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential avatars data", ex);
            }
        }

        private async Task LoadWeapons()
        {
            _weapons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZWeaponAssetInfo>>("weapons.json");
                foreach (var kvp in deserializedMap)
                {
                    _weapons[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading weapons: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential weapons data", ex);
            }
        }

        private async Task LoadEquipments()
        {
            _equipmentItems.Clear();
            _equipmentSuits.Clear();
            try
            {
                var deserializedData = await FetchAndDeserializeAssetAsync<ZZZEquipmentData>("equipments.json");

                if (deserializedData.Items != null)
                {
                    foreach (var kvp in deserializedData.Items)
                    {
                        _equipmentItems[kvp.Key] = kvp.Value;
                    }
                }

                if (deserializedData.Suits != null)
                {
                    foreach (var kvp in deserializedData.Suits)
                    {
                        _equipmentSuits[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading equipments: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential equipments data", ex);
            }
        }

        private async Task LoadPfps()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZPfpAssetInfo>>("pfps.json");
                foreach (var kvp in deserializedMap)
                {
                    _pfps[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading profile pictures: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential profile pictures data", ex);
            }
        }

        private async Task LoadNamecards()
        {
            _namecards.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZNameCardAssetInfo>>("namecards.json");
                foreach (var kvp in deserializedMap)
                {
                    _namecards[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading namecards: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential namecards data", ex);
            }
        }

        private async Task LoadMedals()
        {
            _medals.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZMedalAssetInfo>>("medals.json");
                foreach (var kvp in deserializedMap)
                {
                    _medals[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading medals: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential medals data", ex);
            }
        }

        private async Task LoadTitles()
        {
            _titles.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZTitleAssetInfo>>("titles.json");
                foreach (var kvp in deserializedMap)
                {
                    _titles[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading titles: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential titles data", ex);
            }
        }

        private async Task LoadProperties()
        {
            _properties.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZPropertyAssetInfo>>("property.json");
                foreach (var kvp in deserializedMap)
                {
                    _properties[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Error loading properties: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential properties data", ex);
            }
        }

        public string GetLocalizedText(string key)
        {
            if (_localization != null && !string.IsNullOrEmpty(key) && _localization.TryGetValue(key, out var text))
            {
                return text;
            }
            return key;
        }

        public ZZZAvatarAssetInfo? GetAvatarInfo(string agentId)
        {
            if (_avatars.TryGetValue(agentId, out var avatarInfo))
            {
                return avatarInfo;
            }
            return null;
        }

        public ZZZWeaponAssetInfo? GetWeaponInfo(string weaponId)
        {
            if (_weapons.TryGetValue(weaponId, out var weaponInfo))
            {
                return weaponInfo;
            }
            return null;
        }

        public string GetAgentName(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo))
            {
                if (!string.IsNullOrEmpty(avatarInfo.Name))
                {
                    string localizedName = GetLocalizedText(avatarInfo.Name);
                    if (localizedName != avatarInfo.Name)
                    {
                        return localizedName;
                    }

                    if (avatarInfo.Name.Contains("_"))
                    {
                        string[] nameParts = avatarInfo.Name.Split('_');
                        if (nameParts.Length > 0)
                        {
                            return nameParts[nameParts.Length - 1];
                        }
                    }

                    return avatarInfo.Name;
                }
            }
            return $"Agent_{agentId}";
        }

        public string GetAgentIconUrl(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.Image))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{avatarInfo.Image}";
            }
            return string.Empty;
        }

        public string GetAgentCircleIconUrl(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.CircleIcon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{avatarInfo.CircleIcon}";
            }
            return string.Empty;
        }

        public List<ElementType> GetAgentElements(int agentId)
        {
            string agentIdStr = agentId.ToString();
            var elements = new List<ElementType>();

            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo) && avatarInfo.ElementTypes != null)
            {
                foreach (var element in avatarInfo.ElementTypes)
                {
                    elements.Add(MapElementNameToEnum(element));
                }
            }

            return elements;
        }

        public ProfessionType GetAgentProfessionType(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.ProfessionType))
            {
                return MapProfessionNameToEnum(avatarInfo.ProfessionType);
            }
            return ProfessionType.Unknown;
        }

        public int GetAgentRarity(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo))
            {
                return avatarInfo.Rarity;
            }
            return 0;
        }

        public ZZZEquipmentSuitInfo? GetDiscSetInfo(string suitId)
        {
            if (_equipmentSuits.TryGetValue(suitId, out var suitInfo))
            {
                return suitInfo;
            }
            return null;
        }

        public Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets()
        {
            return _equipmentSuits;
        }

        public string GetWeaponName(int weaponId)
        {
            string weaponIdStr = weaponId.ToString();
            if (_weapons.TryGetValue(weaponIdStr, out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ItemName))
            {
                return GetLocalizedText(weaponInfo.ItemName);
            }
            return $"Weapon_{weaponId}";
        }

        public string GetWeaponIconUrl(int weaponId)
        {
            string weaponIdStr = weaponId.ToString();
            if (_weapons.TryGetValue(weaponIdStr, out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ImagePath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{weaponInfo.ImagePath}";
            }
            return string.Empty;
        }

        public ProfessionType GetWeaponType(int weaponId)
        {
            string weaponIdStr = weaponId.ToString();
            if (_weapons.TryGetValue(weaponIdStr, out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ProfessionType))
            {
                return MapProfessionNameToEnum(weaponInfo.ProfessionType);
            }
            return ProfessionType.Unknown;
        }

        public int GetWeaponRarity(int weaponId)
        {
            string weaponIdStr = weaponId.ToString();
            if (_weapons.TryGetValue(weaponIdStr, out var weaponInfo))
            {
                return weaponInfo.Rarity;
            }
            return 0;
        }

        public string GetDriveDiscSuitName(int suitId)
        {
            string suitIdStr = suitId.ToString();
            if (_equipmentSuits.TryGetValue(suitIdStr, out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Name))
            {
                return GetLocalizedText(suitInfo.Name);
            }
            return $"Suit_{suitId}";
        }

        public string GetDriveDiscSuitIconUrl(int suitId)
        {
            string suitIdStr = suitId.ToString();
            if (_equipmentSuits.TryGetValue(suitIdStr, out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{suitInfo.Icon}";
            }
            return string.Empty;
        }

        public int GetDriveDiscRarity(int discId)
        {
            string discIdStr = discId.ToString();
            if (_equipmentItems.TryGetValue(discIdStr, out var discInfo))
            {
                return discInfo.Rarity;
            }
            return 0;
        }

        public int GetDriveDiscSuitId(int discId)
        {
            string discIdStr = discId.ToString();
            if (_equipmentItems.TryGetValue(discIdStr, out var discInfo))
            {
                return discInfo.SuitId;
            }
            return 0;
        }

        public string GetPropertyName(int propertyId)
        {
            string propertyIdStr = propertyId.ToString();
            if (_properties.TryGetValue(propertyIdStr, out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Name))
            {
                return GetLocalizedText(propertyInfo.Name);
            }
            return $"Property_{propertyId}";
        }

        public string FormatPropertyValue(int propertyId, double value)
        {
            string propertyIdStr = propertyId.ToString();
            if (_properties.TryGetValue(propertyIdStr, out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Format))
            {
                try
                {
                    if (propertyInfo.Format.Contains("%"))
                    {
                        return string.Format(System.Globalization.CultureInfo.InvariantCulture, propertyInfo.Format, value);
                    }
                    else
                    {
                        return string.Format(System.Globalization.CultureInfo.InvariantCulture, propertyInfo.Format, Math.Floor(value));
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"[Assets] Error formatting property {propertyId}: {ex.Message}");
                    bool isPercent = ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
                    return isPercent ? $"{value:F1}%" : $"{Math.Floor(value)}";
                }
            }

            bool isPercentage = ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
            return isPercentage ? $"{value:F1}%" : $"{Math.Floor(value)}";
        }

        public string GetTitleText(int titleId)
        {
            string titleIdStr = titleId.ToString();
            if (_titles.TryGetValue(titleIdStr, out var titleInfo) && !string.IsNullOrEmpty(titleInfo.TitleText))
            {
                return GetLocalizedText(titleInfo.TitleText);
            }
            return $"Title_{titleId}";
        }

        public string GetMedalName(int medalId)
        {
            string medalIdStr = medalId.ToString();
            if (_medals.TryGetValue(medalIdStr, out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Name))
            {
                return GetLocalizedText(medalInfo.Name);
            }
            return $"Medal_{medalId}";
        }

        public string GetMedalIconUrl(int medalId)
        {
            string medalIdStr = medalId.ToString();
            if (_medals.TryGetValue(medalIdStr, out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{medalInfo.Icon}";
            }
            return string.Empty;
        }

        public string GetNameCardIconUrl(int nameCardId)
        {
            string nameCardIdStr = nameCardId.ToString();
            if (_namecards.TryGetValue(nameCardIdStr, out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{nameCardInfo.Icon}";
            }
            return string.Empty;
        }

        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            string profilePictureIdStr = profilePictureId.ToString();
            if (_pfps.TryGetValue(profilePictureIdStr, out var pfpInfo) && !string.IsNullOrEmpty(pfpInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{pfpInfo.Icon}";
            }
            return string.Empty;
        }

        public string GetSkillIconUrl(int agentId, SkillType skillType)
        {
            return string.Empty;
        }

        public List<ZZZEquipmentLevelItem>? GetEquipmentLevelData() => _equipmentLevelData;
        public List<ZZZWeaponLevelItem>? GetWeaponLevelData() => _weaponLevelData;
        public List<ZZZWeaponStarItem>? GetWeaponStarData() => _weaponStarData;

        private ElementType MapElementNameToEnum(string elementName)
        {
            return elementName?.ToUpperInvariant() switch
            {
                "FIRE" => ElementType.Fire,
                "ICE" => ElementType.Ice,
                "FIREFROST" => ElementType.Ice,
                "ELEC" => ElementType.Electric,
                "ETHER" => ElementType.Ether,
                "PHYSICS" => ElementType.Physical,
                _ => ElementType.Unknown
            };
        }

        private ProfessionType MapProfessionNameToEnum(string professionName)
        {
            return professionName?.ToUpperInvariant() switch
            {
                "ATTACK" => ProfessionType.Attack,
                "STUN" => ProfessionType.Stun,
                "ANOMALY" => ProfessionType.Anomaly,
                "DEFENSE" => ProfessionType.Defense,
                "SUPPORT" => ProfessionType.Support,
                _ => ProfessionType.Unknown
            };
        }
    }
}