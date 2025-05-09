using System;

namespace EnkaDotNet.Utils.Common
{
    public class CacheEntry
    {
        public string JsonResponse { get; set; } = string.Empty;
        public string ETag { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public bool IsExpired => DateTimeOffset.UtcNow > Expiration;
    }
}