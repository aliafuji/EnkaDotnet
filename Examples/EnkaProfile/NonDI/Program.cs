using EnkaDotNet;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnkaProfileViewer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                var options = new EnkaClientOptions
                {
                    UserAgent = "HSRStatsViewer/1.0",
                    Raw = false
                };

                var services = new ServiceCollection();

                services.AddLogging(builder =>
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Trace)
                );
                services.AddMemoryCache();

                var provider = services.BuildServiceProvider();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var cache = provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, cache);

                string username = "username"; // Replace with the username you want to fetch

                Console.WriteLine($"Fetching enka profile for {username}...");
                var enkaInfo = await client.GetEnkaProfileByUsernameAsync(username);

                if (enkaInfo == null)
                {
                    Console.WriteLine("No profile found.");
                    return;
                }

                Console.WriteLine($"Profile found for username: {enkaInfo.Username}");
                Console.WriteLine($"Profile Image: {enkaInfo.ProfileImageUrl}");
                Console.WriteLine($"Level: {enkaInfo.Level}");
                Console.WriteLine($"ID: {enkaInfo.UserId}");
                Console.WriteLine($"Avatar URL: {enkaInfo.AvatarUrl}");

            }
            catch (PlayerNotFoundException ex)
            {
                Console.WriteLine($"Error: Player with UID {ex.Uid} not found or profile is private. Message: {ex.Message}");
            }
            catch (ProfilePrivateException ex)
            {
                Console.WriteLine($"Error: Profile for UID {ex.Uid} is private. Message: {ex.Message}");
            }
            catch (RateLimitException ex)
            {
                Console.WriteLine($"Error: API rate limit exceeded. Please try again later. Details: {ex.Message}");
                if (ex.RetryAfter?.Delta.HasValue ?? false)
                {
                    Console.WriteLine($"Retry after: {ex.RetryAfter.Delta.Value.TotalSeconds} seconds.");
                }
                else if (ex.RetryAfter?.Date.HasValue ?? false)
                {
                    Console.WriteLine($"Retry after: {ex.RetryAfter.Date.Value.ToLocalTime()}");
                }
            }
            catch (EnkaNetworkException ex)
            {
                Console.WriteLine($"An Enka.Network API error occurred: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"  Inner Error: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
        }
    }
}