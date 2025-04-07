# Enka.DotNet

Enka.DotNet is a wrapper for accessing and processing character data from the Enka.Network API. It provides a simple interface to retrieve detailed information about Genshin Impact characters, artifacts, weapons, and player profiles.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)

## Features

- Fetch detailed character builds including artifacts, weapons, and stats
- Access player profile information
- Strong typing for all Genshin Impact game entities

## Supported Games

| Game              | Status         | API Support |
| ----------------- | -------------- | ----------- |
| Genshin Impact    | ✅ Ready       | Full        |
| Honkai: Star Rail | ⏳ Coming Soon | Planned     |
| Zenless Zone Zero | ⏳ Coming Soon | Planned     |

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

## Requirements

- .NET 8.0 or higher

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Enka.Network](https://enka.network/) for providing the API
- [seriaati](https://github.com/seriaati) for the inspiration

---

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (miHoYo) or Enka.Network. Genshin Impact is a trademark of HoYoverse.
