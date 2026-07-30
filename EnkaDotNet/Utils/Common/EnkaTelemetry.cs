using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;

namespace EnkaDotNet.Utils.Common
{
    /// <summary>
    /// Central OpenTelemetry instrumentation hooks for EnkaDotNet.
    /// </summary>
    internal static class EnkaTelemetry
    {
        public const string SourceName = "EnkaDotNet";

        public static readonly string SourceVersion =
            typeof(EnkaTelemetry).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

        public static readonly ActivitySource ActivitySource = new ActivitySource(SourceName, SourceVersion);

        public static readonly Meter Meter = new Meter(SourceName, SourceVersion);

        public static readonly Counter<long> RequestCount =
            Meter.CreateCounter<long>("enka.requests.total", description: "Total number of API requests made.");

        public static readonly Counter<long> CacheHits =
            Meter.CreateCounter<long>("enka.cache.hits", description: "Number of cache hits.");

        public static readonly Counter<long> CacheMisses =
            Meter.CreateCounter<long>("enka.cache.misses", description: "Number of cache misses.");

        public static readonly Counter<long> RetryCount =
            Meter.CreateCounter<long>("enka.retries.total", description: "Total number of retry attempts.");

        public static readonly Counter<long> ErrorCount =
            Meter.CreateCounter<long>("enka.errors.total", description: "Total number of API/request errors.");

        public static readonly Histogram<double> RequestDurationMs =
            Meter.CreateHistogram<double>("enka.request.duration", "ms", "Duration of API requests in milliseconds.");

        public static string ResolveGame(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return "unknown";

            var path = relativeUrl.TrimStart('/');
            if (path.StartsWith("zzz/", StringComparison.OrdinalIgnoreCase)) return "zzz";
            if (path.StartsWith("hsr/", StringComparison.OrdinalIgnoreCase)) return "hsr";
            if (path.StartsWith("ef/", StringComparison.OrdinalIgnoreCase)) return "endfield";
            if (path.StartsWith("profile/", StringComparison.OrdinalIgnoreCase)) return "profile";
            if (path.StartsWith("uid/", StringComparison.OrdinalIgnoreCase)) return "genshin";
            return "unknown";
        }

        public static string HashUid(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return string.Empty;

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(uid));
#if NET5_0_OR_GREATER
            return Convert.ToHexString(hash, 0, 4).ToLowerInvariant();
#else
            return BitConverter.ToString(hash, 0, 4).Replace("-", string.Empty).ToLowerInvariant();
#endif
        }

        public static TagList GameTags(string game) => new TagList { { "game", game } };

        public static void RecordError(string type, string game, string? status)
        {
            var tags = new TagList
            {
                { "type", type },
                { "game", game },
                { "status", status ?? "none" }
            };
            ErrorCount.Add(1, in tags);
        }
    }
}
