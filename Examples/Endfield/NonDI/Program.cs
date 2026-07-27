using EnkaDotNet;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EndfieldStatsViewer
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                var options = new EnkaClientOptions
                {
                    UserAgent = "EndfieldStatsViewer/1.0",
                    UseRawStatValues = false,
                };

                var services = new ServiceCollection();
                services.AddLogging(builder =>
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Information)
                );
                services.AddMemoryCache();

                var provider = services.BuildServiceProvider();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var cache = provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();

                using IEnkaClient client = await EnkaClient.CreateAsync(options, loggerFactory, cache);

                long uid = args.Length > 0 && long.TryParse(args[0], out long parsedUid)
                    ? parsedUid
                    : 4228833345L;

                Console.WriteLine($"Fetching Endfield player data for UID: {uid}...");
                var playerInfo = await client.GetEFPlayerInfoAsync(uid, language: "en");
                PrintPlayerInfo(playerInfo);

                if (playerInfo.ShowcaseOperators == null || playerInfo.ShowcaseOperators.Count == 0)
                {
                    Console.WriteLine("\nNo showcase operators found.");
                    return;
                }

                Console.WriteLine("\nSHOWCASE OPERATORS:");
                foreach (var op in playerInfo.ShowcaseOperators)
                {
                    PrintOperator(op);
                }
            }
            catch (PlayerNotFoundException ex)
            {
                Console.WriteLine($"Player not found: {ex.Message}");
            }
            catch (ProfilePrivateException ex)
            {
                Console.WriteLine($"Profile private: {ex.Message}");
            }
            catch (RateLimitException ex)
            {
                Console.WriteLine($"Rate limited: {ex.Message}");
                if (ex.RetryAfter?.Delta.HasValue ?? false)
                {
                    Console.WriteLine($"Retry after: {ex.RetryAfter.Delta.Value.TotalSeconds} seconds.");
                }
            }
            catch (EnkaNetworkException ex)
            {
                Console.WriteLine($"Enka.Network API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
        }

        static void PrintPlayerInfo(EFPlayerInfo playerInfo)
        {
            Console.WriteLine($"Player: {playerInfo.Nickname}");
            Console.WriteLine($"AL {playerInfo.AdminLevel} | EL {playerInfo.EndfieldLevel}");
            Console.WriteLine($"Signature: {playerInfo.Signature}");
            Console.WriteLine($"Region: {playerInfo.Region}");
            Console.WriteLine($"UID: {playerInfo.Uid}");
            Console.WriteLine($"Operators owned: {playerInfo.CharCount}");
            Console.WriteLine($"Weapons owned: {playerInfo.WeaponCount}");
            Console.WriteLine($"Docs: {playerInfo.DocCount}");
            Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");
            Console.WriteLine($"Namecard Icon: {playerInfo.NameCardIcon}");

            PrintMedals(playerInfo.Medals);
            PrintOwnedOperators(playerInfo.CharList);
        }

        static void PrintMedals(IReadOnlyList<EFMedal> medals)
        {
            if (medals == null || medals.Count == 0) return;

            Console.WriteLine("\nACHIEVEMENTS:");
            foreach (var medal in medals)
            {
                string plated = medal.IsPlated ? " [plated]" : string.Empty;
                Console.WriteLine($"  {medal.Name} Lv.{medal.Level}{plated}");
                Console.WriteLine($"    Icon: {medal.IconUrl}");
            }
        }

        static void PrintOwnedOperators(IReadOnlyList<EFCharListEntry> charList)
        {
            if (charList == null || charList.Count == 0) return;

            Console.WriteLine("\nOWNED OPERATORS:");
            foreach (var entry in charList)
            {
                Console.WriteLine($"  {entry.Name} (Lv.{entry.Level}) Potential {entry.PotentialLevel}");
                Console.WriteLine($"    Icon: {entry.IconUrl}");
            }
        }

        static void PrintOperator(EFOperator op)
        {
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine($"OPERATOR: {op.Name} (Lv.{op.Level})");
            Console.WriteLine($"Id: {op.Id} | StrId: {op.StrId}");
            Console.WriteLine($"Rarity: {op.Rarity} | Element: {op.Element} | Profession: {op.Profession}");
            Console.WriteLine($"Weapon Type: {op.WeaponType}");
            Console.WriteLine($"Potential: {op.PotentialLevel}");
            Console.WriteLine($"Icon: {op.IconUrl}");
            Console.WriteLine($"Round Icon: {op.RoundIconUrl}");
            Console.WriteLine($"Splash Art: {op.SplashArtUrl}");
            Console.WriteLine($"Silhouette: {op.SilhouetteUrl}");
            Console.WriteLine($"Profession Icon: {op.ProfessionIconUrl}");

            PrintStats(op);
            PrintWeapon(op.Weapon);
            PrintEquips(op.Equips);
            PrintSkills(op.Skills);
            PrintTalents(op.Talents);
        }

        static void PrintStats(EFOperator op)
        {
            Console.WriteLine("\nSTATS:");
            foreach (var stat in op.GetAllStats())
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }

            Console.WriteLine("\nFINAL STATS (display):");
            foreach (var stat in op.GetFinalStats())
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }

            Console.WriteLine("\nFINAL STATS (raw):");
            foreach (var stat in op.GetFinalStats(raw: true))
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }
        }

        static void PrintWeapon(EFWeapon weapon)
        {
            if (weapon == null) return;

            Console.WriteLine($"\nWEAPON: {weapon.Name} Lv.{weapon.Level}/{weapon.MaxLevel}");
            Console.WriteLine($"  Rarity: {weapon.Rarity} | ATK: {weapon.BaseAtk}");
            Console.WriteLine($"  Type: {weapon.WeaponType}");
            Console.WriteLine($"  Breakthrough: {weapon.BreakthroughLevel} | Refine: {weapon.RefineLevel}");
            Console.WriteLine($"  Icon: {weapon.IconUrl}");
            foreach (var sub in weapon.SubStats)
            {
                Console.WriteLine($"  {sub.Name}: {sub.FormattedValue}");
            }

            PrintGem(weapon.Gem);
        }

        static void PrintGem(EFGem gem)
        {
            if (gem == null) return;

            Console.WriteLine($"  GEM: Id {gem.Id} | Cost {gem.TotalCost} | Domain {gem.DomainId}");
            Console.WriteLine($"    Icon: {gem.IconUrl}");
            if (!string.IsNullOrEmpty(gem.OverlayIconUrl))
            {
                Console.WriteLine($"    Overlay: {gem.OverlayIconUrl}");
            }
            foreach (var term in gem.Terms)
            {
                Console.WriteLine($"    {term.TagName} (cost {term.Cost})");
                if (!string.IsNullOrEmpty(term.TagIconUrl))
                {
                    Console.WriteLine($"      Icon: {term.TagIconUrl}");
                }
            }
        }

        static void PrintEquips(IReadOnlyList<EFEquip> equips)
        {
            if (equips == null || equips.Count == 0) return;

            Console.WriteLine("\nEQUIP:");
            foreach (var equip in equips)
            {
                string suitLabel = string.IsNullOrEmpty(equip.SuitName) ? "(no suit)" : equip.SuitName;
                Console.WriteLine($"  {equip.SlotName}: {suitLabel} (R{equip.Rarity})");
                Console.WriteLine($"    Icon: {equip.IconUrl}");
                if (!string.IsNullOrEmpty(equip.SuitIconUrl))
                {
                    Console.WriteLine($"    Suit Icon: {equip.SuitIconUrl}");
                }
                foreach (var attr in equip.Attributes)
                {
                    Console.WriteLine($"    {attr.Name}: {attr.FormattedValue} (enh {attr.EnhanceLevel})");
                }
            }
        }

        static void PrintSkills(IReadOnlyList<EFSkill> skills)
        {
            if (skills == null || skills.Count == 0) return;

            var combatSkills = skills.Where(s => s.IsCombatSkill).ToList();
            if (combatSkills.Count == 0) return;

            Console.WriteLine("\nSKILLS:");
            foreach (var skill in combatSkills)
            {
                Console.WriteLine($"  {skill.Name}: {skill.Level}/{skill.MaxLevel}");
                if (!string.IsNullOrEmpty(skill.Element))
                {
                    Console.WriteLine($"    Element: {skill.Element}");
                }
                Console.WriteLine($"    Icon: {skill.IconUrl}");
            }
        }

        static void PrintTalents(IReadOnlyList<EFTalent> talents)
        {
            if (talents == null || talents.Count == 0) return;

            Console.WriteLine("\nTALENTS:");
            foreach (var talent in talents)
            {
                Console.WriteLine($"  {talent.Name} Rank {talent.Rank}");
                Console.WriteLine($"    Id: {talent.Id}");
                Console.WriteLine($"    Icon: {talent.IconUrl}");
            }
        }
    }
}
