# Enka.DotNet

C# wrapper for the [Enka.Network](https://enka.network/) API. Fetch player profiles, characters, artifacts, weapons, and builds for Genshin Impact, Honkai: Star Rail, Zenless Zone Zero, and Arknights: Endfield.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=aliafuji_EnkaDotnet&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=aliafuji_EnkaDotnet)

## Features

* Multi-game support: Genshin Impact, Honkai: Star Rail, Zenless Zone Zero, and Arknights: Endfield
* Strongly typed models for player, character, and equipment data
* Direct client creation or dependency injection
* Polly retries with exponential backoff and jitter, circuit breaker, and 429 `Retry-After` handling
* Caching: in-memory by default, SQLite and Redis as separate packages
* Asset preloading at startup
* OpenTelemetry `ActivitySource` and `System.Diagnostics.Metrics` hooks

## Supported Games

| Game              | Status | Method |
|-------------------|--------|--------|
| Genshin Impact       | Ready  | UID (`int`)  |
| Honkai: Star Rail    | Ready  | UID (`int`)  |
| Zenless Zone Zero    | Ready  | UID (`int`)  |
| Arknights: Endfield  | Ready  | UID (`long`) |

## Enka Profile Features

| Feature             | Status | Method        |
|---------------------|--------|---------------|
| Fetch Basic Profile | Ready  | Enka Username |
| Genshin Impact      | Ready  | Enka Username |
| Honkai: Star Rail   | Ready  | Enka Username |
| Zenless Zone Zero   | Ready  | Enka Username |

## Installation

Core package (includes the in-memory cache):

```bash
dotnet add package EnkaDotNet
```

SQLite and Redis are optional. They are separate packages so the core package does not pull in `Microsoft.Data.Sqlite` or `StackExchange.Redis` for apps that only need the default memory cache.

```bash
dotnet add package EnkaDotNet.Caching.Sqlite
dotnet add package EnkaDotNet.Caching.Redis
```

| Package                      | Purpose                                      |
|------------------------------|----------------------------------------------|
| `EnkaDotNet`                 | Client, models, assets, memory cache         |
| `EnkaDotNet.Caching.Sqlite`  | Persistent SQLite cache                      |
| `EnkaDotNet.Caching.Redis`   | Distributed Redis cache                      |

## Migrating from 1.x to 2.0

2.0 is a breaking release. There are no obsolete aliases. Full details are in [CHANGELOG.md](CHANGELOG.md). The changes that affect most users:

1. **Cache packages.** Setting `CacheProvider.SQLite` or `CacheProvider.Redis` alone is no longer enough. Install the matching package and call `UseSqliteCache` / `AddEnkaSqliteCache` (or the Redis equivalents).
2. **`EnkaClientOptions.Raw`** is now **`UseRawStatValues`**.
3. **HSR `RelicType`** members are PascalCase. `NECK` is `PlanarSphere`, `OBJECT` is `LinkRope`. Numeric values are unchanged.
4. **Public constants** in `Constants` are PascalCase (`DefaultGenshinApiUrl`, and so on).
5. **ZZZ models** `StatSummary`, `FormattedStatValues`, and `Skin` moved from `EnkaDotNet.Enums.ZZZ` to `EnkaDotNet.Components.ZZZ`.

## Quick Start

### Direct instantiation

```csharp
using EnkaDotNet;
using EnkaDotNet.Enums;

var options = new EnkaClientOptions
{
    EnableCaching = true,
    CacheDurationMinutes = 10,
    UserAgent = "MyApp/1.0"
};

await using IEnkaClient client = await EnkaClient.CreateAsync(options);

int uid = 800000000; // replace with a real UID
var (player, characters) = await client.GetGenshinUserProfileAsync(uid, Language.English);

Console.WriteLine($"{player.Nickname} (Lv.{player.Level})");
foreach (var character in characters)
{
    Console.WriteLine($"  {character.Name} Lv.{character.Level}");
}
```

### Dependency injection

```csharp
using EnkaDotNet;
using EnkaDotNet.DIExtensions;
using EnkaDotNet.Enums;

builder.Services.AddEnkaNetClient(options =>
{
    options.EnableCaching = true;
    options.CacheDurationMinutes = 10;
    options.UserAgent = "MyApp/1.0";
    options.PreloadedLanguages = new List<Language> { Language.English, Language.Japanese };
});
```

Inject `IEnkaClient` into your services afterward.

## Examples by Game

Replace the sample UIDs with real ones. Profiles that are private or have no showcase characters return empty character lists.

### Genshin Impact

```csharp
var (player, characters) = await client.GetGenshinUserProfileAsync(uid, Language.English);

Console.WriteLine($"{player.Nickname} WL{player.WorldLevel}");
Console.WriteLine(player.Signature);

foreach (var character in characters)
{
    Console.WriteLine($"{character.Name} Lv.{character.Level} C{character.ConstellationLevel}");
    if (character.Weapon != null)
    {
        Console.WriteLine($"  Weapon: {character.Weapon.Name} R{character.Weapon.Refinement}");
    }
}
```

### Honkai: Star Rail

```csharp
var player = await client.GetHSRPlayerInfoAsync(uid, Language.English);
var characters = await client.GetHSRCharactersAsync(uid, Language.English);

Console.WriteLine($"{player.Nickname} (Lv.{player.Level})");

foreach (var character in characters)
{
    Console.WriteLine($"{character.Name} Lv.{character.Level}");
    foreach (var relic in character.RelicList)
    {
        Console.WriteLine($"  {relic.RelicType}: {relic.SetName}");
    }
}
```

### Zenless Zone Zero

```csharp
var player = await client.GetZZZPlayerInfoAsync(uid, Language.English);
var agents = await client.GetZZZAgentsAsync(uid, Language.English);

Console.WriteLine($"{player.Nickname} (Lv.{player.Level})");

foreach (var agent in agents)
{
    Console.WriteLine($"{agent.Name} Lv.{agent.Level}");
}
```

### Arknights: Endfield

Endfield UIDs can exceed `int.MaxValue`, so these APIs take `long`.

```csharp
long uid = 4228833345;
var player = await client.GetEFPlayerInfoAsync(uid, Language.English);
var operators = await client.GetEFOperatorsAsync(uid, Language.English);

Console.WriteLine($"{player.Nickname} AL{player.AdminLevel} EL{player.EndfieldLevel}");
Console.WriteLine(player.Signature);

foreach (var op in operators)
{
    Console.WriteLine($"{op.Name} Lv.{op.Level}");
    Console.WriteLine($"  Splash: {op.SplashArtUrl}");
    Console.WriteLine($"  Silhouette: {op.SilhouetteUrl}");

    // Localized display names
    foreach (var stat in op.GetAllStats())
    {
        Console.WriteLine($"  {stat.Key}: {stat.Value}");
    }

    // Stable English keys for APIs / serialization
    foreach (var stat in op.GetFinalStats())
    {
        Console.WriteLine($"  {stat.Key}: {stat.Value}");
    }
}
```

### Enka profile and saved builds

```csharp
string username = "your_enka_username";

var profile = await client.GetEnkaProfileByUsernameAsync(username);
Console.WriteLine(profile.Username);

foreach (var account in profile.HoyoAccounts)
{
    Console.WriteLine($"{account.Nickname} ({account.Hash})");

    var genshinBuilds = await client.GetGenshinBuildsByUsernameAsync(username, account.Hash);
    var hsrBuilds = await client.GetHSRBuildsByUsernameAsync(username, account.Hash);
    var zzzBuilds = await client.GetZZZBuildsByUsernameAsync(username, account.Hash);
}
```

Runnable samples live under `Examples/` in the repository (Genshin, HSR, ZZZ, and Endfield, each with direct and DI variants where available).

## Caching

### Memory (default)

Built into the core package. No extra install.

```csharp
using EnkaDotNet.Caching;

var options = new EnkaClientOptions
{
    EnableCaching = true,
    CacheDurationMinutes = 10,
    CacheProvider = CacheProvider.Memory
};
```

### SQLite

Requires `EnkaDotNet.Caching.Sqlite`.

Direct:

```csharp
using EnkaDotNet.Caching;

var options = new EnkaClientOptions
{
    EnableCaching = true,
    CacheDurationMinutes = 10
};

options.UseSqliteCache(sqlite =>
{
    sqlite.DatabasePath = "enka_cache.db";
    sqlite.DefaultTtl = TimeSpan.FromMinutes(10);
});

await using var client = await EnkaClient.CreateAsync(options);
```

DI (call order does not matter):

```csharp
using EnkaDotNet.Caching;
using EnkaDotNet.DIExtensions;

builder.Services.AddEnkaNetClient(options =>
{
    options.CacheDurationMinutes = 60;
});

builder.Services.AddEnkaSqliteCache(sqlite =>
{
    sqlite.DatabasePath = "enka_cache.db";
});
```

### Redis

Requires `EnkaDotNet.Caching.Redis`.

Direct:

```csharp
using EnkaDotNet.Caching;

var options = new EnkaClientOptions
{
    EnableCaching = true
};

options.UseRedisCache(redis =>
{
    // remote servers: add ssl=true and a password
    redis.ConnectionString = "localhost:6379";
    redis.KeyPrefix = "myapp:enka:";
    redis.DefaultTtl = TimeSpan.FromMinutes(10);
});

await using var client = await EnkaClient.CreateAsync(options);
```

DI:

```csharp
builder.Services.AddEnkaNetClient();
builder.Services.AddEnkaRedisCache(redis =>
{
    redis.ConnectionString = "localhost:6379";
    redis.KeyPrefix = "myapp:enka:";
});
```

`KeyPrefix` is required and must not be empty. Clear and stats operations are scoped to that prefix so they do not touch other keys on the same Redis server.

Setting `CacheProvider = CacheProvider.SQLite` (or `Redis`) without installing the package and calling the extension throws an error that names the missing package.

### Runtime cache control

```csharp
var profile = await client.GetGenshinPlayerInfoAsync(uid, bypassCache: true);

client.ClearCache();

var (count, _) = client.GetCacheStats();
```

## Language

Game methods accept a `Language` enum. String codes still work.

```csharp
var zzz = await client.GetZZZPlayerInfoAsync(uid, Language.Japanese);
var hsr = await client.GetHSRPlayerInfoAsync(uid, Language.TraditionalChinese);
var gi = await client.GetGenshinPlayerInfoAsync(uid, Language.German);

var zzzAlt = await client.GetZZZPlayerInfoAsync(uid, language: "ja");
```

| Enum | Code |
|------|------|
| `Language.English` | `en` |
| `Language.Russian` | `ru` |
| `Language.Vietnamese` | `vi` |
| `Language.Thai` | `th` |
| `Language.Portuguese` | `pt` |
| `Language.Korean` | `ko` |
| `Language.Japanese` | `ja` |
| `Language.Indonesian` | `id` |
| `Language.French` | `fr` |
| `Language.Spanish` | `es` |
| `Language.German` | `de` |
| `Language.TraditionalChinese` | `zh-tw` |
| `Language.SimplifiedChinese` | `zh-cn` |
| `Language.Italian` | `it` |
| `Language.Turkish` | `tr` |

## HTTP Resiliency

```csharp
options.MaxRetries = 3;
options.RetryDelayMs = 1000;
options.UseExponentialBackoff = true;
options.MaxRetryDelayMs = 30000;
options.CircuitBreakerFailureThreshold = 5;
options.CircuitBreakerBreakDurationSeconds = 30;
```

`429 Too Many Requests` retries use the `Retry-After` header when present. A `RateLimitException` is thrown only after retries are exhausted.

## Observability

No extra package is required. Point your own OpenTelemetry exporters at the library source and meter:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("EnkaDotNet").AddOtlpExporter());

builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("EnkaDotNet").AddPrometheusExporter());
```

| Metric | Description | Tags |
|--------|-------------|------|
| `enka.requests.total` | Total API requests | `game` |
| `enka.cache.hits` | Cache hits | `game` |
| `enka.cache.misses` | Cache misses | `game` |
| `enka.retries.total` | Retry attempts | `game` |
| `enka.request.duration` | Request duration (ms) | `game` |
| `enka.errors.total` | Failed requests | `type`, `game`, `status` |

`game` is one of `genshin`, `hsr`, `zzz`, `endfield`, `profile`, or `unknown`.

`enka.errors.total` `type` values include `not_found`, `private`, `rate_limit`, `maintenance`, `circuit_open`, `timeout`, `canceled`, `http`, `parse`, `network`, and `unknown`. `status` is the HTTP status when known, otherwise `none`.

HTTP activities (`EnkaHttp.Get`) also set `enka.game`, `enka.cache.hit`, and `enka.uid_hash` (short hash of the UID — never the raw UID).

## Asset Preloading

Load game assets at startup so the first request does not wait on asset downloads.

```csharp
await using var client = await EnkaClient.CreateAsync(new EnkaClientOptions
{
    PreloadedLanguages = new List<Language> { Language.English, Language.Japanese }
});
```

With DI, the same option runs through an `IHostedService` and does not block the DI thread:

```csharp
builder.Services.AddEnkaNetClient(options =>
{
    options.PreloadedLanguages = new List<Language> { Language.English, Language.Japanese };
});
```

## Asset Fallback

Set `AssetFallbackDirectory` to keep a local copy of downloaded assets. Successful downloads are written there. If a later download fails, the saved file is used instead.

```csharp
options.AssetFallbackDirectory = "/path/to/enka_assets";
```

`null` (the default) means nothing is written to disk.

Pick a path your app owns. Do not write into the NuGet package folder under `~/.nuget/packages/`. Avoid `AppContext.BaseDirectory` in development builds; that folder is wiped on rebuild.

| App type | Suggested path |
|----------|----------------|
| ASP.NET Core / Worker | `Path.Combine(builder.Environment.ContentRootPath, "enka_assets")` |
| Console | `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnkaDotNet", "assets")` |
| Docker | Mount a volume and set the path explicitly |

```csharp
builder.Services.AddEnkaNetClient(options =>
{
    options.AssetFallbackDirectory =
        Path.Combine(builder.Environment.ContentRootPath, "enka_assets");
    options.PreloadedLanguages = new List<Language> { Language.English };
});
```

Saved layout:

```
enka_assets/
  genshin/  characters.json  text_map.json  namecards.json  ...
  hsr/      honker_characters.json  honker_weps.json  ...
  zzz/      avatars.json  weapons.json  property.json  ...
```

## Requirements

* .NET Standard 2.0 or later (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## Support

Questions and issues: [Alg's Dev Env](https://discord.gg/d4UgxagmwF) on Discord.

## License

Apache 2.0. See the LICENSE file.

## Acknowledgments

* [Enka.Network](https://enka.network/) for the API
* [seriaati](https://github.com/seriaati) for the inspiration

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (COGNOSPHERE PTE. LTD.), Hypergryph, or Enka.Network. Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero are trademarks of HoYoverse. Arknights: Endfield is a trademark of Hypergryph.
