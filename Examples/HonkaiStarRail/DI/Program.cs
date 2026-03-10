using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Exceptions;
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
                    services.AddSingleton<IEnkaCache>(sp => 
                        new SQLiteCacheProvider(new SQLiteCacheOptions { DatabasePath = "hsr_cache.db" }));

                    services.AddEnkaNetClient(options =>
                    {
                        // Cache duration and other options can be configured here
                        options.CacheDurationMinutes = 60;
                    });

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
        }

        public async Task FetchAndDisplayData()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                int uid = 800000000; // Replace with a valid HSR UID
                _logger.LogInformation("Fetching HSR data for UID: {Uid}", uid);

                var playerInfo = await _enkaClient.GetHSRPlayerInfoAsync(uid, language: "en");

                Console.WriteLine($"\nPlayer: {playerInfo.Nickname} (Lv.{playerInfo.Level}, World Level: {playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Platform: {playerInfo.Platform}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");

                Console.WriteLine("\nRECORD INFO:");
                Console.WriteLine($"  Achievements: {playerInfo.RecordInfo.AchievementCount}");
                Console.WriteLine($"  Characters: {playerInfo.RecordInfo.AvatarCount}");
                Console.WriteLine($"  Light Cones: {playerInfo.RecordInfo.LightConeCount}");
                Console.WriteLine($"  Relics: {playerInfo.RecordInfo.RelicCount}");
                Console.WriteLine($"  Memory of Chaos Score: {playerInfo.RecordInfo.MemoryOfChaosScore}");

                if (!playerInfo.DisplayedCharacters.Any())
                {
                    Console.WriteLine("\nNo characters found in showcase.");
                    return;
                }

                Console.WriteLine("\nDISPLAYED CHARACTERS:");
                foreach (var character in playerInfo.DisplayedCharacters)
                {
                    Console.WriteLine("\n" + new string('=', 60));
                    Console.WriteLine($"CHARACTER: {character.Name} (Lv.{character.Level}/Pr.{character.Promotion})");
                    Console.WriteLine($"Rarity: {character.Rarity} | Path: {character.Path.GetPathName()}");
                    Console.WriteLine($"Element: {character.Element.GetElementName()}");
                    Console.WriteLine($"Eidolon Rank: {character.Rank}");
                    Console.WriteLine($"Icon URL: {character.IconUrl}");

                    // Stats
                    Console.WriteLine("\n  STATS:");
                    foreach (var stat in character.GetAllStats())
                        Console.WriteLine($"    {stat.Key}: {stat.Value}");

                    // Light Cone
                    if (character.Equipment != null)
                    {
                        var lc = character.Equipment;
                        Console.WriteLine("\n  LIGHT CONE:");
                        Console.WriteLine($"    {lc.Name} (Lv.{lc.Level}/Pr.{lc.Promotion})");
                        Console.WriteLine($"    Rarity: {lc.Rarity} | Path: {lc.Path.GetPathName()}");
                        Console.WriteLine($"    Superimposition: {lc.Rank}");
                        Console.WriteLine($"    Base HP: {lc.FormattedBaseHP.Value}");
                        Console.WriteLine($"    Base ATK: {lc.FormattedBaseAttack.Value}");
                        Console.WriteLine($"    Base DEF: {lc.FormattedBaseDefense.Value}");
                        Console.WriteLine($"    Icon URL: {lc.IconUrl}");
                    }

                    // Eidolons
                    Console.WriteLine("\n  EIDOLONS:");
                    if (character.Eidolons != null && character.Eidolons.Any())
                    {
                        foreach (var eidolon in character.Eidolons)
                            Console.WriteLine($"    - ID: {eidolon.Id}, Unlocked: {eidolon.Unlocked}");
                    }
                    else
                    {
                        Console.WriteLine("    No Eidolon data available.");
                    }

                    // Relics
                    if (character.RelicList.Any())
                    {
                        Console.WriteLine("\n  RELICS:");
                        foreach (var relic in character.RelicList)
                        {
                            Console.WriteLine($"    - {relic.SetName} ({relic.RelicType.GetRelicName()}) Lv.{relic.Level}");
                            Console.WriteLine($"      Main Stat: {relic.FormattedMainStat.Key}: {relic.FormattedMainStat.Value}");
                            Console.WriteLine($"      Sub Stats:");
                            foreach (var subStat in relic.FormattedSubStats)
                                Console.WriteLine($"        {subStat.Key}: {subStat.Value}");
                        }

                        var relicSets = character.GetEquippedRelicSets();
                        if (relicSets.Any())
                        {
                            Console.WriteLine("\n  RELIC SET BONUSES:");
                            foreach (var setBonus in relicSets)
                            {
                                Console.WriteLine($"    - {setBonus.SetName} ({setBonus.PieceCount}pcs)");
                                foreach (var effect in setBonus.Effects)
                                    Console.WriteLine($"      {effect.PropertyName}: +{effect.FormattedValue}");
                            }
                        }
                    }

                    // Skill Trees
                    Console.WriteLine("\n  SKILL TREES (Traces):");
                    if (character.SkillTreeList != null && character.SkillTreeList.Any())
                    {
                        foreach (HSRSkillTree trace in character.SkillTreeList)
                        {
                            string levelDisplay = trace.IsBoosted
                                ? $"{trace.BaseLevel} + {trace.Level - trace.BaseLevel} = {trace.Level}"
                                : $"{trace.Level}";
                            Console.WriteLine($"    - {trace.Name} ({trace.TraceType})");
                            Console.WriteLine($"      Level: {levelDisplay} / {trace.MaxLevel} | Boosted: {trace.IsBoosted}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    No skill tree data available.");
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