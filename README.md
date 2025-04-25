# Enka.DotNet  

Enka.DotNet is a C# wrapper for accessing and processing character data from the Enka.Network API. It provides a simple interface to retrieve detailed information about characters, artifacts, weapons, and player profiles for Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)

## Features  

- Fetch detailed character builds including artifacts, weapons, stats, and constellations  
- Access player profile information, including namecards and achievements  
- Retrieve comprehensive agent details, including stats, weapons, and drive disc sets for Zenless Zone Zero  
- View detailed character stats, relics, light cones, and skill trees for Honkai Star Rail
- Strongly typed models for all Hoyoverse game entities
- Support for multiple games: Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero

## Supported Games  

| Game              | Status         | API Support |  
| ----------------- | -------------- | ----------- |  
| Genshin Impact    | ✅ Ready       | Full        |  
| Honkai: Star Rail | ✅ Ready       | Full        |  
| Zenless Zone Zero | ✅ Ready       | Full        |

## Installation

Install EnkaDotNet via NuGet package manager:

```
Install-Package EnkaDotNet
```

Or via the .NET CLI:

```
dotnet add package EnkaDotNet
```

## Example Code for Genshin Impact

```csharp
using EnkaDotNet;

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
                    Language = "ja",
                    GameType = EnkaDotNet.Enums.GameType.Genshin,
                    EnableCaching = true,
                    UserAgent = "EnkaDotNet/5.0",
                    Raw = true
                };

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                using var client = new EnkaClient(options);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 829344442;

                Console.WriteLine($"Fetching data for UID: {uid}...");

                var (playerInfo, characters) = await client.GetUserProfile(uid);

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

                Console.WriteLine($"\nCharacters ({characters.Count}):");

                if (!characters.Any())
                {
                    Console.WriteLine("No characters found or profile is private.");
                }

                foreach (var character in characters)
                {
                    Console.WriteLine($"\n--- {character.Name} (Lv.{character.Level}/{character.Ascension}) ---");
                    Console.WriteLine($"Element: {character.Element}");
                    Console.WriteLine($"Friendship: {character.Friendship}");
                    Console.WriteLine($"Constellation: C{character.ConstellationLevel}");
                    Console.WriteLine($"Icon: {character.IconUrl}");

                    Console.WriteLine("\n  STATS:");
                    foreach (var stat in character.GetAllStats())
                    {
                        Console.WriteLine($"    {stat.Key}: {stat.Value}");
                    }

                    Console.WriteLine("\n  WEAPON:");
                    if (character.Weapon != null)
                    {
                        var weapon = character.Weapon;
                        Console.WriteLine($"    {weapon.Name} (Lv.{weapon.Level}/{weapon.Ascension}, R{weapon.Refinement})");
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
                            Console.WriteLine($"    {talent.IconUrl}");
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
                            Console.WriteLine($"    {constellation.IconUrl}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("    Constellation data not available.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
```

## Example Code for Zenless Zone Zero

```csharp
using EnkaDotNet;

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
                    GameType = EnkaDotNet.Enums.GameType.ZZZ,
                    Language = "ja",
                    EnableCaching = true,
                    UserAgent = "EnkaDotNet/5.0",
                    Raw = false
                };

                using var client = new EnkaClient(options);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 1302375046;

                Console.WriteLine($"Fetching player data for UID: {uid}...");
                var playerInfo = await client.GetZZZPlayerInfo(uid);

                Console.WriteLine($"Player: {playerInfo.Nickname} (Lv.{playerInfo.Level})");
                Console.WriteLine($"Title: {playerInfo.TitleText}");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");
                Console.WriteLine($"Player Namecard Icon: {playerInfo.NameCardIcon}");

                if (playerInfo.ShowcaseAgents.Count == 0)
                {
                    Console.WriteLine("\nNo showcase agents found.");
                    return;
                }

                Console.WriteLine("\nMEDALS:");
                foreach (var medal in playerInfo.Medals)
                {
                    Console.WriteLine($"Name: {medal.Name} (Lv.{medal.Value})");
                    Console.WriteLine($"Icon: {medal.Icon}");
                    Console.WriteLine($"Type: {medal.Type}");
                }

                Console.WriteLine("\nSHOWCASE AGENTS:");
                foreach (var agent in playerInfo.ShowcaseAgents)
                {
                    Console.WriteLine("\n" + new string('-', 50));
                    Console.WriteLine($"AGENT: {agent.Name} (Lv.{agent.Level})");
                    Console.WriteLine($"Rarity: {agent.Rarity} | Profession: {agent.ProfessionType}");
                    Console.WriteLine($"Elements: {string.Join(", ", agent.ElementTypes)}");
                    Console.WriteLine($"Mindscapes: M{agent.TalentLevel}");
                    Console.WriteLine($"Image URL: {agent.ImageUrl}");
                    Console.WriteLine($"Circle Icon URL: {agent.CircleIconUrl}");
                    Console.WriteLine($"Promotion Level: {agent.PromotionLevel}");
                    Console.WriteLine($"Obtained Time: {agent.ObtainmentTimestamp.ToLocalTime()}");
                    Console.WriteLine($"Weapon Effect State: {agent.WeaponEffectState}");

                    Console.WriteLine("\nAGENT STATS:");
                    var agentStats = agent.GetAllStats();
                    foreach (var statPair in agentStats)
                    {
                        Console.WriteLine($"{statPair.Key}: {statPair.Value}");
                    }

                    if (agent.Weapon != null)
                    {
                        Console.WriteLine("\nW-ENGINE:");
                        Console.WriteLine($"Name: {agent.Weapon.Name} (Lv.{agent.Weapon.Level}/{agent.Weapon.BreakLevel})");
                        Console.WriteLine($"Rarity: {agent.Weapon.Rarity}");
                        Console.WriteLine($"Main Stat: {agent.Weapon.FormattedMainStat.Key}: {agent.Weapon.FormattedMainStat.Value}");
                        Console.WriteLine($"Secondary Stat: {agent.Weapon.FormattedSecondaryStat.Key}: {agent.Weapon.FormattedSecondaryStat.Value}");
                        Console.WriteLine($"Effect State: {agent.WeaponEffectState}");
                        Console.WriteLine($"Overclock: {agent.Weapon.UpgradeLevel}");
                        Console.WriteLine($"Icon URL: {agent.Weapon.ImageUrl}");
                    }

                    var discSets = agent.GetEquippedDiscSets();
                    if (discSets.Count > 0)
                    {
                        Console.WriteLine("\nDRIVE DISC SETS:");
                        foreach (var set in discSets)
                        {
                            Console.WriteLine($"- {set.SuitName} ({set.PieceCount} pieces)");
                            foreach (var bonus in set.BonusStats)
                            {
                                Console.WriteLine($"  - {bonus.Key}: {bonus.Value}");
                            }
                        }
                    }

                    if (agent.EquippedDiscs.Count > 0)
                    {
                        Console.WriteLine("\nEQUIPPED DISCS:");
                        foreach (var disc in agent.EquippedDiscs)
                        {
                            Console.WriteLine($"- {disc.SuitName} (Lv.{disc.Level})");
                            Console.WriteLine($"  - Main Stats: {disc.FormattedMainStat.Key}: {disc.FormattedMainStat.Value}");
                            Console.WriteLine($"  - Rarity: {disc.Rarity}");
                            Console.WriteLine($"  - Slot: {disc.Slot}");
                            Console.WriteLine($"  - Icon URL: {disc.IconUrl}");
                            Console.WriteLine($"  - Locked: {disc.IsLocked}");
                            Console.WriteLine($"  - Available: {disc.IsAvailable}");
                            Console.WriteLine($"  - Trash: {disc.IsTrash}");

                            foreach (var statPair in disc.SubStats)
                            {
                                Console.WriteLine($"  - {statPair.Key}: {statPair.Value}");
                            }
                        }
                    }

                    if (agent.CoreSkillEnhancements.Count > 0)
                    {
                        Console.WriteLine("\nCORE SKILLS:");
                        foreach (var skill in agent.CoreSkillEnhancements)
                        {
                            if (agent.CoreSkillEnhancements.Count > 0)
                            {
                                char skillLetter = (char)('A' + skill);
                                Console.WriteLine($"- {skillLetter} ({skill})");
                            }
                        }
                    }

                    Console.WriteLine("\nSKILL LEVELS:");
                    foreach (var skill in agent.SkillLevels)
                    {
                        Console.WriteLine($"- {skill.Key}: {skill.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
```

## Example Code for Honkai Star Rail

```csharp
using EnkaDotNet;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Utils.HSR;

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
                    GameType = EnkaDotNet.Enums.GameType.HSR,
                    Language = "ja",
                    EnableCaching = true,
                    UserAgent = "EnkaDotNet/5.0",
                    Raw = false,
                };

                using var client = new EnkaClient(options);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 802175546;

                Console.WriteLine($"Fetching player data for UID: {uid}...");
                var playerInfo = await client.GetHSRPlayerInfo(uid);

                Console.WriteLine($"Player: {playerInfo.Nickname} (Lv.{playerInfo.Level}, World Level: {playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Platform: {playerInfo.Platform}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");

                Console.WriteLine("\nRECORD INFO:");
                Console.WriteLine($"Achievements: {playerInfo.RecordInfo.AchievementCount}");
                Console.WriteLine($"Characters: {playerInfo.RecordInfo.AvatarCount}");
                Console.WriteLine($"Light Cones: {playerInfo.RecordInfo.LightConeCount}");
                Console.WriteLine($"Relics: {playerInfo.RecordInfo.RelicCount}");
                Console.WriteLine($"Memory of Chaos Score: {playerInfo.RecordInfo.MemoryOfChaosScore}");

                if (playerInfo.DisplayedCharacters.Count == 0)
                {
                    Console.WriteLine("\nNo characters found in showcase.");
                    return;
                }

                Console.WriteLine("\nDISPLAYED CHARACTERS:");
                foreach (var character in playerInfo.DisplayedCharacters)
                {
                    Console.WriteLine("\n" + new string('=', 60));
                    Console.WriteLine($"CHARACTER INFO:");
                    Console.WriteLine($"Name: {character.Name} (Lv.{character.Level}/{character.Promotion})");
                    Console.WriteLine($"Rarity: {character.Rarity} | Path: {character.Path.GetPathName()}");
                    Console.WriteLine($"Element: {character.Element}");
                    Console.WriteLine($"Rank: {character.Rank} | Position: {character.Position}");
                    Console.WriteLine($"Icon URL: {character.IconUrl}");
                    Console.WriteLine($"Avatar Icon URL: {character.AvatarIconUrl}");

                    Console.WriteLine("\nSTATS:");
                    foreach (var stat in character.GetAllStats())
                    {
                        Console.WriteLine($"{stat.Key}: {stat.Value}");
                    }

                    if (character.Equipment != null)
                    {
                        var lightCone = character.Equipment;
                        Console.WriteLine("\nLIGHT CONE:");
                        Console.WriteLine($"Name: {lightCone.Name} (Lv.{lightCone.Level}/{lightCone.Promotion})");
                        Console.WriteLine($"Rarity: {lightCone.Rarity} | Path: {lightCone.Path}");
                        Console.WriteLine($"Superimposition: {lightCone.Rank}");
                        Console.WriteLine($"Base HP: {lightCone.BaseHP:F0}");
                        Console.WriteLine($"Base ATK: {lightCone.BaseAttack:F0}");
                        Console.WriteLine($"Base DEF: {lightCone.BaseDefense:F0}");
                        Console.WriteLine($"Icon URL: {lightCone.IconUrl}");
                    }

                    Console.WriteLine("\nEIDOLONS:");
                    foreach (var eidolon in character.Eidolons)
                    {
                        Console.WriteLine($"- Id: {eidolon.Id}");
                        Console.WriteLine($"- Icon URL: {eidolon.Icon}");
                        Console.WriteLine($"- Unlocked: {eidolon.Unlocked}");
                    }

                    if (character.RelicList.Count > 0)
                    {
                        Console.WriteLine("\nRELICS:");
                        foreach (var relic in character.RelicList)
                        {
                            Console.WriteLine($"- {relic.SetName} ({relic.RelicType.GetRelicName()}) - Lv.{relic.Level}");
                            Console.WriteLine($"  Rarity: {relic.Rarity}");
                            Console.WriteLine($"  Main Stat: {relic.MainStat}");
                            Console.WriteLine($"  Icon URL: {relic.IconUrl}");

                            Console.WriteLine("  Sub Stats:");
                            foreach (var subStat in relic.SubStats)
                            {
                                Console.WriteLine($"  - {HSRStatPropertyUtils.GetDisplayName(subStat.Type)}: {subStat.DisplayValue}");
                            }
                        }
                    }

                    var relicSets = character.GetEquippedRelicSets();
                    if (relicSets.Count > 0)
                    {
                        Console.WriteLine("\nRELIC SET BONUSES:");
                        foreach (var setBonus in relicSets)
                        {
                            Console.WriteLine($"    - {setBonus.SetName} ({setBonus.PieceCount}pcs)");
                            foreach (var effectDetail in setBonus.Effects)
                            {
                                Console.WriteLine($"      - {effectDetail.PropertyName}: +{effectDetail.FormattedValue}");
                            }
                        }
                    }

                    Console.WriteLine("\nSKILL TREES:");
                    if (character.SkillTreeList != null && character.SkillTreeList.Count > 0)
                    {
                        foreach (HSRSkillTree trace in character.SkillTreeList)
                        {
                            Console.WriteLine($"Type: {trace.TraceType}");
                            Console.WriteLine($"Icon URL: {trace.Icon}");
                            Console.WriteLine($"Anchor: {trace.Anchor}");

                            string levelDisplay;
                            if (trace.IsBoosted)
                            {
                                levelDisplay = $"{trace.BaseLevel} + {trace.Level - trace.BaseLevel} = {trace.Level}";
                            }
                            else
                            {
                                levelDisplay = $"{trace.Level}";
                            }
                            Console.WriteLine($"Level: {levelDisplay} / {trace.MaxLevel}");
                            Console.WriteLine($"Boosted by Eidolon: {trace.IsBoosted}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{character.Name} has no skill tree information available.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
            }
        }
    }
}
```

## Requirements  

- .NET Standard 2.0

## Support

Having questions or issues? Join our Discord server: [Alg's Dev Env](https://discord.gg/d4UgxagmwF)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Enka.Network](https://enka.network/) for providing the API
- [seriaati](https://github.com/seriaati) for the inspiration

---

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (miHoYo) or Enka.Network. Genshin Impact is a trademark of HoYoverse.