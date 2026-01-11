using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SQLiteCacheExample
{
    /// <summary>
    /// Example demonstrating SQLite persistent cache usage with EnkaDotNet.
    /// SQLite cache persists data across application restarts without external dependencies.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Configure EnkaClient with SQLite cache
                var options = new EnkaClientOptions
                {
                    UserAgent = "SQLiteCacheExample/1.0",
                    
                    // Use SQLite for persistent caching
                    CacheProvider = CacheProvider.SQLite,
                    
                    // Configure SQLite cache options
                    SQLiteCache = new SQLiteCacheOptions
                    {
                        // Path to the SQLite database file
                        DatabasePath = "enka_cache.db",
                        
                        // Cache entries expire after 10 minutes
                        DefaultTtl = TimeSpan.FromMinutes(10),
                        
                        // Enable automatic cleanup of expired entries
                        EnableAutoCleanup = true,
                        
                        // Run cleanup every hour
                        CleanupInterval = TimeSpan.FromHours(1)
                    }
                };

                var services = new ServiceCollection();
                services.AddLogging(builder =>
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information)
                );
                services.AddMemoryCache();

                var provider = services.BuildServiceProvider();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var memoryCache = provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // Create the client with SQLite cache
                IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, memoryCache);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 80000000;

                Console.WriteLine("=== SQLite Cache Example ===\n");
                Console.WriteLine($"Cache database: {options.SQLiteCache.DatabasePath}");
                Console.WriteLine($"Cache TTL: {options.SQLiteCache.DefaultTtl.TotalMinutes} minutes\n");

                // First request - will fetch from API and cache
                Console.WriteLine($"First request for UID {uid} (fetching from API)...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var (playerInfo, characters) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"  Characters: {characters.Count}\n");

                // Second request - should be served from cache
                Console.WriteLine($"Second request for UID {uid} (should be from cache)...");
                stopwatch.Restart();
                var (playerInfo2, characters2) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo2.Nickname} (Lv.{playerInfo2.Level})");
                Console.WriteLine($"  Characters: {characters2.Count}\n");

                Console.WriteLine("Note: The SQLite cache file persists across application restarts.");
                Console.WriteLine("Restart the application to see cached data being served immediately.");
            }
            catch (CacheException ex)
            {
                Console.WriteLine($"Cache error: {ex.Message}");
                Console.WriteLine($"Provider: {ex.Provider}");
                if (ex.ConfigurationProperty != null)
                {
                    Console.WriteLine($"Configuration property: {ex.ConfigurationProperty}");
                }
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
}
