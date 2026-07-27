# Changelog

## Unreleased

### Added

* Arknights: Endfield support (`GameType.Endfield`)
  * `GetEFRawUserResponseAsync`, `GetEFPlayerInfoAsync`, `GetEFOperatorsAsync`
  * UID type is `long` (Endfield UIDs can be larger than `int`)
  * Operator stats: HP, ATK, attributes, crit, elemental bonuses
  * Gear, weapons, gems, and 4 piece suit bonuses are included in the calc
  * `EFOperator.CalculateAllTotalStats()` / `GetFinalStats()` use stable English keys
  * `GetAllStats()` uses localized stat names for display
  * Operator talents via `EFOperator.Talents` (rank + icon from passive skill nodes)
  * Profile achievements via `EFPlayerInfo.Medals` (name, level, plated icon)
  * Image URLs on `EFOperator`: icon, round icon, splash art, silhouette, profession icon
  * Examples in `Examples/Endfield/` (NonDI and DI)

## 2.0.0

A clean-break release. There are no `[Obsolete]` compatibility aliases for the renamed members,
so every rename below is a compile error until you update your call sites. No runtime behaviour
changed as part of the renames: enum numeric values, string literals, and wire formats are identical.

### Added

* **`EnkaDotNet.Caching.Sqlite`** and **`EnkaDotNet.Caching.Redis`** as opt-in packages so the
  core client no longer pulls in those dependencies by default.

### Breaking changes

#### `Constants` renamed to PascalCase

The public constants in `EnkaDotNet.Utils.Constants` now match the PascalCase style already used
by `DefaultUserAgent`. Their values are unchanged.

| Old | New |
| --- | --- |
| `DEFAULT_GENSHIN_API_URL` | `DefaultGenshinApiUrl` |
| `DEFAULT_GENSHIN_ASSET_CDN_URL` | `DefaultGenshinAssetCdnUrl` |
| `DEFAULT_ZZZ_API_URL` | `DefaultZZZApiUrl` |
| `DEFAULT_ZZZ_ASSET_CDN_URL` | `DefaultZZZAssetCdnUrl` |
| `DEFAULT_HSR_API_URL` | `DefaultHSRApiUrl` |
| `DEFAULT_HSR_ASSET_CDN_URL` | `DefaultHSRAssetCdnUrl` |
| `DEFAULT_ENKA_PROFILE_API_BASE_URL` | `DefaultEnkaProfileApiBaseUrl` |
| `ENKA_PROFILE_ENDPOINT_FORMAT` | `EnkaProfileEndpointFormat` |
| `ENKA_BUILDS_ENDPOINT_FORMAT` | `EnkaBuildsEndpointFormat` |
| `DEFAULT_GAME_SPECIFIC_USER_INFO_ENDPOINT_FORMAT` | `DefaultGameSpecificUserInfoEndpointFormat` |

#### `EnkaDotNet.Enums.HSR.RelicType` members renamed

The members are now game-accurate PascalCase. The underlying numeric values are unchanged, so
serialized data and any code that casts to or from `int` keeps working. `Unknown = 0` is unchanged.
Note that `NECK` and `OBJECT` were misnamed relative to the game and are now `PlanarSphere` and
`LinkRope`.

| Old | New | Value |
| --- | --- | --- |
| `HEAD` | `Head` | 1 |
| `HAND` | `Hands` | 2 |
| `BODY` | `Body` | 3 |
| `FOOT` | `Feet` | 4 |
| `NECK` | `PlanarSphere` | 5 |
| `OBJECT` | `LinkRope` | 6 |

`GetRelicName()` returns the same display strings as before, and the asset data strings that are
parsed into `RelicType` (`"HEAD"`, `"HAND"`, `"BODY"`, `"FOOT"`, `"NECK"`, `"OBJECT"`) are unchanged.

#### `EnkaClientOptions.Raw` renamed to `UseRawStatValues`

`EnkaClientOptions.Raw` is now `EnkaClientOptions.UseRawStatValues`. It still defaults to `false`
and still controls whether stats are exposed as raw or formatted display values.

```csharp
// Before
var options = new EnkaClientOptions { Raw = false };

// After
var options = new EnkaClientOptions { UseRawStatValues = false };
```

The unrelated `Raw` properties on `HSRStatValue` and `ZZZStatValue` are unchanged.

#### ZZZ model classes moved out of the `Enums` namespace

`StatSummary`, `FormattedStatValues`, and `Skin` are data models rather than enums, and moved from
`EnkaDotNet.Enums.ZZZ` to `EnkaDotNet.Components.ZZZ`. The class definitions are otherwise
unchanged. If you referenced them by their fully qualified name, or had a `using
EnkaDotNet.Enums.ZZZ;` solely for them, add `using EnkaDotNet.Components.ZZZ;`.

#### SQLite and Redis cache providers moved to opt-in packages

The SQLite and Redis cache providers now live in the separate `EnkaDotNet.Caching.Sqlite` and
`EnkaDotNet.Caching.Redis` packages. Setting `CacheProvider.SQLite` or `CacheProvider.Redis` alone
is no longer enough: install the matching package and register the provider with
`options.UseSqliteCache(...)` / `services.AddEnkaSqliteCache(...)` (or the Redis equivalents,
`options.UseRedisCache(...)` / `services.AddEnkaRedisCache(...)`). Without this the client throws
an error telling you which package to install.

#### `HSRStatCalculator` reports the Elation damage bonus

The Elation damage bonus was previously always reported as zero. It is now calculated, so
characters with Elation damage bonus sources report a different (correct) value than in 1.x.
