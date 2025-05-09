using System;
using System.Linq;
using System.Threading.Tasks;
using EnkaDotNet;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Logging;

namespace GenshinStatsViewer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var options = new EnkaClientOptions
                {
                    Language = "en",
                    GameType = GameType.Genshin,
                    UserAgent = "GenshinStatsViewer/1.0",
                    Raw = false
                };

                ILogger<EnkaClient> clientLogger = null;

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                IEnkaClient client = await EnkaClient.CreateAsync(options, clientLogger);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 800000000;

                Console.WriteLine($"Fetching data for UID: {uid}...");

                var (playerInfo, characters) = await client.GetUserProfileAsync(uid);

                Console.WriteLine($"\nPlayer: {playerInfo.Nickname} (Lv.{playerInfo.Level}, WL{playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Achievements: {playerInfo.FinishedAchievements}");
                if (playerInfo.Challenge?.SpiralAbyss != null)
                {
                    Console.WriteLine($"Spiral Abyss: Floor {playerInfo.Challenge.SpiralAbyss.Floor}-{playerInfo.Challenge.SpiralAbyss.Chamber} ({playerInfo.Challenge.SpiralAbyss.Star} Stars)");
                }
                if (playerInfo.Challenge?.Theater != null)
                {
                    Console.WriteLine($"Imaginarium Theater: Act {playerInfo.Challenge.Theater.Act} ({playerInfo.Challenge.Theater.Star} Stars)");
                }

                if (!characters.Any())
                {
                    Console.WriteLine("No characters found or profile is private.");
                }
                else
                {
                    Console.WriteLine($"\nCharacters ({characters.Count}):");
                }

                foreach (var character in characters)
                {
                    Console.WriteLine($"\n--- {character.Name} (Lv.{character.Level}/Asc. {character.Ascension}) ---");
                    Console.WriteLine($"Element: {character.Element}");
                    Console.WriteLine($"Friendship: {character.Friendship}");
                    Console.WriteLine($"Constellation: C{character.ConstellationLevel}");
                    Console.WriteLine($"Icon: {character.IconUrl}");

                    Console.WriteLine("\n  STATS:");
                    var characterStats = character.GetAllStats();
                    if (characterStats.Any())
                    {
                        foreach (var stat in characterStats)
                        {
                            Console.WriteLine($"    {stat.Key}: {stat.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    No stats available.");
                    }

                    Console.WriteLine("\n  WEAPON:");
                    if (character.Weapon != null)
                    {
                        var weapon = character.Weapon;
                        Console.WriteLine($"    {weapon.Name} (Lv.{weapon.Level}/Asc. {weapon.Ascension}, R{weapon.Refinement})");
                        Console.WriteLine($"      Type: {weapon.Type}");
                        Console.WriteLine($"      Rarity: {weapon.Rarity}");
                        Console.WriteLine($"      Icon: {weapon.IconUrl}");
                        Console.WriteLine($"      {weapon.FormattedBaseAttack.Key}: {weapon.FormattedBaseAttack.Value}");
                        if (weapon.SecondaryStat != null && weapon.SecondaryStat.Type != EnkaDotNet.Enums.Genshin.StatType.None)
                        {
                            Console.WriteLine($"      {weapon.FormattedSecondaryStat.Key}: {weapon.FormattedSecondaryStat.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    No weapon equipped.");
                    }

                    Console.WriteLine("\n  ARTIFACTS:");
                    if (character.Artifacts != null && character.Artifacts.Any())
                    {
                        foreach (var artifact in character.Artifacts.OrderBy(a => (int)a.Slot))
                        {
                            Console.WriteLine($"    {artifact.Name} ({artifact.Slot}) +{artifact.Level}");
                            Console.WriteLine($"      Set: {artifact.SetName}");
                            Console.WriteLine($"      Icon: {artifact.IconUrl}");
                            Console.WriteLine($"      Rarity: {artifact.Rarity}");
                            Console.WriteLine($"      Main Stat: {artifact.FormattedMainStat.Key}: {artifact.FormattedMainStat.Value}");
                            Console.WriteLine($"      Substats:");
                            foreach (var subStat in artifact.FormattedSubStats)
                            {
                                Console.WriteLine($"        {subStat.Key}: {subStat.Value}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("    No artifacts equipped.");
                    }

                    Console.WriteLine("\n  TALENTS:");
                    if (character.Talents != null && character.Talents.Any())
                    {
                        foreach (var talent in character.Talents)
                        {
                            Console.WriteLine($"    {talent.ToString()}");
                            Console.WriteLine($"      Icon: {talent.IconUrl}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    Talent data not available.");
                    }

                    Console.WriteLine("\n  CONSTELLATIONS:");
                    if (character.Constellations != null && character.Constellations.Any())
                    {
                        foreach (var constellation in character.Constellations.OrderBy(c => c.Position))
                        {
                            Console.WriteLine($"    {constellation.ToString()}");
                            Console.WriteLine($"      Icon: {constellation.IconUrl}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    Constellation data not available.");
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
                if (ex.InnerException != null) Console.WriteLine($"  Inner Error: {ex.InnerException.Message}");
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