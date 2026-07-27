using EnkaDotNet;
using EnkaDotNet.Components.EF;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Examples.Endfield.DI
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddEnkaSqliteCache(sqlite => sqlite.DatabasePath = "endfield_cache.db");

                    services.AddEnkaNetClient(options =>
                    {
                        options.CacheDurationMinutes = 60;
                        options.UseRawStatValues = false;
                    });

                    services.AddTransient<EndfieldService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var endfieldService = host.Services.GetRequiredService<EndfieldService>();
            await endfieldService.FetchAndDisplayData(args);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }

    public class EndfieldService
    {
        private readonly IEnkaClient _enkaClient;
        private readonly ILogger<EndfieldService> _logger;

        public EndfieldService(IEnkaClient enkaClient, ILogger<EndfieldService> logger)
        {
            _enkaClient = enkaClient;
            _logger = logger;
        }

        public async Task FetchAndDisplayData(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                long uid = args.Length > 0 && long.TryParse(args[0], out long parsedUid)
                    ? parsedUid
                    : 4228833345L;

                _logger.LogInformation("Fetching Endfield data for UID: {Uid}", uid);

                var playerInfo = await _enkaClient.GetEFPlayerInfoAsync(uid, language: "en");
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
                _logger.LogWarning(ex, "Player not found.");
            }
            catch (ProfilePrivateException ex)
            {
                _logger.LogWarning(ex, "Profile is private.");
            }
            catch (RateLimitException ex)
            {
                _logger.LogWarning(ex, "API rate limit exceeded.");
            }
            catch (EnkaNetworkException ex)
            {
                _logger.LogError(ex, "Enka.Network API error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error.");
            }
        }

        private static void PrintPlayerInfo(EFPlayerInfo playerInfo)
        {
            Console.WriteLine($"\nPlayer: {playerInfo.Nickname}");
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

        private static void PrintMedals(IReadOnlyList<EFMedal> medals)
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

        private static void PrintOwnedOperators(IReadOnlyList<EFCharListEntry> charList)
        {
            if (charList == null || charList.Count == 0) return;

            Console.WriteLine("\nOWNED OPERATORS:");
            foreach (var entry in charList)
            {
                Console.WriteLine($"  {entry.Name} (Lv.{entry.Level}) Potential {entry.PotentialLevel}");
                Console.WriteLine($"    Icon: {entry.IconUrl}");
            }
        }

        private static void PrintOperator(EFOperator op)
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

        private static void PrintStats(EFOperator op)
        {
            Console.WriteLine("\n  STATS:");
            foreach (var stat in op.GetAllStats())
            {
                Console.WriteLine($"    {stat.Key}: {stat.Value}");
            }

            Console.WriteLine("\n  FINAL STATS (display):");
            foreach (var stat in op.GetFinalStats())
            {
                Console.WriteLine($"    {stat.Key}: {stat.Value}");
            }

            Console.WriteLine("\n  FINAL STATS (raw):");
            foreach (var stat in op.GetFinalStats(raw: true))
            {
                Console.WriteLine($"    {stat.Key}: {stat.Value}");
            }
        }

        private static void PrintWeapon(EFWeapon weapon)
        {
            if (weapon == null) return;

            Console.WriteLine("\n  WEAPON:");
            Console.WriteLine($"    {weapon.Name} Lv.{weapon.Level}/{weapon.MaxLevel}");
            Console.WriteLine($"    Rarity: {weapon.Rarity} | ATK: {weapon.BaseAtk}");
            Console.WriteLine($"    Type: {weapon.WeaponType}");
            Console.WriteLine($"    Breakthrough: {weapon.BreakthroughLevel} | Refine: {weapon.RefineLevel}");
            Console.WriteLine($"    Icon: {weapon.IconUrl}");
            foreach (var sub in weapon.SubStats)
            {
                Console.WriteLine($"    {sub.Name}: {sub.FormattedValue}");
            }

            PrintGem(weapon.Gem);
        }

        private static void PrintGem(EFGem gem)
        {
            if (gem == null) return;

            Console.WriteLine($"    GEM: Id {gem.Id} | Cost {gem.TotalCost} | Domain {gem.DomainId}");
            Console.WriteLine($"      Icon: {gem.IconUrl}");
            if (!string.IsNullOrEmpty(gem.OverlayIconUrl))
            {
                Console.WriteLine($"      Overlay: {gem.OverlayIconUrl}");
            }
            foreach (var term in gem.Terms)
            {
                Console.WriteLine($"      {term.TagName} (cost {term.Cost})");
                if (!string.IsNullOrEmpty(term.TagIconUrl))
                {
                    Console.WriteLine($"        Icon: {term.TagIconUrl}");
                }
            }
        }

        private static void PrintEquips(IReadOnlyList<EFEquip> equips)
        {
            if (equips == null || equips.Count == 0) return;

            Console.WriteLine("\n  EQUIP:");
            foreach (var equip in equips)
            {
                string suitLabel = string.IsNullOrEmpty(equip.SuitName) ? "(no suit)" : equip.SuitName;
                Console.WriteLine($"    {equip.SlotName}: {suitLabel} (R{equip.Rarity})");
                Console.WriteLine($"      Icon: {equip.IconUrl}");
                if (!string.IsNullOrEmpty(equip.SuitIconUrl))
                {
                    Console.WriteLine($"      Suit Icon: {equip.SuitIconUrl}");
                }
                foreach (var attr in equip.Attributes)
                {
                    Console.WriteLine($"      {attr.Name}: {attr.FormattedValue} (enh {attr.EnhanceLevel})");
                }
            }
        }

        private static void PrintSkills(IReadOnlyList<EFSkill> skills)
        {
            if (skills == null || skills.Count == 0) return;

            var combatSkills = skills.Where(s => s.IsCombatSkill).ToList();
            if (combatSkills.Count == 0) return;

            Console.WriteLine("\n  SKILLS:");
            foreach (var skill in combatSkills)
            {
                Console.WriteLine($"    {skill.Name}: {skill.Level}/{skill.MaxLevel}");
                if (!string.IsNullOrEmpty(skill.Element))
                {
                    Console.WriteLine($"      Element: {skill.Element}");
                }
                Console.WriteLine($"      Icon: {skill.IconUrl}");
            }
        }

        private static void PrintTalents(IReadOnlyList<EFTalent> talents)
        {
            if (talents == null || talents.Count == 0) return;

            Console.WriteLine("\n  TALENTS:");
            foreach (var talent in talents)
            {
                Console.WriteLine($"    {talent.Name} Rank {talent.Rank}");
                Console.WriteLine($"      Id: {talent.Id}");
                Console.WriteLine($"      Icon: {talent.IconUrl}");
            }
        }
    }
}
