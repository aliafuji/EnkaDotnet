using EnkaDotNet.DIExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Examples.GenshinImpact.DI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddEnkaNetClient();
                    services.AddTransient<GenshinService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var genshinService = host.Services.GetRequiredService<GenshinService>();
            await genshinService.FetchAndDisplayData();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public class GenshinService
    {
        private readonly IEnkaClient _enkaClient;
        private readonly ILogger<GenshinService> _logger;

        public GenshinService(IEnkaClient enkaClient, ILogger<GenshinService> logger)
        {
            _enkaClient = enkaClient;
            _logger = logger;
            _logger.LogInformation($"EnkaClient initialized");
        }

        public async Task FetchAndDisplayData()
        {
            _logger.LogInformation("Fetching Genshin Impact Data...");
            try
            {
                int uid = 800000000; // Replace with a valid Genshin Impact UID
                _logger.LogInformation($"Fetching profile for UID: {uid}");

                var userProfile = await _enkaClient.GetGenshinUserProfileAsync(uid, language: "en");

                _logger.LogInformation($"Nickname: {userProfile.PlayerInfo.Nickname}");
                _logger.LogInformation($"Level: {userProfile.PlayerInfo.Level}");

                if (userProfile.Characters.Any())
                {
                    _logger.LogInformation($"Characters ({userProfile.Characters.Count}):");
                    foreach (var character in userProfile.Characters)
                    {
                        _logger.LogInformation($"  - {character.Name} (Lv. {character.Level})");
                    }
                }
                else
                {
                    _logger.LogInformation("No character data found or profile might be private.");
                }
            }
            catch (EnkaDotNet.Exceptions.PlayerNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Player with UID was not found or profile is private.");
            }
            catch (EnkaDotNet.Exceptions.EnkaNetworkException ex)
            {
                _logger.LogError(ex, "An Enka.Network API error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
            }
        }
    }
}