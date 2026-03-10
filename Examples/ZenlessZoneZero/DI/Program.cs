using EnkaDotNet;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Exceptions;
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
                    services.AddSingleton<IEnkaCache>(sp => 
                        new SQLiteCacheProvider(new SQLiteCacheOptions { DatabasePath = "zzz_cache.db" }));

                    services.AddEnkaNetClient(options =>
                    {
                        // Cache duration and other options can be configured here
                        options.CacheDurationMinutes = 60;
                    });

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
        }

        public async Task FetchAndDisplayData()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                int uid = 10000000; // Replace with a valid ZZZ UID
                _logger.LogInformation("Fetching ZZZ data for UID: {Uid}", uid);

                var playerInfo = await _enkaClient.GetZZZPlayerInfoAsync(uid, language: "en");

                Console.WriteLine($"\nPlayer: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"Title: {playerInfo.TitleText}");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");
                Console.WriteLine($"Namecard Icon: {playerInfo.NameCardIcon}");

                // Medals
                Console.WriteLine("\nMEDALS:");
                if (playerInfo.Medals != null && playerInfo.Medals.Any())
                {
                    foreach (var medal in playerInfo.Medals)
                        Console.WriteLine($"  {medal.Name} (Lv.{medal.Value}) - {medal.Type}");
                }
                else
                {
                    Console.WriteLine("  No medals to display.");
                }

                if (!playerInfo.ShowcaseAgents.Any())
                {
                    Console.WriteLine("\nNo showcase agents found.");
                    return;
                }

                Console.WriteLine("\nSHOWCASE AGENTS:");
                foreach (var agent in playerInfo.ShowcaseAgents)
                {
                    Console.WriteLine("\n" + new string('-', 50));
                    Console.WriteLine($"AGENT: {agent.Name} (Lv.{agent.Level})");
                    Console.WriteLine($"Rarity: {agent.Rarity} | Profession: {agent.ProfessionType}");
                    Console.WriteLine($"Elements: {string.Join(", ", agent.ElementTypes)}");
                    Console.WriteLine($"Mindscapes: {agent.TalentLevel}");
                    Console.WriteLine($"Promotion Level: {agent.PromotionLevel}");
                    Console.WriteLine($"Image URL: {agent.ImageUrl}");

                    if (agent.Colors != null && agent.Colors.Any())
                    {
                        foreach (var color in agent.Colors)
                            Console.WriteLine($"  Colors -> Accent: {color.Accent} | AccentExtra: {color.AccentExtra} | Mindscape: {color.Mindscape}");
                    }

                    // Stats
                    Console.WriteLine("\n  STATS (Calculated):");
                    var agentStats = agent.GetAllStats();
                    foreach (var statPair in agentStats)
                        Console.WriteLine($"    {statPair.Key}: {statPair.Value.Final} (Base: {statPair.Value.Base}, Added: {statPair.Value.Added})");

                    // W-Engine
                    if (agent.Weapon != null)
                    {
                        Console.WriteLine("\n  W-ENGINE:");
                        Console.WriteLine($"    {agent.Weapon.Name} (Lv.{agent.Weapon.Level}/BreakLvl.{agent.Weapon.BreakLevel})");
                        Console.WriteLine($"    Rarity: {agent.Weapon.Rarity}");
                        Console.WriteLine($"    Main Stat: {agent.Weapon.FormattedMainStat.Key}: {agent.Weapon.FormattedMainStat.Value}");
                        Console.WriteLine($"    Secondary Stat: {agent.Weapon.FormattedSecondaryStat.Key}: {agent.Weapon.FormattedSecondaryStat.Value}");
                        Console.WriteLine($"    Overclock: {agent.Weapon.UpgradeLevel}");
                        Console.WriteLine($"    Icon URL: {agent.Weapon.ImageUrl}");
                    }

                    // Drive Disc Sets
                    var discSets = agent.GetEquippedDiscSets();
                    if (discSets.Any())
                    {
                        Console.WriteLine("\n  DRIVE DISC SET BONUSES:");
                        foreach (var set in discSets)
                        {
                            Console.WriteLine($"    - {set.SuitName} ({set.PieceCount} pieces)");
                            foreach (var bonus in set.BonusStats)
                                Console.WriteLine($"      {bonus.Key}: {bonus.Value}");
                        }
                    }

                    // Equipped Discs
                    if (agent.EquippedDiscs.Any())
                    {
                        Console.WriteLine("\n  EQUIPPED DISCS:");
                        foreach (var disc in agent.EquippedDiscs.OrderBy(d => d.Slot))
                        {
                            Console.WriteLine($"    - {disc.SuitName} ({disc.Slot}) Lv.{disc.Level}");
                            Console.WriteLine($"      Main Stat: {disc.FormattedMainStat.Key}: {disc.FormattedMainStat.Value}");
                            Console.WriteLine($"      Rarity: {disc.Rarity}");
                            Console.WriteLine("      Substats:");
                            foreach (var statPair in disc.SubStats)
                                Console.WriteLine($"        {statPair.Key}: {statPair.Value}");
                        }
                    }

                    // Core Skill Enhancements
                    if (agent.CoreSkillEnhancements != null && agent.CoreSkillEnhancements.Any())
                    {
                        Console.WriteLine($"\n  CORE SKILL ENHANCEMENT: Level {agent.CoreSkillEnhancement}");
                    }

                    // Skill Levels
                    Console.WriteLine("\n  SKILL LEVELS:");
                    if (agent.SkillLevels != null && agent.SkillLevels.Any())
                    {
                        foreach (var skill in agent.SkillLevels.OrderBy(s => s.Key))
                            Console.WriteLine($"    - {skill.Key}: {skill.Value}");
                    }
                    else
                    {
                        Console.WriteLine("    No skill level data available.");
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