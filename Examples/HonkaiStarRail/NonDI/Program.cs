using EnkaDotNet;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HSRStatsViewer
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
                    Raw = false,
                    EnableCaching = true,
                    PreloadedLanguages = new List<Language> { Language.English },
                    CacheProvider = EnkaDotNet.Caching.CacheProvider.SQLite,
                    SQLiteCache = new EnkaDotNet.Caching.SQLiteCacheOptions
                    {
                        DatabasePath = "HSREnkaCache.db",
                    }
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

                IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, cache);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 80000000;

                Console.WriteLine($"Fetching player data for UID: {uid}...");
                var playerInfo = await client.GetHSRPlayerInfoAsync(uid, Language.English);

                Console.WriteLine($"Player: {playerInfo.Nickname} (Lv.{playerInfo.Level}, World Level: {playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Platform: {playerInfo.Platform}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");
                Console.WriteLine($"Namecard Icon: {playerInfo.NameCardIcon}");

                Console.WriteLine("\nRECORD INFO:");
                Console.WriteLine($"Achievements: {playerInfo.RecordInfo.AchievementCount}");
                Console.WriteLine($"Characters: {playerInfo.RecordInfo.AvatarCount}");
                Console.WriteLine($"Light Cones: {playerInfo.RecordInfo.LightConeCount}");
                Console.WriteLine($"Relics: {playerInfo.RecordInfo.RelicCount}");
                Console.WriteLine($"Memory of Chaos Score: {playerInfo.RecordInfo.MemoryOfChaosScore}");

                if (playerInfo.RecordInfo.BookCount.HasValue)
                    Console.WriteLine($"Books: {playerInfo.RecordInfo.BookCount}");
                if (playerInfo.RecordInfo.MusicCount.HasValue)
                    Console.WriteLine($"Music: {playerInfo.RecordInfo.MusicCount}");
                if (playerInfo.RecordInfo.MaxSimulatedUniverseWorld.HasValue)
                    Console.WriteLine($"Max Simulated Universe World: {playerInfo.RecordInfo.MaxSimulatedUniverseWorld}");

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
                    Console.WriteLine($"Eidolon Rank: {character.Rank} | Position: {character.Position}");
                    Console.WriteLine($"Icon URL: {character.IconUrl}");
                    Console.WriteLine($"Avatar Icon URL: {character.AvatarIconUrl}");

                    Console.WriteLine("\nSTATS:");
                    foreach (var stat in character.GetAllStats())
                    {
                        Console.WriteLine($"  {stat.Key}: {stat.Value}");
                    }

                    if (character.Equipment != null)
                    {
                        var lightCone = character.Equipment;
                        Console.WriteLine("\nLIGHT CONE:");
                        Console.WriteLine($"  Name: {lightCone.Name} (Lv.{lightCone.Level}/Pr.{lightCone.Promotion})");
                        Console.WriteLine($"  Rarity: {lightCone.Rarity} | Path: {lightCone.Path.GetPathName()}");
                        Console.WriteLine($"  Superimposition: {lightCone.Rank}");
                        Console.WriteLine($"  Base HP: {lightCone.FormattedBaseHP.Value}");
                        Console.WriteLine($"  Base ATK: {lightCone.FormattedBaseAttack.Value}");
                        Console.WriteLine($"  Base DEF: {lightCone.FormattedBaseDefense.Value}");
                        Console.WriteLine($"  Icon URL: {lightCone.IconUrl}");
                    }

                    Console.WriteLine("\nEIDOLONS:");
                    if (character.Eidolons != null && character.Eidolons.Any())
                    {
                        foreach (var eidolon in character.Eidolons)
                        {
                            Console.WriteLine($"  - Eidolon ID: {eidolon.Id}, Unlocked: {eidolon.Unlocked}");
                            Console.WriteLine($"    Icon URL: {eidolon.Icon}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  No Eidolon data available.");
                    }

                    if (character.RelicList.Any())
                    {
                        Console.WriteLine("\nRELICS:");
                        foreach (var relic in character.RelicList)
                        {
                            Console.WriteLine($"  - {relic.SetName} ({relic.RelicType.GetRelicName()}) - Lv.{relic.Level}");
                            Console.WriteLine($"    Rarity: {relic.Rarity}");
                            Console.WriteLine($"    Main Stat: {relic.FormattedMainStat.Key}: {relic.FormattedMainStat.Value}");
                            Console.WriteLine($"    Icon URL: {relic.IconUrl}");
                            Console.WriteLine("    Sub Stats:");
                            foreach (var subStat in relic.FormattedSubStats)
                            {
                                Console.WriteLine($"      - {subStat.Key}: {subStat.Value}");
                            }
                        }
                    }

                    var relicSets = character.GetEquippedRelicSets();
                    if (relicSets.Any())
                    {
                        Console.WriteLine("\nRELIC SET BONUSES:");
                        foreach (var setBonus in relicSets)
                        {
                            Console.WriteLine($"  - {setBonus.SetName} ({setBonus.PieceCount}pcs)");
                            foreach (var effectDetail in setBonus.Effects)
                            {
                                Console.WriteLine($"    - {effectDetail.PropertyName}: +{effectDetail.FormattedValue}");
                            }
                        }
                    }

                    Console.WriteLine("\nSKILL TREES (Traces):");
                    if (character.SkillTreeList != null && character.SkillTreeList.Any())
                    {
                        foreach (HSRSkillTree trace in character.SkillTreeList)
                        {
                            string levelDisplay = trace.IsBoosted
                                ? $"{trace.BaseLevel} + {trace.Level - trace.BaseLevel} = {trace.Level}"
                                : $"{trace.Level}";
                            Console.WriteLine($"  - {trace.Name} ({trace.TraceType})");
                            Console.WriteLine($"    Level: {levelDisplay} / {trace.MaxLevel}");
                            Console.WriteLine($"    Boosted by Eidolon: {trace.IsBoosted}");
                            Console.WriteLine($"    Icon URL: {trace.Icon}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  {character.Name} has no skill tree information available.");
                    }
                }
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
                if (ex.InnerException != null) Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
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