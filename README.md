# Enka.DotNet  

Enka.DotNet is a wrapper for accessing and processing character data from the Enka.Network API. It provides a simple interface to retrieve detailed information about characters, artifacts, weapons, and player profiles for Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)

## Features  

- Fetch detailed character builds including artifacts, weapons, stats, and constellations  
- Access player profile information, including namecards and achievements  
- Strong typing for all Genshin Impact game entities
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

## Important Notice  

If you encounter the following error: Please ensure you have the correct version of Newtonsoft.Json installed or Could not load file or assembly 'Newtonsoft.Json'... 
You can resolve this by running:
```
Install-Package Newtonsoft.Json -Version 13.0.3
```
Alternatively, update the package to the latest version if it is already installed.

## Example Code for Genshin Impact

```csharp
using EnkaDotNet;
using EnkaDotNet.Enums.Genshin;

var options = new EnkaClientOptions
{
    Language = "ja",
    UserAgent = "EnkaDotNet/1.5",
};

using var client = new EnkaClient(options);

try
{
    int uid = 829344442;

    Console.OutputEncoding = System.Text.Encoding.UTF8;

    Console.WriteLine($"Fetching profile for UID {uid}...");

    var (player, characters) = await client.GetUserProfile(uid);

    Console.WriteLine($"\nPlayer: {player.Nickname} (AR {player.Level})");
    Console.WriteLine($"World Level: {player.WorldLevel}");
    Console.WriteLine($"Signature: {player.Signature}");
    Console.WriteLine($"IconUrl: {player.IconUrl}");
    Console.WriteLine($"NameCard: {player.NameCardIcon} | {player.NameCardId}");
    Console.WriteLine($"Characters in profile: {characters.Count}");

    // Print Abyss data
    Console.WriteLine("\nAbyss data:");
    Console.WriteLine($"Spiral Abyss Floor: {player.Challenge?.SpiralAbyss?.Floor}");
    Console.WriteLine($"Spiral Abyss Chamber: {player.Challenge?.SpiralAbyss?.Chamber}");
    Console.WriteLine($"Spiral Abyss Stars: {player.Challenge?.SpiralAbyss?.Star}");

    // Print Theater data
    Console.WriteLine("\nTheater data:");
    Console.WriteLine($"Theater Mechanicus Act: {player.Challenge?.Theater?.Act}");
    Console.WriteLine($"Theater Mechanicus Stars: {player.Challenge?.Theater?.Star}");

    // Print Namecards
    Console.WriteLine("\nNamecards Showcase:");
    foreach (var namecard in player.ShowcaseNameCards)
    {
        Console.WriteLine($"Icon: {namecard.IconUrl} | ID: {namecard.Id}");
    }

    Console.WriteLine("\nCharacter Details:");
    foreach (var character in characters)
    {
        Console.WriteLine($"\n{character.Name} | Level {character.Level} | C{character.ConstellationLevel}");

        if (character.Weapon != null)
        {
            Console.WriteLine($"Weapon: {character.Weapon.Name} | R{character.Weapon.Refinement} | Level {character.Weapon.Level}");
            Console.WriteLine($"Base ATK: {character.Weapon.BaseAttack} | Substat: {character.Weapon.SecondaryStat?.Value} | Type: {character.Weapon.SecondaryStat?.Type}");
            Console.WriteLine($"Icon: {character.Weapon.IconUrl}");
        }

        // Print all stats
        Console.WriteLine("\nStats:");
        var stats = character.GetStats();
        foreach (var category in stats)
        {
            Console.WriteLine($"{category.Key}:");
            foreach (var stat in category.Value)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value.Formatted} ({stat.Value.Raw})");
            }
        }

        // Print artifact sets
        Console.WriteLine("\nArtifact Sets:");
        var artifactSets = character.Artifacts
            .GroupBy(a => a.SetName)
            .Select(g => new { SetName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        foreach (var set in artifactSets)
        {
            Console.WriteLine($"Set: {set.SetName} ({set.Count}pc)");
        }

        // Print all artifacts
        Console.WriteLine("\nArtifacts:");
        foreach (var artifact in character.Artifacts)
        {
            Console.WriteLine($"Artifact: {artifact.Name} | Level {artifact.Level} | Rarity {artifact.Rarity} | Set {artifact.SetName}");
            Console.WriteLine($"Main Stat: {artifact.MainStat?.Type} | {artifact.MainStat?.Value}");
            Console.WriteLine($"Substats: {string.Join(", ", artifact.SubStats.Select(s => $"{s.Type}: {s.Value}"))}");
            Console.WriteLine($"Icon: {artifact.IconUrl}");
        }

        // Print specific stats
        Console.WriteLine("\nSpecific Stats:");
        character.GetStatValue(StatType.BaseAttack, out var value);
        Console.WriteLine($"Base Attack: {value}");
        character.GetStatValue(StatType.BaseHP, out value);
        Console.WriteLine($"Base HP: {value}");
        character.GetStatValue(StatType.BaseDefense, out value);
        Console.WriteLine($"Base Defense: {value}");
        character.GetStatValue(StatType.CriticalRate, out value);
        Console.WriteLine($"Critical Rate: {value}");
        character.GetStatValue(StatType.CriticalDamage, out value);
        Console.WriteLine($"Critical Damage: {value}");

        // Constellation data
        Console.WriteLine("\nConstellations:");
        foreach (var constellation in character.Constellations)
        {
            Console.WriteLine($"Name: {constellation.Name}");
            Console.WriteLine($"Icon: {constellation.IconUrl}");
            Console.WriteLine($"Position: {constellation.Position}");
            Console.WriteLine($"ID: {constellation.Id}");
        }

    }
}
catch (Exception ex) when (
        ex is EnkaDotNet.Exceptions.PlayerNotFoundException ||
        ex is EnkaDotNet.Exceptions.ProfilePrivateException ||
        ex is EnkaDotNet.Exceptions.EnkaNetworkException)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Example Code for Zenless Zone Zero

```csharp
using EnkaDotNet;
using EnkaDotNet.Enums;

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
                    GameType = GameType.ZZZ,
                    Language = "en",
                    EnableCaching = true,
                    UserAgent = "EnkaDotNet/5.0",
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

                // Display medals
                Console.WriteLine("\nMEDALS:");
                foreach (var medal in playerInfo.Medals)
                {
                    Console.WriteLine($"Name: {medal.Name} (Lv.{medal.Value})");
                    Console.WriteLine($"Icon: {medal.Icon}");
                    Console.WriteLine($"Type: {medal.Type}");
                }

                // Display agent's info
                var agent = playerInfo.ShowcaseAgents[0];
                Console.WriteLine($"\nAGENT: {agent.Name} (Lv.{agent.Level})");
                Console.WriteLine($"Rarity: {agent.Rarity} | Profession: {agent.ProfessionType}");
                Console.WriteLine($"Elements: {string.Join(", ", agent.ElementTypes)}");

                // Display Mindscapes
                Console.WriteLine($"Mindscapes: M{agent.TalentLevel}");

                // Display agent's image URL
                Console.WriteLine($"Image URL: {agent.ImageUrl}");
                Console.WriteLine($"Circle Icon URL: {agent.CircleIconUrl}");
                Console.WriteLine($"Promotion Level: {agent.PromotionLevel}");

                // Display agent's obtained timestamp
                Console.WriteLine($"Obtained Time: {agent.ObtainmentTimestamp.ToLocalTime()}");

                // Display agent's weapon effect state
                Console.WriteLine($"Weapon Effect State: {agent.WeaponEffectState}");

                // Display agent's stats
                Console.WriteLine("\nAGENT STATS:");
                var stats = agent.GetAllStats();
                foreach (var stat in stats)
                {
                    Console.WriteLine($"{stat.Key}: {stat.Value}");
                }

                // Display weapon
                if (agent.Weapon != null)
                {
                    Console.WriteLine("\nW-ENGINE:");
                    Console.WriteLine($"Name: {agent.Weapon.Name} (Lv.{agent.Weapon.Level}/{agent.Weapon.BreakLevel})");
                    Console.WriteLine($"Rarity: {agent.Weapon.Rarity}");
                    Console.WriteLine($"Main Stat: {agent.Weapon.MainStat}");
                    Console.WriteLine($"Secondary Stat: {agent.Weapon.SecondaryStat}");
                    Console.WriteLine($"Effect State: {agent.WeaponEffectState}");
                    
                    Console.WriteLine($"Overclock: {agent.Weapon.UpgradeLevel}");
                    Console.WriteLine($"Icon URL: {agent.Weapon.ImageUrl}");
                }

                // Display equipped disc sets
                var discSets = agent.GetEquippedDiscSets();
                if (discSets.Count > 0)
                {
                    Console.WriteLine("\nDRIVE DISC SETS:");
                    foreach (var set in discSets)
                    {
                        Console.WriteLine($"- {set.SuitName} ({set.PieceCount} pieces)");
                        foreach (var disc in set.BonusStats)
                        {
                            Console.WriteLine($"  - {disc.StatType}: {disc.Value}");
                        }
                    }
                }

                // Display equipped discs
                if (agent.EquippedDiscs.Count > 0)
                {
                    Console.WriteLine("\nEQUIPPED DISCS:");
                    foreach (var disc in agent.EquippedDiscs)
                    {
                        Console.WriteLine($"- {disc.SuitName} (Lv.{disc.Level})");
                        Console.WriteLine($"  - Main Stats: {disc.MainStat}");
                        Console.WriteLine($"  - Rarity: {disc.Rarity}");
                        Console.WriteLine($"  - Slot: {disc.Slot}");
                        Console.WriteLine($"  - Icon URL: {disc.IconUrl}");
                        Console.WriteLine($"  - Locked: {disc.IsLocked}");
                        Console.WriteLine($"  - Available: {disc.IsAvailable}");
                        Console.WriteLine($"  - Trash: {disc.IsTrash}");

                        foreach (var stat in disc.SubStats)
                        {
                            if (stat.IsPercentage)
                            {
                                Console.WriteLine($"  - {stat.Type}: {stat.Value * stat.Level:F1}% +{stat.Level}");
                            }
                            else
                            {
                                Console.WriteLine($"  - {stat.Type}: {(int)stat.Value * stat.Level} +{stat.Level}");
                            }
                        }
                    }
                }

                // Display core skills
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

                // Display skill levels
                Console.WriteLine("\nSKILL LEVELS:");
                foreach (var skill in agent.SkillLevels)
                {
                    Console.WriteLine($"- {skill.Key}: {skill.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
```

## Example Code for Honkai Star Rail

```csharp
using EnkaDotNet;
using EnkaDotNet.Enums;
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
                    GameType = GameType.HSR,
                    Language = "en",
                    EnableCaching = true,
                    UserAgent = "EnkaDotNet/5.0",
                };

                using var client = new EnkaClient(options);

                int uid = args.Length > 0 && int.TryParse(args[0], out int parsedUid) ? parsedUid : 802175546;

                Console.WriteLine($"Fetching player data for UID: {uid}...");
                var playerInfo = await client.GetHSRPlayerInfo(uid);

                Console.WriteLine($"Player: {playerInfo.Nickname} (Lv.{playerInfo.Level}, World Level: {playerInfo.WorldLevel})");
                Console.WriteLine($"Signature: {playerInfo.Signature}");
                Console.WriteLine($"Platform: {playerInfo.Platform}");
                Console.WriteLine($"Player Icon: {playerInfo.ProfilePictureIcon}");

                // Display record info
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

                // Display character info for first character
                var character = playerInfo.DisplayedCharacters[6];
                Console.WriteLine($"\nCHARACTER INFO:");
                Console.WriteLine($"Name: {character.Name} (Lv.{character.Level}/{character.Promotion})");
                Console.WriteLine($"Rarity: {character.Rarity} | Path: {character.Path.GetPathName()}");
                Console.WriteLine($"Element: {character.Element}");
                Console.WriteLine($"Rank: {character.Rank} | Position: {character.Position}");
                Console.WriteLine($"Icon URL: {character.IconUrl}");
                Console.WriteLine($"Avatar Icon URL: {character.AvatarIconUrl}");

                // Display character stats
                Console.WriteLine("\nSTATS:");
                foreach (var stat in character.Stats)
                {
                    Console.WriteLine($"{stat.Key}: {stat.Value}");
                }

                // Display light cone
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

                // Display eidolons
                Console.WriteLine("\nEIDOLONS:");
                foreach (var eidolon in character.Eidolons)
                {
                    Console.WriteLine($"- Id: {eidolon.Id}");
                    Console.WriteLine($"- Icon URL: {eidolon.Icon}");
                    Console.WriteLine($"- Unlocked: {eidolon.Unlocked}");
                }

                // Display relics
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

                // Display relic sets
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

                // Display skill trees
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