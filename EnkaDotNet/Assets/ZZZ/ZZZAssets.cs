using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.ZZZ;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Assets.ZZZ
{
    public class ZZZAssets : BaseAssets, IZZZAssets
    {
        private readonly Dictionary<string, ZZZAvatarAssetInfo> _avatars = new Dictionary<string, ZZZAvatarAssetInfo>();
        private readonly Dictionary<string, ZZZWeaponAssetInfo> _weapons = new Dictionary<string, ZZZWeaponAssetInfo>();
        private readonly Dictionary<string, ZZZPfpAssetInfo> _pfps = new Dictionary<string, ZZZPfpAssetInfo>();
        private readonly Dictionary<string, ZZZNameCardAssetInfo> _namecards = new Dictionary<string, ZZZNameCardAssetInfo>();
        private readonly Dictionary<string, ZZZTitleAssetInfo> _titles = new Dictionary<string, ZZZTitleAssetInfo>();
        private readonly Dictionary<string, ZZZMedalAssetInfo> _medals = new Dictionary<string, ZZZMedalAssetInfo>();
        private readonly Dictionary<string, ZZZPropertyAssetInfo> _properties = new Dictionary<string, ZZZPropertyAssetInfo>();
        private readonly Dictionary<string, ZZZEquipmentItemInfo> _equipmentItems = new Dictionary<string, ZZZEquipmentItemInfo>();
        private readonly Dictionary<string, ZZZEquipmentSuitInfo> _equipmentSuits = new Dictionary<string, ZZZEquipmentSuitInfo>();

        private List<ZZZEquipmentLevelItem> _equipmentLevelData;
        private List<ZZZWeaponLevelItem> _weaponLevelData;
        private List<ZZZWeaponStarItem> _weaponStarData;

        public ZZZAssets(string language, HttpClient httpClient, ILogger<ZZZAssets> logger)
            : base(language, GameType.ZZZ, httpClient, logger)
        {
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
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
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task LoadEquipmentLevel()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZEquipmentLevelData>("equipment_level.json").ConfigureAwait(false);
                _equipmentLevelData = data?.Items ?? new List<ZZZEquipmentLevelItem>();
                if (_equipmentLevelData == null || !_equipmentLevelData.Any())
                    throw new InvalidOperationException("ZZZ equipment_level.json data is null or empty after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero equipment_level.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero equipment level data.", ex);
            }
        }

        private async Task LoadWeaponLevel()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZWeaponLevelData>("weapon_level.json").ConfigureAwait(false);
                _weaponLevelData = data?.Items ?? new List<ZZZWeaponLevelItem>();
                if (_weaponLevelData == null || !_weaponLevelData.Any())
                    throw new InvalidOperationException("ZZZ weapon_level.json data is null or empty after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero weapon_level.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero weapon level data.", ex);
            }
        }

        private async Task LoadWeaponStar()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZWeaponStarData>("weapon_star.json").ConfigureAwait(false);
                _weaponStarData = data?.Items ?? new List<ZZZWeaponStarItem>();
                if (_weaponStarData == null || !_weaponStarData.Any())
                    throw new InvalidOperationException("ZZZ weapon_star.json data is null or empty after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero weapon_star.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero weapon star data.", ex);
            }
        }

        private async Task LoadAvatars()
        {
            _avatars.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZAvatarAssetInfo>>("avatars.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _avatars[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ avatars.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero avatars.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero avatar data.", ex);
            }
        }

        private async Task LoadWeapons()
        {
            _weapons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZWeaponAssetInfo>>("weapons.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _weapons[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ weapons.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero weapons.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero weapon data.", ex);
            }
        }

        private async Task LoadEquipments()
        {
            _equipmentItems.Clear(); _equipmentSuits.Clear();
            try
            {
                var deserializedData = await FetchAndDeserializeAssetAsync<ZZZEquipmentData>("equipments.json").ConfigureAwait(false);
                if (deserializedData?.Items != null) foreach (var kvp in deserializedData.Items) _equipmentItems[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ equipments.json Items data is null after deserialization.");
                if (deserializedData?.Suits != null) foreach (var kvp in deserializedData.Suits) _equipmentSuits[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ equipments.json Suits data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero equipments.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero equipment data.", ex);
            }
        }

        private async Task LoadPfps()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZPfpAssetInfo>>("pfps.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _pfps[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ pfps.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero pfps.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero profile picture data.", ex);
            }
        }

        private async Task LoadNamecards()
        {
            _namecards.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZNameCardAssetInfo>>("namecards.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _namecards[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ namecards.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero namecards.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero namecard data.", ex);
            }
        }

        private async Task LoadMedals()
        {
            _medals.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZMedalAssetInfo>>("medals.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _medals[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ medals.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero medals.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero medal data.", ex);
            }
        }

        private async Task LoadTitles()
        {
            _titles.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZTitleAssetInfo>>("titles.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _titles[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ titles.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero titles.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero title data.", ex);
            }
        }

        private async Task LoadProperties()
        {
            _properties.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, ZZZPropertyAssetInfo>>("property.json").ConfigureAwait(false);
                if (deserializedMap != null) foreach (var kvp in deserializedMap) _properties[kvp.Key] = kvp.Value;
                else throw new InvalidOperationException("ZZZ property.json data is null after deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero property.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero property data.", ex);
            }
        }
        public string GetLocalizedText(string key) => GetText(key);
        public ZZZAvatarAssetInfo GetAvatarInfo(string agentId) { _avatars.TryGetValue(agentId, out var info); return info; }
        public ZZZWeaponAssetInfo GetWeaponInfo(string weaponId) { _weapons.TryGetValue(weaponId, out var info); return info; }

        public List<ZZZAvatarColors> GetAvatarColors(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && avatarInfo.Colors != null)
                return new List<ZZZAvatarColors> { avatarInfo.Colors };
            return new List<ZZZAvatarColors>();
        }
        public string GetAgentName(int agentId)
        {
            string agentIdStr = agentId.ToString();
            if (_avatars.TryGetValue(agentIdStr, out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.Name))
            {
                string localizedName = GetText(avatarInfo.Name);
                if (localizedName != avatarInfo.Name) return localizedName;
                if (avatarInfo.Name.Contains("_"))
                {
                    string[] nameParts = avatarInfo.Name.Split('_');
                    if (nameParts.Length > 0) return nameParts[nameParts.Length - 1];
                }
                return avatarInfo.Name;
            }
            return $"Agent_{agentId}";
        }
        public string GetAgentIconUrl(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.Image))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{avatarInfo.Image}";
            return string.Empty;
        }
        public string GetAgentCircleIconUrl(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.CircleIcon))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{avatarInfo.CircleIcon}";
            return string.Empty;
        }
        public List<ElementType> GetAgentElements(int agentId)
        {
            var elements = new List<ElementType>();
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && avatarInfo.ElementTypes != null)
            {
                foreach (var element in avatarInfo.ElementTypes) elements.Add(MapElementNameToEnum(element));
            }
            return elements;
        }
        public ProfessionType GetAgentProfessionType(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.ProfessionType))
                return MapProfessionNameToEnum(avatarInfo.ProfessionType);
            return ProfessionType.Unknown;
        }
        public int GetAgentRarity(int agentId) { _avatars.TryGetValue(agentId.ToString(), out var info); return info?.Rarity ?? 0; }
        public ZZZEquipmentSuitInfo GetDiscSetInfo(string suitId) { _equipmentSuits.TryGetValue(suitId, out var info); return info; }
        public Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets() => _equipmentSuits;
        public string GetWeaponName(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ItemName))
                return GetText(weaponInfo.ItemName);
            return $"Weapon_{weaponId}";
        }
        public string GetWeaponIconUrl(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ImagePath))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{weaponInfo.ImagePath}";
            return string.Empty;
        }
        public ProfessionType GetWeaponType(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ProfessionType))
                return MapProfessionNameToEnum(weaponInfo.ProfessionType);
            return ProfessionType.Unknown;
        }
        public int GetWeaponRarity(int weaponId) { _weapons.TryGetValue(weaponId.ToString(), out var info); return info?.Rarity ?? 0; }
        public string GetDriveDiscSuitName(int suitId)
        {
            if (_equipmentSuits.TryGetValue(suitId.ToString(), out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Name))
                return GetText(suitInfo.Name);
            return $"Suit_{suitId}";
        }
        public string GetDriveDiscSuitIconUrl(int suitId)
        {
            if (_equipmentSuits.TryGetValue(suitId.ToString(), out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Icon))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{suitInfo.Icon}";
            return string.Empty;
        }
        public int GetDriveDiscRarity(int discId) { _equipmentItems.TryGetValue(discId.ToString(), out var info); return info?.Rarity ?? 0; }
        public int GetDriveDiscSuitId(int discId) { _equipmentItems.TryGetValue(discId.ToString(), out var info); return info?.SuitId ?? 0; }
        public string GetPropertyName(int propertyId)
        {
            if (_properties.TryGetValue(propertyId.ToString(), out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Name))
                return GetText(propertyInfo.Name);
            return $"Property_{propertyId}";
        }
        public string FormatPropertyValue(int propertyId, double value)
        {
            if (_properties.TryGetValue(propertyId.ToString(), out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Format))
            {
                try
                {
                    if (propertyInfo.Format.Contains("%"))
                        return string.Format(System.Globalization.CultureInfo.InvariantCulture, propertyInfo.Format, value);
                    else
                        return string.Format(System.Globalization.CultureInfo.InvariantCulture, propertyInfo.Format, Math.Floor(value));
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Error formatting ZZZ property {PropertyId}.", propertyId);
                    bool isPercent = ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
                    return isPercent ? $"{value:F1}%" : $"{Math.Floor(value)}";
                }
            }
            bool isPercentage = ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
            return isPercentage ? $"{value:F1}%" : $"{Math.Floor(value)}";
        }
        public string GetTitleText(int titleId)
        {
            if (_titles.TryGetValue(titleId.ToString(), out var titleInfo) && !string.IsNullOrEmpty(titleInfo.TitleText))
                return GetText(titleInfo.TitleText);
            return $"Title_{titleId}";
        }
        public string GetMedalName(int medalId)
        {
            if (_medals.TryGetValue(medalId.ToString(), out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Name))
                return GetText(medalInfo.Name);
            return $"Medal_{medalId}";
        }
        public string GetMedalIconUrl(int medalId)
        {
            if (_medals.TryGetValue(medalId.ToString(), out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Icon))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{medalInfo.Icon}";
            return string.Empty;
        }
        public string GetNameCardIconUrl(int nameCardId)
        {
            if (_namecards.TryGetValue(nameCardId.ToString(), out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{nameCardInfo.Icon}";
            return string.Empty;
        }
        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            if (_pfps.TryGetValue(profilePictureId.ToString(), out var pfpInfo) && !string.IsNullOrEmpty(pfpInfo.Icon))
                return $"{Constants.GetAssetCdnBaseUrl(GameType)}{pfpInfo.Icon}";
            return string.Empty;
        }
        public string GetSkillIconUrl(int agentId, SkillType skillType) => string.Empty;
        public List<ZZZEquipmentLevelItem> GetEquipmentLevelData() => _equipmentLevelData;
        public List<ZZZWeaponLevelItem> GetWeaponLevelData() => _weaponLevelData;
        public List<ZZZWeaponStarItem> GetWeaponStarData() => _weaponStarData;

        private ElementType MapElementNameToEnum(string elementName)
        {
            switch (elementName?.ToUpperInvariant())
            {
                case "FIRE": return ElementType.Fire;
                case "ICE": case "FIREFROST": return ElementType.Ice;
                case "ELEC": return ElementType.Electric;
                case "ETHER": return ElementType.Ether;
                case "PHYSICS": return ElementType.Physical;
                default: return ElementType.Unknown;
            }
        }
        private ProfessionType MapProfessionNameToEnum(string professionName)
        {
            switch (professionName?.ToUpperInvariant())
            {
                case "ATTACK": return ProfessionType.Attack;
                case "STUN": return ProfessionType.Stun;
                case "ANOMALY": return ProfessionType.Anomaly;
                case "DEFENSE": return ProfessionType.Defense;
                case "SUPPORT": return ProfessionType.Support;
                default: return ProfessionType.Unknown;
            }
        }
    }
}