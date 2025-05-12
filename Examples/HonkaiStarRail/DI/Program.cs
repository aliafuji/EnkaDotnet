using System;
using System.Linq;
using System.Threading.Tasks;
using EnkaDotNet;
using EnkaDotNet.Enums;
using EnkaDotNet.DIExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Examples.HonkaiStarRail.DI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddEnkaNetClient();
                    services.AddTransient<HsrService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var hsrService = host.Services.GetRequiredService<HsrService>();
            await hsrService.FetchAndDisplayData();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public class HsrService
    {
        private readonly IEnkaClient _enkaClient;
        private readonly ILogger<HsrService> _logger;

        public HsrService(IEnkaClient enkaClient, ILogger<HsrService> logger)
        {
            _enkaClient = enkaClient;
            _logger = logger;
            _logger.LogInformation($"EnkaClient initialized");
        }

        public async Task FetchAndDisplayData()
        {
            _logger.LogInformation("Fetching Honkai: Star Rail Data (DI)...");
            try
            {
                int uid = 800000000; // Replace with a valid HSR UID
                _logger.LogInformation($"Fetching HSR player info for UID: {uid}");

                var playerInfo = await _enkaClient.GetHSRPlayerInfoAsync(uid, language: "en");
                _logger.LogInformation($"Nickname: {playerInfo.Nickname}, Level: {playerInfo.Level}");

                if (playerInfo.DisplayedCharacters.Any())
                {
                    _logger.LogInformation("\nDisplayed Characters:");
                    foreach (var character in playerInfo.DisplayedCharacters)
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