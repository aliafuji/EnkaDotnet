using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Utils.Common;

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        protected readonly string _baseAssetsPath;
        protected readonly string _gameSpecificPath;
        protected Dictionary<string, string>? _textMap;

        public GameType GameType { get; }
        public string Language { get; }
        public string AssetsPath => _gameSpecificPath;

        protected BaseAssets(string assetsBasePath, string language, GameType gameType)
        {
            _baseAssetsPath = assetsBasePath ?? throw new ArgumentNullException(nameof(assetsBasePath));
            Language = language;
            GameType = gameType;

            _gameSpecificPath = Path.Combine(_baseAssetsPath, gameType.ToString().ToLowerInvariant());

            bool assetsReady = EnsureAssetsDownloadedAsync().GetAwaiter().GetResult();

            if (!assetsReady)
            {
                throw new InvalidOperationException($"Failed to download or verify essential asset files in '{_gameSpecificPath}'. Cannot initialize Assets.");
            }

            LoadTextMap(language);
            LoadAssets();
        }

        protected abstract Dictionary<string, string> GetAssetUrls();
        protected abstract void LoadAssets();
        public string GetText(string? hash) => hash != null && _textMap != null && _textMap.TryGetValue(hash, out var text) ? text ?? string.Empty : hash ?? string.Empty;

        protected async Task<bool> EnsureAssetsDownloadedAsync()
        {
            var assetLinks = GetAssetUrls();
            if (assetLinks == null || assetLinks.Count == 0)
            {
                throw new UnsupportedGameTypeException(GameType, $"No asset links defined for game type {GameType}.");
            }

            try
            {
                Directory.CreateDirectory(_gameSpecificPath);

                foreach (var kvp in assetLinks)
                {
                    string relativePath = kvp.Key;
                    string url = kvp.Value;
                    string localFilePath = Path.Combine(_gameSpecificPath, relativePath);
                    string? directory = Path.GetDirectoryName(localFilePath);
                    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                    if (!File.Exists(localFilePath))
                    {
                        Console.WriteLine($"[Assets] Downloading '{relativePath}' for {GameType} from {url}...");
                        try
                        {
                            using var request = new HttpRequestMessage(HttpMethod.Get, url);
                            request.Headers.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                            HttpResponseMessage response = await _httpClient.SendAsync(request);

                            if (!response.IsSuccessStatusCode)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($" -> FAILED to download '{relativePath}': HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                                Console.ResetColor();
                                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) Console.WriteLine($" -> Check if the URL is correct: {url}");
                                return false;
                            }
                            string content = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($" -> WARNING: Downloaded content for '{relativePath}' is empty.");
                                Console.ResetColor();
                            }
                            await File.WriteAllTextAsync(localFilePath, content);
                            Console.WriteLine($" -> Saved to '{localFilePath}' (Size: {new FileInfo(localFilePath).Length} bytes)");
                        }
                        catch (HttpRequestException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($" -> FAILED to download '{relativePath}' (Network Error): {ex.Message}");
                            Console.ResetColor();
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($" -> FAILED to save '{relativePath}': {ex.Message}");
                            Console.ResetColor();
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Assets] Error preparing asset directory '{_gameSpecificPath}': {ex.Message}");
                Console.ResetColor();
                throw new DirectoryNotFoundException($"Failed to create or access asset directory '{_gameSpecificPath}'.", ex);
            }
        }

        protected void LoadTextMap(string language)
        {
            string filePath = Path.Combine(_gameSpecificPath, "text_map.json");
            _textMap = new Dictionary<string, string>();
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"TextMap file not found at the expected location.", filePath);
                }
                var jsonData = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    throw new InvalidOperationException("TextMap file is empty.");
                }

                var allLanguageMaps = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonData);

                if (allLanguageMaps == null)
                {
                    throw new InvalidOperationException("Failed to deserialize TextMap structure.");
                }

                if (allLanguageMaps.TryGetValue(language, out var languageSpecificMap))
                {
                    _textMap = languageSpecificMap;
                    Console.WriteLine($"[Assets] Loaded {_textMap.Count} entries for language '{language}'");
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
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"[Assets] Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to load essential TextMap", ex);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Assets] Error parsing TextMap JSON: {ex.Message} (Path: {ex.Path}, Line: {ex.LineNumber}, Pos: {ex.BytePositionInLine})");
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