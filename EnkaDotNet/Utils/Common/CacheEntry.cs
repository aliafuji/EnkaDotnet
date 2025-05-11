using System;

namespace EnkaDotNet.Utils.Common
{
    public class CacheEntry
    {
        public string JsonResponse { get; set; } = string.Empty;
        public DateTimeOffset Expiration { get; set; }
        public bool IsExpired => DateTimeOffset.UtcNow > Expiration;
    }
}
