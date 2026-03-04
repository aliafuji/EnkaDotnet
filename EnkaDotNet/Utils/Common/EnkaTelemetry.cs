using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

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

        public static readonly Histogram<double> RequestDurationMs =
            Meter.CreateHistogram<double>("enka.request.duration", "ms", "Duration of API requests in milliseconds.");
    }
}
