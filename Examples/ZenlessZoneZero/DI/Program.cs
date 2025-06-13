using EnkaDotNet.DIExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Examples.ZenlessZoneZero.DI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddEnkaNetClient();
                    services.AddTransient<ZzzService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var zzzService = host.Services.GetRequiredService<ZzzService>();
            await zzzService.FetchAndDisplayData();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public class ZzzService
    {
        private readonly IEnkaClient _enkaClient;
        private readonly ILogger<ZzzService> _logger;

        public ZzzService(IEnkaClient enkaClient, ILogger<ZzzService> logger)
        {
            _enkaClient = enkaClient;
            _logger = logger;
            _logger.LogInformation($"EnkaClient initialized");
        }

        public async Task FetchAndDisplayData()
        {
            _logger.LogInformation("Fetching Zenless Zone Zero Data...");
            try
            {
                int uid = 10000000; // Replace with a valid ZZZ UID
                _logger.LogInformation($"Fetching ZZZ player info for UID: {uid}");

                var playerInfo = await _enkaClient.GetZZZPlayerInfoAsync(uid, language: "en");
                _logger.LogInformation($"Nickname: {playerInfo.Nickname}, Level: {playerInfo.Level}");

                if (playerInfo.ShowcaseAgents.Any())
                {
                    _logger.LogInformation("\nShowcase Agents:");
                    foreach (var agent in playerInfo.ShowcaseAgents)
                    {
                        _logger.LogInformation($"  - {agent.Name} (Lv. {agent.Level})");
                    }
                }
                else
                {
                    _logger.LogInformation("No agent data found or profile might be private.");
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