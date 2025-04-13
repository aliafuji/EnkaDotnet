using System.Text.Json;
using System.Text.Json.Serialization;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils;

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        protected readonly Dictionary<string, object> _assetCache = new Dictionary<string, object>();
        protected Dictionary<string, string>? _textMap;

        public GameType GameType { get; }
        public string Language { get; }

        protected BaseAssets(string language, GameType gameType)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            GameType = gameType;

            LoadTextMap(language).GetAwaiter().GetResult();
            LoadAssets().GetAwaiter().GetResult();
        }

        protected abstract Dictionary<string, string> GetAssetUrls();
        protected abstract Task LoadAssets();

        public string GetText(string? hash) => hash != null && _textMap != null && _textMap.TryGetValue(hash, out var text) ? text ?? string.Empty : hash ?? string.Empty;

        protected async Task<string> FetchAssetAsync(string assetKey)
        {
            var assetUrls = GetAssetUrls();
            if (!assetUrls.TryGetValue(assetKey, out var url))
            {
                throw new InvalidOperationException($"No URL defined for asset '{assetKey}' in game type {GameType}.");
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd(Constants.DefaultUserAgent);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch '{assetKey}': HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                }

                string content = await response.Content.ReadAsStringAsync();

                return content;
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Assets] FAILED to fetch '{assetKey}' (Network Error): {ex.Message}");
                Console.ResetColor();
                throw;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Assets] Unexpected error fetching '{assetKey}': {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        protected async Task<T> FetchAndDeserializeAssetAsync<T>(string assetKey)
        {
            if (_assetCache.TryGetValue(assetKey, out var cachedAsset) && cachedAsset is T typedAsset)
            {
                return typedAsset;
            }

            string jsonContent = await FetchAssetAsync(assetKey);

            try
            {
                var options = GetJsonOptions();
                var result = JsonSerializer.Deserialize<T>(jsonContent, options);

                if (result == null)
                {
                    throw new JsonException($"Failed to deserialize {assetKey} - result was null");
                }

                _assetCache[assetKey] = result;

                return result;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing {assetKey} JSON: {ex.Message}");
                throw;
            }
        }

        protected async Task LoadTextMap(string language)
        {
            try
            {
                _textMap = new Dictionary<string, string>();

                var allLanguageMaps = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json");

                if (allLanguageMaps.TryGetValue(language, out var languageSpecificMap))
                {
                    _textMap = languageSpecificMap;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Assets] Error: Language code '{language}' not found in the TextMap file.");
                    Console.ResetColor();
                    Console.WriteLine($"[Assets] Available languages in file: {string.Join(", ", allLanguageMaps.Keys)}");

                    if (language != "en" && allLanguageMaps.TryGetValue("en", out var englishMap))
                    {
                        Console.WriteLine($"[Assets] Falling back to English language.");
                        _textMap = englishMap;
                    }
                    else
                    {
                        var firstLang = allLanguageMaps.Keys.FirstOrDefault();
                        if (firstLang != null && allLanguageMaps.TryGetValue(firstLang, out var firstLangMap))
                        {
                            Console.WriteLine($"[Assets] Falling back to '{firstLang}' language.");
                            _textMap = firstLangMap;
                        }
                        else
                        {
                            throw new InvalidOperationException($"No languages found in the TextMap data.");
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[Assets] Error fetching TextMap: {ex.Message}");
                throw new InvalidOperationException($"Failed to fetch essential TextMap", ex);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing TextMap JSON: {ex.Message}");
                throw new InvalidOperationException($"Failed to parse essential TextMap JSON structure", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Unexpected error loading TextMap: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential TextMap", ex);
            }
        }

        protected JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new JsonStringOrNumberConverter() },
                PropertyNameCaseInsensitive = true
            };
        }
    }
}