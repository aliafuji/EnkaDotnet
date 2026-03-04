using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Utils.Common
{
    internal static class EnkaEventIds
    {
        public static readonly EventId CacheHit          = new EventId(1000, "CacheHit");
        public static readonly EventId CacheMiss         = new EventId(1001, "CacheMiss");
        public static readonly EventId RequestStart      = new EventId(1002, "RequestStart");
        public static readonly EventId RetryAttempt      = new EventId(1003, "RetryAttempt");
        public static readonly EventId RateLimited       = new EventId(1004, "RateLimited");
        public static readonly EventId AssetPreloadStart = new EventId(1005, "AssetPreloadStart");
        public static readonly EventId AssetPreloadDone  = new EventId(1006, "AssetPreloadDone");
        public static readonly EventId AssetLoadFailed   = new EventId(1007, "AssetLoadFailed");
        public static readonly EventId CircuitOpen       = new EventId(1008, "CircuitOpen");
        public static readonly EventId CircuitClosed     = new EventId(1009, "CircuitClosed");
        public static readonly EventId CircuitHalfOpen   = new EventId(1010, "CircuitHalfOpen");
    }
}
