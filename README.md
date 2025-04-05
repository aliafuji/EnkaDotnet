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

## Quick Start

```csharp
using EnkaDotNet;

async Task DisplayPlayerInfo(int uid)
{
    using var client = new EnkaClient();
    
    try
    {
        // Fetch player profile
        var (player, characters) = await client.GetUserProfile(uid);
        
        // Display basic player info
        Console.WriteLine($"Player: {player.Nickname} (AR {player.Level})");
        Console.WriteLine($"Characters: {characters.Count}");
        
        // Display character details
        foreach (var char in characters)
        {
            Console.WriteLine($"- {char.Name} (Lvl {char.Level}) | {char.Element}");
            Console.WriteLine($"  Weapon: {char.Weapon?.Name} (R{char.Weapon?.Refinement})");
            
            foreach (var artifact in char.Artifacts)
            {
                Console.WriteLine($"  {artifact.Slot}: {artifact.Name} - {artifact.MainStat}");
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
}

await DisplayPlayerInfo(829344442);
```

### Working with Artifacts and Stats

```csharp
// Get artifacts for a character
var artifacts = character.Artifacts;

// Get artifact set bonuses (if any)
var setNames = artifacts.GroupBy(a => a.SetName)
    .Select(g => new { SetName = g.Key, Count = g.Count() })
    .Where(s => s.Count >= 2)
    .ToList();

// Get critical stats
double critRate = character.GetStatValue(StatType.CriticalRate);
double critDmg = character.GetStatValue(StatType.CriticalDamage);
Console.WriteLine($"Crit Ratio: {critRate:P1} / {critDmg:P1}");
```

## Requirements

- .NET 8.0 or higher

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Enka.Network](https://enka.network/) for providing the API
- [seriaati](https://github.com/seriaati) for contributions and inspiration

---

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (miHoYo) or Enka.Network. Genshin Impact is a trademark of HoYoverse.
