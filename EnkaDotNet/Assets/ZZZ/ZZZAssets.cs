using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Assets.ZZZ
{
    public class ZZZAssets : BaseAssets, IZZZAssets, IDisposable
    {
        private readonly ConcurrentDictionary<string, ZZZAvatarAssetInfo> _avatars = new ConcurrentDictionary<string, ZZZAvatarAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZWeaponAssetInfo> _weapons = new ConcurrentDictionary<string, ZZZWeaponAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZPfpAssetInfo> _pfps = new ConcurrentDictionary<string, ZZZPfpAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZNameCardAssetInfo> _namecards = new ConcurrentDictionary<string, ZZZNameCardAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZTitleAssetInfo> _titles = new ConcurrentDictionary<string, ZZZTitleAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZMedalAssetInfo> _medals = new ConcurrentDictionary<string, ZZZMedalAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZPropertyAssetInfo> _properties = new ConcurrentDictionary<string, ZZZPropertyAssetInfo>();
        private readonly ConcurrentDictionary<string, ZZZEquipmentItemInfo> _equipmentItems = new ConcurrentDictionary<string, ZZZEquipmentItemInfo>();
        private readonly ConcurrentDictionary<string, ZZZEquipmentSuitInfo> _equipmentSuits = new ConcurrentDictionary<string, ZZZEquipmentSuitInfo>();
        private IReadOnlyList<ZZZEquipmentLevelItem> _equipmentLevelData;
        private IReadOnlyList<ZZZWeaponLevelItem> _weaponLevelData;
        private IReadOnlyList<ZZZWeaponStarItem> _weaponStarData;
        private readonly SemaphoreSlim _loadingSemaphore;
        private bool _disposed = false;

        public ZZZAssets(string language, HttpClient httpClient, ILogger<ZZZAssets> logger)
            : base(language, "zzz", httpClient, logger)
        {
            int maxConcurrency = Meth.Clamp(Environment.ProcessorCount, 1, 8);
            _loadingSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        protected override IReadOnlyDictionary<string, string> GetAssetFileUrls()
        {
            return Constants.ZZZAssetFileUrls;
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
                LoadWithSemaphore(LoadAvatars),
                LoadWithSemaphore(LoadWeapons),
                LoadWithSemaphore(LoadEquipments),
                LoadWithSemaphore(LoadPfps),
                LoadWithSemaphore(LoadNamecards),
                LoadWithSemaphore(LoadMedals),
                LoadWithSemaphore(LoadTitles),
                LoadWithSemaphore(LoadProperties),
                LoadWithSemaphore(LoadEquipmentLevel),
                LoadWithSemaphore(LoadWeaponLevel),
                LoadWithSemaphore(LoadWeaponStar)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task LoadWithSemaphore(Func<Task> loadFunction)
        {
            await _loadingSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var loadTask = loadFunction();
                var timeout = TimeSpan.FromMinutes(5);
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(loadTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Asset loading operation '{loadFunction.Method.Name}' timed out after {timeout.TotalMinutes} minutes.");
                }

                await loadTask.ConfigureAwait(false);
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        private async Task LoadEquipmentLevel()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<ZZZEquipmentLevelData>("equipment_level.json").ConfigureAwait(false);
                _equipmentLevelData = data?.Items ?? new List<ZZZEquipmentLevelItem>();
                if (_equipmentLevelData == null || _equipmentLevelData.Count == 0)
                {
                    throw new InvalidOperationException("ZZZ equipment_level.json data is null or empty after deserialization.");
                }
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
                if (_weaponLevelData == null || _weaponLevelData.Count == 0)
                {
                    throw new InvalidOperationException("ZZZ weapon_level.json data is null or empty after deserialization.");
                }
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
                if (_weaponStarData == null || _weaponStarData.Count == 0)
                {
                    throw new InvalidOperationException("ZZZ weapon_star.json data is null or empty after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _avatars[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ avatars.json data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _weapons[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ weapons.json data is null after deserialization.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Zenless Zone Zero weapons.json asset.");
                throw new InvalidOperationException("Failed to load essential Zenless Zone Zero weapon data.", ex);
            }
        }

        private async Task LoadEquipments()
        {
            _equipmentItems.Clear();
            _equipmentSuits.Clear();
            try
            {
                var deserializedData = await FetchAndDeserializeAssetAsync<ZZZEquipmentData>("equipments.json").ConfigureAwait(false);
                if (deserializedData?.Items != null)
                {
                    foreach (var kvp in deserializedData.Items) _equipmentItems[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ equipments.json Items data is null after deserialization.");
                }

                if (deserializedData?.Suits != null)
                {
                    foreach (var kvp in deserializedData.Suits) _equipmentSuits[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ equipments.json Suits data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _pfps[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ pfps.json data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _namecards[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ namecards.json data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _medals[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ medals.json data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _titles[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ titles.json data is null after deserialization.");
                }
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
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _properties[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new InvalidOperationException("ZZZ property.json data is null after deserialization.");
                }
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
        public IReadOnlyList<ZZZAvatarColors> GetAvatarColors(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && avatarInfo.Colors != null)
            {
                return new List<ZZZAvatarColors> { avatarInfo.Colors };
            }
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
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{avatarInfo.Image}";
            }
            return string.Empty;
        }
        public string GetAgentCircleIconUrl(int agentId)
        {
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.CircleIcon))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{avatarInfo.CircleIcon}";
            }
            return string.Empty;
        }
        public IReadOnlyList<ElementType> GetAgentElements(int agentId)
        {
            var elements = new List<ElementType>();
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && avatarInfo.ElementTypes != null)
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
            if (_avatars.TryGetValue(agentId.ToString(), out var avatarInfo) && !string.IsNullOrEmpty(avatarInfo.ProfessionType))
            {
                return MapProfessionNameToEnum(avatarInfo.ProfessionType);
            }
            return ProfessionType.Unknown;
        }
        public int GetAgentRarity(int agentId) { _avatars.TryGetValue(agentId.ToString(), out var info); return info?.Rarity ?? 0; }
        public ZZZEquipmentSuitInfo GetDiscSetInfo(string suitId) { _equipmentSuits.TryGetValue(suitId, out var info); return info; }
        public Dictionary<string, ZZZEquipmentSuitInfo> GetAllDiscSets() => new Dictionary<string, ZZZEquipmentSuitInfo>(_equipmentSuits);
        public string GetWeaponName(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ItemName))
            {
                return GetText(weaponInfo.ItemName);
            }
            return $"Weapon_{weaponId}";
        }
        public string GetWeaponIconUrl(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ImagePath))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{weaponInfo.ImagePath}";
            }
            return string.Empty;
        }
        public ProfessionType GetWeaponType(int weaponId)
        {
            if (_weapons.TryGetValue(weaponId.ToString(), out var weaponInfo) && !string.IsNullOrEmpty(weaponInfo.ProfessionType))
            {
                return MapProfessionNameToEnum(weaponInfo.ProfessionType);
            }
            return ProfessionType.Unknown;
        }
        public int GetWeaponRarity(int weaponId) { _weapons.TryGetValue(weaponId.ToString(), out var info); return info?.Rarity ?? 0; }
        public string GetDriveDiscSuitName(int suitId)
        {
            if (_equipmentSuits.TryGetValue(suitId.ToString(), out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Name))
            {
                return GetText(suitInfo.Name);
            }
            return $"Suit_{suitId}";
        }
        public string GetDriveDiscSuitIconUrl(int suitId)
        {
            if (_equipmentSuits.TryGetValue(suitId.ToString(), out var suitInfo) && !string.IsNullOrEmpty(suitInfo.Icon))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{suitInfo.Icon}";
            }
            return string.Empty;
        }
        public int GetDriveDiscRarity(int discId) { _equipmentItems.TryGetValue(discId.ToString(), out var info); return info?.Rarity ?? 0; }
        public int GetDriveDiscSuitId(int discId) { _equipmentItems.TryGetValue(discId.ToString(), out var info); return info?.SuitId ?? 0; }
        public string GetPropertyName(int propertyId)
        {
            if (_properties.TryGetValue(propertyId.ToString(), out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Name))
            {
                return GetText(propertyInfo.Name);
            }
            return $"Property_{propertyId}";
        }
        public string FormatPropertyValue(int propertyId, double value)
        {
            if (_properties.TryGetValue(propertyId.ToString(), out var propertyInfo) && !string.IsNullOrEmpty(propertyInfo.Format))
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
                    _logger.LogWarning(ex, "Error formatting ZZZ property {PropertyId}", propertyId);
                    bool isPercent = EnkaDotNet.Utils.ZZZ.ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
                    return isPercent ? $"{value:F1}%" : $"{Math.Floor(value)}";
                }
            }
            bool isPercentage = EnkaDotNet.Utils.ZZZ.ZZZStatsHelpers.IsDisplayPercentageStat((StatType)propertyId);
            return isPercentage ? $"{value:F1}%" : $"{Math.Floor(value)}";
        }
        public string GetTitleText(int titleId)
        {
            if (_titles.TryGetValue(titleId.ToString(), out var titleInfo) && !string.IsNullOrEmpty(titleInfo.TitleText))
            {
                return GetText(titleInfo.TitleText);
            }
            return $"Title_{titleId}";
        }
        public string GetMedalName(int medalId)
        {
            if (_medals.TryGetValue(medalId.ToString(), out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Name))
            {
                return GetText(medalInfo.Name);
            }
            return $"Medal_{medalId}";
        }
        public string GetMedalIconUrl(int medalId)
        {
            if (_medals.TryGetValue(medalId.ToString(), out var medalInfo) && !string.IsNullOrEmpty(medalInfo.Icon))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{medalInfo.Icon}";
            }
            return string.Empty;
        }
        public string GetNameCardIconUrl(int nameCardId)
        {
            if (_namecards.TryGetValue(nameCardId.ToString(), out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{nameCardInfo.Icon}";
            }
            return string.Empty;
        }
        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            if (_pfps.TryGetValue(profilePictureId.ToString(), out var pfpInfo) && !string.IsNullOrEmpty(pfpInfo.Icon))
            {
                return $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{pfpInfo.Icon}";
            }
            return string.Empty;
        }
        public string GetSkillIconUrl(int agentId, SkillType skillType) => string.Empty;
        public IReadOnlyList<ZZZEquipmentLevelItem> GetEquipmentLevelData() => _equipmentLevelData;
        public IReadOnlyList<ZZZWeaponLevelItem> GetWeaponLevelData() => _weaponLevelData;
        public IReadOnlyList<ZZZWeaponStarItem> GetWeaponStarData() => _weaponStarData;
        public IReadOnlyDictionary<string, Skin> GetAgentSkins(string agentId)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                _logger?.LogWarning("Invalid agent ID provided: {AgentId}", agentId);
                return new Dictionary<string, Skin>();
            }

            _logger?.LogDebug("Fetching skins for agent ID: {AgentId}", agentId);

            if (!_avatars.TryGetValue(agentId, out var avatarInfo))
            {
                _logger?.LogInformation("No avatar found for agent ID: {AgentId}", agentId);
                return new Dictionary<string, Skin>();
            }

            if (avatarInfo.Skins == null || avatarInfo.Skins.Count == 0)
            {
                _logger?.LogInformation("No skins available for agent ID: {AgentId}", agentId);
                return new Dictionary<string, Skin>();
            }

            _logger?.LogDebug("Found {SkinCount} skins for agent ID: {AgentId}",
                avatarInfo.Skins.Count, agentId);

            try
            {
                var result = new Dictionary<string, Skin>();
                foreach (var skinEntry in avatarInfo.Skins)
                {
                    result[skinEntry.Key] = new Skin
                    {
                        Image = $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{skinEntry.Value.Image}",
                        CircleIcon = $"{Constants.DEFAULT_ZZZ_ASSET_CDN_URL}{skinEntry.Value.CircleIcon}"
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing skins for agent ID: {AgentId}", agentId);
                return new Dictionary<string, Skin>();
            }
        }

        public Skin GetAgentSkin(string agentId, string skinId)
        {
            if (string.IsNullOrWhiteSpace(skinId))
            {
                _logger?.LogWarning("Invalid skin ID provided for agent {AgentId}", agentId);
                return null;
            }

            var skins = GetAgentSkins(agentId);
            return skins.TryGetValue(skinId, out var skin) ? skin : null;
        }

        private ElementType MapElementNameToEnum(string elementName)
        {
            switch (elementName?.ToUpperInvariant())
            {
                case "FIRE": return ElementType.Fire;
                case "ICE": return ElementType.Ice;
                case "FIREFROST": return ElementType.FireFrost;
                case "ELEC": return ElementType.Electric;
                case "ETHER": return ElementType.Ether;
                case "PHYSICS": return ElementType.Physical;
                case "AURICETHER": return ElementType.AuricEther;
                default: return ElementType.Unknown;
            }
        }

        private ProfessionType MapProfessionNameToEnum(string professionName)
        {
            switch (professionName?.ToUpperInvariant())
            {
                case "RUPTURE": return ProfessionType.Rupture;
                case "ATTACK": return ProfessionType.Attack;
                case "STUN": return ProfessionType.Stun;
                case "ANOMALY": return ProfessionType.Anomaly;
                case "DEFENSE": return ProfessionType.Defense;
                case "SUPPORT": return ProfessionType.Support;
                default: return ProfessionType.Unknown;
            }
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _loadingSemaphore.Dispose();
            }

            _disposed = true;
        }
    }
}
