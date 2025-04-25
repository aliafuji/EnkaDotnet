using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
