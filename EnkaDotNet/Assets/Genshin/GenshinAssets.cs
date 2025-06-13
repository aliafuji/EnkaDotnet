using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin.Models;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Logging;
using EnkaDotNet.Utils.Common;

namespace EnkaDotNet.Assets.Genshin
{
    public class GenshinAssets : BaseAssets, IGenshinAssets, IDisposable
    {
        private readonly ConcurrentDictionary<int, CharacterAssetInfo> _characters = new ConcurrentDictionary<int, CharacterAssetInfo>();
        private readonly ConcurrentDictionary<int, TalentAssetInfo> _talents = new ConcurrentDictionary<int, TalentAssetInfo>();
        private readonly ConcurrentDictionary<string, ConstellationAssetInfo> _constellations = new ConcurrentDictionary<string, ConstellationAssetInfo>();
        private readonly ConcurrentDictionary<string, NameCardAssetInfo> _namecards = new ConcurrentDictionary<string, NameCardAssetInfo>();
        private readonly ConcurrentDictionary<string, PfpAssetInfo> _pfps = new ConcurrentDictionary<string, PfpAssetInfo>();

        private readonly SemaphoreSlim _loadingSemaphore;
        private bool _disposed = false;

        public GenshinAssets(string language, HttpClient httpClient, ILogger<GenshinAssets> logger)
            : base(language, "genshin", httpClient, logger)
        {
            int maxConcurrency = Meth.Clamp(Environment.ProcessorCount, 1, 8);
            _loadingSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        protected override IReadOnlyDictionary<string, string> GetAssetFileUrls()
        {
            return Constants.GenshinAssetFileUrls;
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
                LoadWithSemaphore(LoadCharacters),
                LoadWithSemaphore(LoadTalents),
                LoadWithSemaphore(LoadConstellations),
                LoadWithSemaphore(LoadNamecards),
                LoadWithSemaphore(LoadPfps)
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

        private string GetStringFromHashJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) return element.GetString();
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
                        {
                            _characters[charId] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact characters.json asset");
                throw new InvalidOperationException("Failed to load essential Genshin Impact character data", ex);
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
                _logger.LogError(ex, "Error loading Genshin Impact pfps.json asset");
                throw new InvalidOperationException("Failed to load essential Genshin Impact profile picture data", ex);
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
                        if (int.TryParse(kvp.Key, out int talentId))
                        {
                            _talents[talentId] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact talents.json asset");
                throw new InvalidOperationException("Failed to load essential Genshin Impact talent data", ex);
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
                        if (kvp.Value != null)
                        {
                            if (kvp.Value.NameTextMapHash.ValueKind != JsonValueKind.Undefined && kvp.Value.NameTextMapHash.ValueKind != JsonValueKind.Null)
                            {
                                _constellations[kvp.Key] = kvp.Value;
                            }
                            else
                            {
                                _constellations[kvp.Key] = kvp.Value;
                                _logger.LogWarning("Constellation '{Key}' has a null or undefined NameTextMapHash", kvp.Key);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Genshin Impact consts.json asset");
                throw new InvalidOperationException("Failed to load essential Genshin Impact constellation data", ex);
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
                _logger.LogError(ex, "Error loading Genshin Impact namecards.json asset");
                throw new InvalidOperationException("Failed to load essential Genshin Impact namecard data", ex);
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
                    return $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{iconName}.png";
                }
            }
            return string.Empty;
        }

        public ElementType GetCharacterElement(int characterId) => _characters.TryGetValue(characterId, out var charInfo) && charInfo.Element != null ? MapElementNameToEnum(charInfo.Element) : ElementType.Unknown;
        public string GetWeaponNameFromHash(string nameHash) => GetText(nameHash);
        public string GetWeaponIconUrlFromIconName(string iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{iconName}.png" : string.Empty;
        public string GetArtifactNameFromHash(string nameHash) => GetText(nameHash);
        public string GetArtifactSetNameFromHash(string setNameHash) => GetText(setNameHash);
        public string GetArtifactIconUrlFromIconName(string iconName) => !string.IsNullOrEmpty(iconName) ? $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{iconName}.png" : string.Empty;

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
                    return $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{adjustedIconPath}.png";
                }
            }
            return GetCharacterIconUrl(characterId);
        }

        public string GetNameCardIconUrl(int nameCardId) => _namecards.TryGetValue(nameCardId.ToString(), out var nameCardInfo) && !string.IsNullOrEmpty(nameCardInfo.Icon) ? $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{nameCardInfo.Icon}.png" : string.Empty;

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
            if (_talents.TryGetValue(talentId, out var talentInfo) && talentInfo != null && !string.IsNullOrEmpty(talentInfo.Icon))
            {
                return $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{talentInfo.Icon}.png";
            }
            return string.Empty;
        }

        public string GetConstellationIconUrl(int constellationId) => _constellations.TryGetValue(constellationId.ToString(), out var constellationInfo) && !string.IsNullOrEmpty(constellationInfo.Icon) ? $"{Constants.DEFAULT_GENSHIN_ASSET_CDN_URL}{constellationInfo.Icon}.png" : string.Empty;

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
