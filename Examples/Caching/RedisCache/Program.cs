using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RedisCacheExample
{
    /// <summary>
    /// Example demonstrating Redis distributed cache usage with EnkaDotNet.
    /// Redis cache allows multiple application instances to share cached data.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Configure EnkaClient with Redis cache
                var options = new EnkaClientOptions
                {
                    UserAgent = "RedisCacheExample/1.0",
                    
                    // Use Redis for distributed caching
                    CacheProvider = CacheProvider.Redis,
                    
                    // Configure Redis cache options
                    RedisCache = new RedisCacheOptions
                    {
                        // Redis server connection string
                        // Format: host:port or host:port,password=xxx,ssl=true
                        ConnectionString = "localhost:6379",
                        
                        // Key prefix for namespace isolation
                        // Useful when sharing Redis with other applications
                        KeyPrefix = "enka:example:",
                        
                        // Cache entries expire after 10 minutes
                        DefaultTtl = TimeSpan.FromMinutes(10),
                        
                        // Number of connection retry attempts
                        ConnectRetry = 3,
                        
                        // Connection timeout
                        ConnectTimeout = TimeSpan.FromSeconds(5)
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

                Console.WriteLine("=== Redis Cache Example ===\n");
                Console.WriteLine($"Redis server: {options.RedisCache.ConnectionString}");
                Console.WriteLine($"Key prefix: {options.RedisCache.KeyPrefix}");
                Console.WriteLine($"Cache TTL: {options.RedisCache.DefaultTtl.TotalMinutes} minutes\n");

                Console.WriteLine("Connecting to Redis...");

                // Create the client with Redis cache
                IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, memoryCache);

                Console.WriteLine("Connected successfully!\n");

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 80000000;

                // First request - will fetch from API and cache in Redis
                Console.WriteLine($"First request for UID {uid} (fetching from API)...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var (playerInfo, characters) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"  Characters: {characters.Count}\n");

                // Second request - should be served from Redis cache
                Console.WriteLine($"Second request for UID {uid} (should be from Redis cache)...");
                stopwatch.Restart();
                var (playerInfo2, characters2) = await client.GetGenshinUserProfileAsync(uid, language: "en");
                stopwatch.Stop();
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Player: {playerInfo2.Nickname} (Lv.{playerInfo2.Level})");
                Console.WriteLine($"  Characters: {characters2.Count}\n");

                Console.WriteLine("Note: Redis cache is shared across all application instances.");
                Console.WriteLine("Run multiple instances of this application to see shared caching in action.");
            }
            catch (CacheException ex)
            {
                Console.WriteLine($"Cache error: {ex.Message}");
                Console.WriteLine($"Provider: {ex.Provider}");
                if (ex.ConfigurationProperty != null)
                {
                    Console.WriteLine($"Configuration property: {ex.ConfigurationProperty}");
                }
                Console.WriteLine("\nMake sure Redis server is running on localhost:6379");
                Console.WriteLine("You can start Redis using Docker: docker run -p 6379:6379 redis");
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
