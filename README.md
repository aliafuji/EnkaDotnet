﻿# Enka.DotNet

Enka.DotNet is a C# wrapper for accessing and processing character data from the Enka.Network API. It provides a simple interface to retrieve detailed information about characters, artifacts, weapons, and player profiles for Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)

## Features

  * **Comprehensive Data Retrieval (Non DI & DI):**
      * Fetch detailed Genshin Impact character builds including artifacts, weapons, stats, and constellations.
      * Access Genshin Impact player profile information, including namecards and achievements.
      * Retrieve comprehensive Zenless Zone Zero agent details, including stats, W-Engines (weapons), and Drive Discs.
      * View detailed Honkai: Star Rail character stats, Relics, Light Cones, Traces (Skill Trees), and Eidolons.
  * **Multi-Game Support:** Seamlessly switch between Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero by configuring the client.
  * **Strongly-Typed Models:** Provides clear and easy to use C# models for all game entities.
  * **Flexible Client Instantiation:**
      * **Non DI:** Easy setup using the `EnkaClient.CreateAsync()` factory method.
      * **DI:** Full support for .NET Dependency Injection via `AddEnkaNetClient()` for integration into ASP.NET Core, Worker Services, etc.
  * **Asset Management:** Handles fetching and caching of necessary game assets (text maps, item details).
  * **Configurable Caching:** Built-in support for caching API responses with options to control duration, bypass, and clear the cache. [cite: 1, 2, 3, 4]
  * **Customizable Options:** Control aspects like language, user agent, and API request behavior. [cite: 3]

## Supported Games

| Game                | Status   | API Support |
| :------------------ | :------- | :---------- |
| Genshin Impact      | ✅ Ready | Full        |
| Honkai: Star Rail   | ✅ Ready | Full        |
| Zenless Zone Zero   | ✅ Ready | Full        |

## Installation

Install EnkaDotNet via NuGet package manager:

```bash
Install-Package EnkaDotNet
```

Or via the .NET CLI:

```bash
dotnet add package EnkaDotNet
```

## Usage & Examples

Enka.DotNet supports both direct instantiation (Non-DI) for simpler applications and Dependency Injection (DI) for more complex setups like ASP.NET Core or Worker Services.

### Key Concepts:

  * **`EnkaClientOptions`**: Use this class to configure the `GameType` (Genshin, HSR, ZZZ), `Language`, caching behavior, and other settings. [cite: 3]
  * **Non DI Usage**: Instantiate the client using the static factory method `await EnkaClient.CreateAsync(options)`. This method handles asynchronous initialization of game assets. [cite: 4]
  * **Dependency Injection Usage**: Register the client in your service collection using the `services.AddEnkaNetClient(options => { ... });` extension method. Then, inject `IEnkaClient` into your services. [cite: 5]

### Detailed Code Examples

For detailed and runnable code examples demonstrating how to use Enka.DotNet for Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero, please refer to the `Examples/` directory within this repository.

The `Examples/` folder contains separate projects for each game, showcasing:

  * **Non DI (Direct Instantiation)**: Basic console applications showing how to set up and use the client directly.
  * **DI (Dependency Injection)**: Examples using the .NET Generic Host to demonstrate DI setup and usage.

These examples cover fetching player profiles, character/agent details, equipment, stats, and more.

## Controlling the Cache

Enka.DotNet provides several ways to control caching behavior for API responses. [cite: 1, 2, 3, 4]

### 1\. Configuration via `EnkaClientOptions` [cite: 3]

When creating an `EnkaClient` instance (either directly or via DI configuration), you can set the following options:

  * **`EnableCaching`**: A boolean (default `true`) to enable or disable caching entirely. If set to `false`, no responses will be cached, and every request will hit the Enka.Network API.
  * **`CacheDurationMinutes`**: An integer (default `5`) specifying how long responses should be cached in minutes.

**Example (Non DI):**

```csharp
var options = new EnkaClientOptions
{
    GameType = GameType.Genshin,
    Language = "en",
    EnableCaching = true,                 // Default is true
    CacheDurationMinutes = 10,            // Cache responses for 10 minutes
    UserAgent = "MyApp/1.0"
};
await using IEnkaClient client = await EnkaClient.CreateAsync(options);
```

**Example (DI in `Program.cs`):**

```csharp
builder.Services.AddEnkaNetClient(options =>
{
    options.GameType = GameType.Genshin;
    options.Language = "en";
    options.EnableCaching = false; // Disable caching
    // options.CacheDurationMinutes will be ignored if EnableCaching is false
});
```

### 2\. Runtime Cache Control via `IEnkaClient`

#### Bypassing Cache for Specific Requests [cite: 2, 4]

All data fetching methods on `IEnkaClient` (e.g., `GetUserProfileAsync`, `GetHSRPlayerInfoAsync`, `GetZZZAgentsAsync`) accept an optional `bypassCache` boolean parameter. Setting this to `true` for a specific call will force the client to fetch fresh data from the API, ignoring any existing cached response for that particular UID.

**Example:**

```csharp
// Assuming client is an initialized IEnkaClient instance
int uid = 8000000;

// This call will use the cache if available and not expired
var (playerInfoCached, charactersCached) = await client.GetUserProfileAsync(uid);

// This call will always fetch fresh data from the API
var (playerInfoFresh, charactersFresh) = await client.GetUserProfileAsync(uid, bypassCache: true);
```

#### Clearing the Entire Cache [cite: 2, 4]

You can programmatically clear all cached responses held by an `EnkaClient` instance.

**Example:**

```csharp
// Assuming client is an initialized IEnkaClient instance
client.ClearCache();
Console.WriteLine("All Enka.DotNet cache entries have been cleared.");
```

#### Getting Cache Statistics [cite: 2, 4]

You can retrieve statistics about the current state of the cache.

**Example:**

```csharp
// Assuming client is an initialized IEnkaClient instance
var (currentEntryCount, expiredCountInfo) = client.GetCacheStats();
Console.WriteLine($"Current items in cache: {currentEntryCount}");
// Note: ExpiredCountNotAvailable indicates if the detailed count of expired items (before they arecompacted) is available.
```

## Requirements

  * .NET Standard 2.0 compatible framework (e.g., .NET Core 2.0+, .NET Framework 4.6.1+, .NET 5+)

## Support

Having questions or issues? Join our Discord server: [Alg's Dev Env](https://discord.gg/d4UgxagmwF)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

  * [Enka.Network](https://enka.network/) for providing the API
  * [seriaati](https://github.com/seriaati) for the inspiration

-----

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (COGNOSPHERE PTE. LTD.) or Enka.Network. Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero are trademarks of HoYoverse.