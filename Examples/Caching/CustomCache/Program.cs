using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CustomCacheExample
{
    /// <summary>
    /// Example demonstrating custom cache provider implementation with EnkaDotNet.
    /// Implement IEnkaCache interface to create your own cache provider.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Create a custom cache provider instance
                var customCache = new SimpleFileCache("cache_data");

                // Configure EnkaClient with custom cache
                var options = new EnkaClientOptions
                {
                    UserAgent = "CustomCacheExample/1.0",
                    
                    // Use custom cache provider
                    CacheProvider = CacheProvider.Custom
                };

                var services = new ServiceCollection();
                services.AddLogging(builder =>
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information)
                );
                services.AddMemoryCache();

                // Register the custom cache provider for dependency injection
                services.AddSingleton<IEnkaCache>(customCache);

                var provider = services.BuildServiceProvider();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var memoryCache = provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                var enkaCache = provider.GetService<IEnkaCache>();

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                Console.WriteLine("=== Custom Cache Provider Example ===\n");
                Console.WriteLine($"Cache directory: cache_data/");
                Console.WriteLine("Using SimpleFileCache - a custom file-based cache implementation\n");

                // Create the client with custom cache
                IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, memoryCache, enkaCache);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 80000000;

                // First request - will fetch from API and cache
                Console.WriteLine($"First request for UID {uid} (fetching from API)...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var (playerInfo, characters) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"  Characters: {characters.Count}\n");

                // Check cache statistics
                var stats = await customCache.GetStatsAsync();
                Console.WriteLine($"Cache stats after first request:");
                Console.WriteLine($"  Hits: {stats.HitCount}");
                Console.WriteLine($"  Misses: {stats.MissCount}");
                Console.WriteLine($"  Entries: {stats.EntryCount}\n");

                // Second request - should be served from cache
                Console.WriteLine($"Second request for UID {uid} (should be from cache)...");
                stopwatch.Restart();
                var (playerInfo2, characters2) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo2.Nickname} (Lv.{playerInfo2.Level})");
                Console.WriteLine($"  Characters: {characters2.Count}\n");

                // Check cache statistics again
                stats = await customCache.GetStatsAsync();
                Console.WriteLine($"Cache stats after second request:");
                Console.WriteLine($"  Hits: {stats.HitCount}");
                Console.WriteLine($"  Misses: {stats.MissCount}");
                Console.WriteLine($"  Hit rate: {stats.HitRate:P1}\n");

                Console.WriteLine("Note: You can implement IEnkaCache to create any custom cache provider.");
                Console.WriteLine("Examples: MongoDB, PostgreSQL, Azure Blob Storage, AWS S3, etc.");
            }
            catch (CacheException ex)
            {
                Console.WriteLine($"Cache error: {ex.Message}");
            }
            catch (PlayerNotFoundException ex)
            {
                Console.WriteLine($"Error: Player with UID {ex.Uid} not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// A simple file-based cache implementation demonstrating how to implement IEnkaCache.
    /// Each cache entry is stored as a separate JSON file.
    /// </summary>
    public class SimpleFileCache : IEnkaCache
    {
        private readonly string _cacheDirectory;
        private readonly ConcurrentDictionary<string, DateTime> _expirations = new();
        private long _hitCount;
        private long _missCount;

        public SimpleFileCache(string cacheDirectory)
        {
            _cacheDirectory = cacheDirectory;
            Directory.CreateDirectory(cacheDirectory);
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
        {
            var filePath = GetFilePath(key);
            
            if (!File.Exists(filePath))
            {
                Interlocked.Increment(ref _missCount);
                return null;
            }

            // Check expiration
            if (_expirations.TryGetValue(key, out var expiration) && DateTime.UtcNow > expiration)
            {
                await RemoveAsync(key, ct);
                Interlocked.Increment(ref _missCount);
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                var result = JsonSerializer.Deserialize<T>(json);
                Interlocked.Increment(ref _hitCount);
                return result;
            }
            catch
            {
                Interlocked.Increment(ref _missCount);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
        {
            var filePath = GetFilePath(key);
            var json = JsonSerializer.Serialize(value);
            await File.WriteAllTextAsync(filePath, json, ct);
            
            var expiration = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(5));
            _expirations[key] = expiration;
        }

        public Task<bool> RemoveAsync(string key, CancellationToken ct = default)
        {
            var filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _expirations.TryRemove(key, out _);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task ClearAsync(CancellationToken ct = default)
        {
            foreach (var file in Directory.GetFiles(_cacheDirectory, "*.json"))
            {
                File.Delete(file);
            }
            _expirations.Clear();
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            var filePath = GetFilePath(key);
            if (!File.Exists(filePath))
                return Task.FromResult(false);

            // Check expiration
            if (_expirations.TryGetValue(key, out var expiration) && DateTime.UtcNow > expiration)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<CacheStatistics> GetStatsAsync(CancellationToken ct = default)
        {
            var files = Directory.GetFiles(_cacheDirectory, "*.json");
            var totalSize = files.Sum(f => new FileInfo(f).Length);

            return Task.FromResult(new CacheStatistics
            {
                HitCount = _hitCount,
                MissCount = _missCount,
                EntryCount = files.Length,
                SizeBytes = totalSize
            });
        }

        public void Dispose()
        {
            // Nothing to dispose for file-based cache
        }

        private string GetFilePath(string key)
        {
            // Sanitize key for file name
            var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_cacheDirectory, $"{safeKey}.json");
        }
    }
}
