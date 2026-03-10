using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Exceptions;
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
                    services.AddSingleton<IEnkaCache>(sp => 
                        new SQLiteCacheProvider(new SQLiteCacheOptions { DatabasePath = "genshin_cache.db" }));

                    services.AddEnkaNetClient(options =>
                    {
                        options.CacheDurationMinutes = 60;
                    });

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
        }

        public async Task FetchAndDisplayData()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                int uid = 800000000; // Replace with a valid Genshin Impact UID
                _logger.LogInformation("Fetching data for UID: {Uid}", uid);

                var (playerInfo, characters) = await _enkaClient.GetGenshinUserProfileAsync(uid, language: "en");

                Console.WriteLine($"\nPlayer: {playerInfo.Nickname} (Lv.{playerInfo.Level}, WL{playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Achievements: {playerInfo.FinishedAchievements}");

                if (playerInfo.Challenge?.SpiralAbyss != null)
                    Console.WriteLine($"Spiral Abyss: Floor {playerInfo.Challenge.SpiralAbyss.Floor}-{playerInfo.Challenge.SpiralAbyss.Chamber} ({playerInfo.Challenge.SpiralAbyss.Star} Stars)");

                if (playerInfo.Challenge?.Theater != null)
                    Console.WriteLine($"Imaginarium Theater: Act {playerInfo.Challenge.Theater.Act} ({playerInfo.Challenge.Theater.Star} Stars)");

                if (!characters.Any())
                {
                    Console.WriteLine("No characters found or profile is private.");
                    return;
                }

                Console.WriteLine($"\nCharacters ({characters.Count}):");

                foreach (var character in characters)
                {
                    Console.WriteLine($"\n--- {character.Name} (Lv.{character.Level}/Asc. {character.Ascension}) ---");
                    Console.WriteLine($"Element: {character.Element}");
                    Console.WriteLine($"Friendship: {character.Friendship}");
                    Console.WriteLine($"Constellation: C{character.ConstellationLevel}");
                    Console.WriteLine($"Icon: {character.IconUrl}");

                    // Stats
                    Console.WriteLine("\n  STATS:");
                    var characterStats = character.GetAllStats();
                    if (characterStats.Any())
                    {
                        foreach (var stat in characterStats)
                            Console.WriteLine($"    {stat.Key}: {stat.Value}");
                    }
                    else
                    {
                        Console.WriteLine("    No stats available.");
                    }

                    // Weapon
                    Console.WriteLine("\n  WEAPON:");
                    if (character.Weapon != null)
                    {
                        var weapon = character.Weapon;
                        Console.WriteLine($"    {weapon.Name} (Lv.{weapon.Level}/Asc. {weapon.Ascension}, R{weapon.Refinement})");
                        Console.WriteLine($"      Type: {weapon.Type} | Rarity: {weapon.Rarity}");
                        Console.WriteLine($"      {weapon.FormattedBaseAttack.Key}: {weapon.FormattedBaseAttack.Value}");
                        if (weapon.SecondaryStat != null && weapon.SecondaryStat.Type != EnkaDotNet.Enums.Genshin.StatType.None)
                            Console.WriteLine($"      {weapon.FormattedSecondaryStat.Key}: {weapon.FormattedSecondaryStat.Value}");
                        Console.WriteLine($"      Icon: {weapon.IconUrl}");
                    }
                    else
                    {
                        Console.WriteLine("    No weapon equipped.");
                    }

                    // Artifacts
                    Console.WriteLine("\n  ARTIFACTS:");
                    if (character.Artifacts != null && character.Artifacts.Any())
                    {
                        foreach (var artifact in character.Artifacts.OrderBy(a => (int)a.Slot))
                        {
                            Console.WriteLine($"    {artifact.Name} ({artifact.Slot}) +{artifact.Level}");
                            Console.WriteLine($"      Set: {artifact.SetName} | Rarity: {artifact.Rarity}");
                            Console.WriteLine($"      Main Stat: {artifact.FormattedMainStat.Key}: {artifact.FormattedMainStat.Value}");
                            Console.WriteLine($"      Substats:");
                            foreach (var subStat in artifact.FormattedSubStats)
                                Console.WriteLine($"        {subStat.Key}: {subStat.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    No artifacts equipped.");
                    }

                    // Talents
                    Console.WriteLine("\n  TALENTS:");
                    if (character.Talents != null && character.Talents.Any())
                    {
                        foreach (var talent in character.Talents)
                            Console.WriteLine($"    {talent.ToString()}");
                    }
                    else
                    {
                        Console.WriteLine("    Talent data not available.");
                    }

                    // Constellations
                    Console.WriteLine("\n  CONSTELLATIONS:");
                    if (character.Constellations != null && character.Constellations.Any())
                    {
                        foreach (var constellation in character.Constellations.OrderBy(c => c.Position))
                            Console.WriteLine($"    {constellation.ToString()}");
                    }
                    else
                    {
                        Console.WriteLine("    Constellation data not available.");
                    }
                }
            }
            catch (PlayerNotFoundException ex)
            {
                _logger.LogWarning(ex, "Player not found or profile is private.");
            }
            catch (EnkaNetworkException ex)
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