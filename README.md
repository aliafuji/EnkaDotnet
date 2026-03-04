# Enka.DotNet

Enka.DotNet is a C# wrapper for accessing and processing character data from the Enka.Network API. It provides a simple interface to retrieve detailed information about characters, artifacts, weapons, and player profiles for Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero.

[![NuGet](https://img.shields.io/nuget/v/EnkaDotNet.svg)](https://www.nuget.org/packages/EnkaDotNet/)

## Features

* **Multi-Game Support:** Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero
* **Strongly Typed Models:** Clear and user-friendly C# models for all entities
* **Flexible Client Setup:**
    * **Direct Instantiation:** `await EnkaClient.CreateAsync(options)`
    * **Dependency Injection:** `services.AddEnkaNetClient(options => { ... })`
* **HTTP Resiliency:** Polly-powered retry with exponential backoff + jitter, circuit breaker, and 429 Retry After support
* **Multi-Provider Caching:** Memory (default), SQLite, Redis, or custom `IEnkaCache`
* **Asset Preloading:** Warm up game assets at startup for zero-latency first requests
* **Observability:** OpenTelemetry `ActivitySource` + `System.Diagnostics.Metrics` hooks for traces and metrics

## Supported Games

| Game              | Status   | Method      |
|-------------------|----------|-------------|
| Genshin Impact    | ✅ Ready | UID         |
| Honkai: Star Rail | ✅ Ready | UID         |
| Zenless Zone Zero | ✅ Ready | UID         |

## Miscellaneous

| Feature             | Status   | Method        |
|---------------------|----------|---------------|
| Fetch Basic Profile | ✅ Ready | Enka Username |
| Genshin Impact      | ✅ Ready | Enka Username |
| Honkai: Star Rail   | ✅ Ready | Enka Username |
| Zenless Zone Zero   | ✅ Ready | Enka Username |

## Installation

```bash
dotnet add package EnkaDotNet
```

## Usage & Examples

Enka.DotNet supports both direct instantiation and Dependency Injection.

### Direct Instantiation

```csharp
var options = new EnkaClientOptions
{
    EnableCaching = true,
    CacheDurationMinutes = 10,
    UserAgent = "MyApp/1.0"
};
await using IEnkaClient client = await EnkaClient.CreateAsync(options);
```

### Dependency Injection (ASP.NET Core / Worker Service)

```csharp
builder.Services.AddEnkaNetClient(options =>
{
    options.EnableCaching = true;
    options.CacheDurationMinutes = 10;

    // Warm up assets at startup (runs via IHostedService, never blocks DI thread)
    options.PreloadLanguages = new List<string> { "en", "ja" };
});
```

For detailed runnable examples, see the `Examples/` directory in the repository.

## Caching

### Cache Providers

Configure the cache provider via `EnkaClientOptions.CacheProvider`:

```csharp
// In memory (default)
options.CacheProvider = CacheProvider.Memory;

// SQLite (persistent across restarts, no external dependency)
options.CacheProvider = CacheProvider.SQLite;
options.SQLiteCache = new SQLiteCacheOptions
{
    DatabasePath = "enka_cache.db",
    DefaultTtl = TimeSpan.FromMinutes(10)
};

// Redis (distributed, for multi instance apps)
options.CacheProvider = CacheProvider.Redis;
options.RedisCache = new RedisCacheOptions
{
    ConnectionString = "localhost:6379",
    KeyPrefix = "myapp:enka:"
};
```

### Runtime Cache Control

```csharp
// Force fresh data for one call
var profile = await client.GetGenshinPlayerInfoAsync(uid, bypassCache: true);

// Clear all cached entries
client.ClearCache();

// Get cache statistics
var (count, _) = client.GetCacheStats();
```

## HTTP Resiliency

Resiliency is configured via `EnkaClientOptions`:

```csharp
options.MaxRetries = 3;                         // Retry attempts (default: 1)
options.RetryDelayMs = 1000;                    // Base delay in ms (default: 1000)
options.UseExponentialBackoff = true;           // Exponential backoff with jitter (default: true)
options.MaxRetryDelayMs = 30000;                // Cap on retry delay (default: 30000)
options.CircuitBreakerFailureThreshold = 5;     // Failures before circuit opens (default: 5)
options.CircuitBreakerBreakDurationSeconds = 30; // Seconds circuit stays open (default: 30)
```

**429 Too Many Requests** is automatically retried using the `Retry-After` response header as the delay. A `RateLimitException` is only thrown after all retry attempts are exhausted.

## Observability

The library exposes OpenTelemetry hooks with no additional dependencies. Wire up your own exporters:

```csharp
// Distributed tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("EnkaDotNet").AddOtlpExporter());

// Metrics (counters + request duration histogram)
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("EnkaDotNet").AddPrometheusExporter());
```

**Exposed metrics:**

| Metric | Description |
|---|---|
| `enka.requests.total` | Total API requests made |
| `enka.cache.hits` | Cache hit count |
| `enka.cache.misses` | Cache miss count |
| `enka.retries.total` | Total retry attempts |
| `enka.request.duration` (ms) | Request duration histogram |

## Asset Preloading

Preloading warms up the in-memory asset cache at startup so the first real request has zero asset-load latency:

```csharp
// Direct instantiation
await using var client = await EnkaClient.CreateAsync(new EnkaClientOptions
{
    PreloadLanguages = new List<string> { "en", "ja" }
});

// DI (runs asynchronously via IHostedService)
builder.Services.AddEnkaNetClient(options =>
{
    options.PreloadLanguages = new List<string> { "en", "ja" };
});
```

## Asset Fallback (Offline / GitHub Unavailable)

Set `AssetFallbackDirectory` to make the library resilient to network outages.
The first time an asset is downloaded successfully it is saved to disk.
If a subsequent download fails (GitHub down, no internet), the saved copy is served instead.

```csharp
options.AssetFallbackDirectory = "/path/to/your/enka_assets";
```

By default (`null`) nothing is ever written to disk.

### Where do the files go?

The path is set by **your application** — the library just reads and writes to wherever you point it.
The NuGet package folder (`~/.nuget/packages/enkadotnet/`) is read-only and shared; files are never written there.

| App type | Recommended path | Resolves to (example) |
|---|---|---|
| ASP.NET Core / Worker Service | `Path.Combine(builder.Environment.ContentRootPath, "enka_assets")` | `/myapp/enka_assets/` |
| Console app | `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnkaDotNet", "assets")` | `C:/Users/john/AppData/Local/EnkaDotNet/assets` |
| Docker | Mount a volume and set the path explicitly | `/data/enka_assets` |

> **Avoid using `AppContext.BaseDirectory`** that resolves to the build output folder (`bin/Release/net8.0/`) which is wiped on each rebuild.

**Full ASP.NET Core example:**

```csharp
builder.Services.AddEnkaNetClient(options =>
{
    // Writes to <project-root>/enka_assets/ which survives rebuilds
    options.AssetFallbackDirectory =
        Path.Combine(builder.Environment.ContentRootPath, "enka_assets");

    options.PreloadLanguages = new List<string> { "en", "ja" };
});
```

**Saved file layout:**

```
enka_assets/
  genshin/ characters.json  text_map.json  namecards.json  ...
  hsr/     honker_characters.json  honker_weps.json  ...
  zzz/     avatars.json  weapons.json  property.json  ...
```

## Requirements

* .NET Standard 2.0 compatible framework (.NET Core 2.0+, .NET Framework 4.6.1+, .NET 5+)

## Support

Having questions or issues? Join our Discord server: [Alg's Dev Env](https://discord.gg/d4UgxagmwF)

## License

This project is licensed under the Apache 2.0 License - see the LICENSE file for details.

## Acknowledgments

* [Enka.Network](https://enka.network/) for providing the API
* [seriaati](https://github.com/seriaati) for the inspiration

---

## Disclaimer

This project is not affiliated with or endorsed by HoYoverse (COGNOSPHERE PTE. LTD.) or Enka.Network. Genshin Impact, Honkai: Star Rail, and Zenless Zone Zero are trademarks of HoYoverse.