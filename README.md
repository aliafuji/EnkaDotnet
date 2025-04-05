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
using EnkaDotNet.Components.Genshin;
using System;
using System.Threading.Tasks;

// Initialize the client with asset path
var client = new EnkaClient("enka_assets");

// Get a player's profile including character builds
async Task GetPlayerProfile()
{
    try
    {
        int uid = 829344442; // Replace with the UID you want to look up
        var (playerInfo, characters) = await client.GetUserProfile(uid);

        Console.WriteLine($"Player: {playerInfo.Nickname} (AR {playerInfo.Level})");
        Console.WriteLine($"Characters: {characters.Count}");

        foreach (var character in characters)
        {
            Console.WriteLine($"- {character.Name} (Lvl {character.Level})");
            Console.WriteLine($"  Element: {character.Element}");
            Console.WriteLine($"  Weapon: {character.Weapon?.Name} (R{character.Weapon?.Refinement})");

            foreach (var artifact in character.Artifacts)
            {
                Console.WriteLine($"  {artifact.Slot}: {artifact.Name} - {artifact.MainStat}");
            }
        }
    }
    catch (EnkaDotNet.Exceptions.PlayerNotFoundException ex)
    {
        Console.WriteLine($"Player not found: {ex.Message}");
    }
    catch (EnkaDotNet.Exceptions.ProfilePrivateException ex)
    {
        Console.WriteLine($"Profile is private: {ex.Message}");
    }
    catch (EnkaDotNet.Exceptions.EnkaNetworkException ex)
    {
        Console.WriteLine($"API error: {ex.Message}");
    }
}

await GetPlayerProfile();

client.Dispose();
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

## Error Handling

EnkaSharp provides specific exception types to handle different scenarios:

- `PlayerNotFoundException`: The UID does not exist
- `ProfilePrivateException`: The profile exists but character details are hidden
- `EnkaNetworkException`: Base exception type for network and API errors

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
