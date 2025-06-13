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
                    services.AddTransient<EnkaProfileService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var genshinService = host.Services.GetRequiredService<EnkaProfileService>();
            await genshinService.FetchAndDisplayData();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public class EnkaProfileService
    {
        private readonly IEnkaClient _enkaClient;
        private readonly ILogger<EnkaProfileService> _logger;

        public EnkaProfileService(IEnkaClient enkaClient, ILogger<EnkaProfileService> logger)
        {
            _enkaClient = enkaClient;
            _logger = logger;
            _logger.LogInformation($"EnkaClient initialized");
        }

        public async Task FetchAndDisplayData()
        {
            _logger.LogInformation("Fetching Genshin Impact Data (DI)...");
            try
            {
                string username = "username"; // Replace with the actual username
                _logger.LogInformation($"Fetching profile for username: {username}");

                var userProfile = await _enkaClient.GetEnkaProfileByUsernameAsync(username);

                _logger.LogInformation($"ID: {userProfile.UserId}");
                _logger.LogInformation($"Level:  {userProfile.Level}");
                _logger.LogInformation($"Username: {userProfile.Username}");
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