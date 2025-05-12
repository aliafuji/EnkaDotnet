using System;
using System.Linq;
using System.Threading.Tasks;
using EnkaDotNet;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Logging;

namespace ZZZStatsViewer
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
                    UserAgent = "ZZZStatsViewer/1.0",
                    Raw = false,
                };

                ILogger<EnkaClient> clientLogger = null;

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                IEnkaClient client = await EnkaClient.CreateAsync(options, clientLogger);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 100000000;

                Console.WriteLine($"Fetching player data for UID: {uid}...");
                var playerInfo = await client.GetZZZPlayerInfoAsync(uid, language: "en");

                Console.WriteLine($"Player: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"Title: {playerInfo.TitleText}");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");
                Console.WriteLine($"Player Namecard Icon: {playerInfo.NameCardIcon}");

                if (!playerInfo.ShowcaseAgents.Any())
                {
                    Console.WriteLine("\nNo showcase agents found.");
                    return;
                }

                Console.WriteLine("\nMEDALS:");
                if (playerInfo.Medals != null && playerInfo.Medals.Any())
                {
                    foreach (var medal in playerInfo.Medals)
                    {
                        Console.WriteLine($"Name: {medal.Name} (Lv.{medal.Value})");
                        Console.WriteLine($"Icon: {medal.Icon}");
                        Console.WriteLine($"Type: {medal.Type}");
                    }
                }
                else
                {
                    Console.WriteLine("No medals to display.");
                }


                Console.WriteLine("\nSHOWCASE AGENTS:");
                foreach (var agent in playerInfo.ShowcaseAgents)
                {
                    Console.WriteLine("\n" + new string('-', 50));
                    Console.WriteLine($"AGENT: {agent.Name} (Lv.{agent.Level})");
                    Console.WriteLine($"Rarity: {agent.Rarity} | Profession: {agent.ProfessionType}");
                    Console.WriteLine($"Elements: {string.Join(", ", agent.ElementTypes)}");
                    Console.WriteLine($"Mindscapes (Talent Level): {agent.TalentLevel}");
                    Console.WriteLine($"Image URL: {agent.ImageUrl}");
                    Console.WriteLine($"Circle Icon URL: {agent.CircleIconUrl}");
                    Console.WriteLine($"Promotion Level: {agent.PromotionLevel}");
                    Console.WriteLine($"Obtained Time: {agent.ObtainmentTimestamp.ToLocalTime()}");
                    Console.WriteLine($"Weapon Effect State: {agent.WeaponEffectState}");

                    if (agent.Colors != null && agent.Colors.Any())
                    {
                        foreach (var color in agent.Colors)
                        {
                            Console.WriteLine($"Color Accent: {color.Accent} - Mindscape Color: {color.Mindscape}");
                        }
                    }


                    Console.WriteLine("\nAGENT STATS (Calculated):");
                    var agentStats = agent.GetAllStats();
                    foreach (var statPair in agentStats)
                    {
                        Console.WriteLine($"{statPair.Key}: {statPair.Value.Final} (Base: {statPair.Value.Base}, Added: {statPair.Value.Added})");
                    }

                    if (agent.Weapon != null)
                    {
                        Console.WriteLine("\nW-ENGINE (Weapon):");
                        Console.WriteLine($"Name: {agent.Weapon.Name} (Lv.{agent.Weapon.Level}/BreakLvl.{agent.Weapon.BreakLevel})");
                        Console.WriteLine($"Rarity: {agent.Weapon.Rarity}");
                        Console.WriteLine($"Main Stat: {agent.Weapon.FormattedMainStat.Key}: {agent.Weapon.FormattedMainStat.Value}");
                        Console.WriteLine($"Secondary Stat: {agent.Weapon.FormattedSecondaryStat.Key}: {agent.Weapon.FormattedSecondaryStat.Value}");
                        Console.WriteLine($"Effect State: {agent.WeaponEffectState}");
                        Console.WriteLine($"Overclock (Upgrade Level): {agent.Weapon.UpgradeLevel}");
                        Console.WriteLine($"Icon URL: {agent.Weapon.ImageUrl}");
                    }

                    var discSets = agent.GetEquippedDiscSets();
                    if (discSets.Any())
                    {
                        Console.WriteLine("\nDRIVE DISC SET BONUSES:");
                        foreach (var set in discSets)
                        {
                            Console.WriteLine($"- {set.SuitName} ({set.PieceCount} pieces)");
                            foreach (var bonus in set.BonusStats)
                            {
                                Console.WriteLine($"  - {bonus.Key}: {bonus.Value}");
                            }
                        }
                    }

                    if (agent.EquippedDiscs.Any())
                    {
                        Console.WriteLine("\nEQUIPPED DISCS:");
                        foreach (var disc in agent.EquippedDiscs.OrderBy(d => d.Slot))
                        {
                            Console.WriteLine($"- {disc.SuitName} ({disc.Slot}) (Lv.{disc.Level})");
                            Console.WriteLine($"  Main Stat: {disc.FormattedMainStat.Key}: {disc.FormattedMainStat.Value}");
                            Console.WriteLine($"  Rarity: {disc.Rarity}");
                            Console.WriteLine($"  Icon URL: {disc.IconUrl}");
                            Console.WriteLine($"  Locked: {disc.IsLocked}, Available: {disc.IsAvailable}, Trash: {disc.IsTrash}");
                            Console.WriteLine("  Substats:");
                            foreach (var statPair in disc.SubStats)
                            {
                                Console.WriteLine($"    - {statPair.Key}: {statPair.Value}");
                            }
                        }
                    }

                    if (agent.CoreSkillEnhancements != null && agent.CoreSkillEnhancements.Any())
                    {
                        Console.WriteLine("\nCORE SKILL ENHANCEMENTS:");
                        for (int i = 0; i < agent.CoreSkillEnhancement; i++)
                        {
                            Console.WriteLine($"- Enhancement Level {i + 1} Active");
                        }
                        Console.WriteLine($"Total Core Skill Enhancement Level: {agent.CoreSkillEnhancement}");
                    }


                    Console.WriteLine("\nSKILL LEVELS:");
                    if (agent.SkillLevels != null && agent.SkillLevels.Any())
                    {
                        foreach (var skill in agent.SkillLevels.OrderBy(s => s.Key))
                        {
                            Console.WriteLine($"- {skill.Key}: {skill.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  No skill level data available.");
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