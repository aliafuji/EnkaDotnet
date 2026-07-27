using EnkaDotNet;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Examples.Endfield.DI
{
    /// <summary>
    /// DI / Hosting sample. For a full field dump see Examples/Endfield/NonDI.
    /// </summary>
    static class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddEnkaSqliteCache(sqlite => sqlite.DatabasePath = "endfield_cache.db");
                    services.AddEnkaNetClient(options =>
                    {
                        options.CacheDurationMinutes = 60;
                        options.UseRawStatValues = false;
                    });
                    services.AddTransient<EndfieldService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await host.Services.GetRequiredService<EndfieldService>().RunAsync(args);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public sealed class EndfieldService
    {
        private readonly IEnkaClient _client;
        private readonly ILogger<EndfieldService> _logger;

        public EndfieldService(IEnkaClient client, ILogger<EndfieldService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task RunAsync(string[] args)
        {
            try
            {
                long uid = args.Length > 0 && long.TryParse(args[0], out long parsed)
                    ? parsed
                    : 4228833345L;

                _logger.LogInformation("Resolving Endfield UID {Uid} via DI client", uid);

                var info = await _client.GetEFPlayerInfoAsync(uid, language: "en");
                int showcaseCount = info.ShowcaseOperators?.Count ?? 0;

                Console.WriteLine($"Resolved {info.Nickname} (UID {info.Uid}) via AddEnkaNetClient");
                Console.WriteLine($"AL {info.AdminLevel} / EL {info.EndfieldLevel} | showcase operators: {showcaseCount}");

                if (showcaseCount == 0)
                {
                    Console.WriteLine("Showcase empty — open NonDI example for a full dump checklist.");
                    return;
                }

                foreach (var op in info.ShowcaseOperators)
                {
                    var finals = op.GetFinalStats();
                    finals.TryGetValue("ATK", out double atk);
                    finals.TryGetValue("HP", out double hp);
                    Console.WriteLine($"  {op.Name} Lv.{op.Level} | HP {hp} | ATK {atk} | {op.Element}/{op.Profession}");
                }
            }
            catch (PlayerNotFoundException ex)
            {
                _logger.LogWarning(ex, "UID not found");
            }
            catch (ProfilePrivateException ex)
            {
                _logger.LogWarning(ex, "Profile private");
            }
            catch (RateLimitException ex)
            {
                _logger.LogWarning(ex, "Rate limited");
            }
            catch (EnkaNetworkException ex)
            {
                _logger.LogError(ex, "Enka API failure");
            }
        }
    }
}
