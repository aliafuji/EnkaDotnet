using EnkaDotNet.Enums;
using EnkaDotNet.Utils;
using System.Collections.Generic;
using System.Net;

namespace EnkaDotNet
{
    public class EnkaClientOptions
    {
        public string UserAgent { get; set; } = Constants.DefaultUserAgent;
        public string BaseUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 5;

        public int MaxRetries { get; set; } = 0;
        public int RetryDelayMs { get; set; } = 1000;
        public bool UseExponentialBackoff { get; set; } = true;
        public int MaxRetryDelayMs { get; set; } = 30000;
        public List<HttpStatusCode> RetryOnStatusCodes { get; set; } = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway,          // 502
            HttpStatusCode.ServiceUnavailable,  // 503
            HttpStatusCode.GatewayTimeout,      // 504
            (HttpStatusCode)429                 // TooManyRequests
        };

        public bool EnableVerboseLogging { get; set; } = false;
        public string Language { get; set; } = "en";
        public GameType GameType { get; set; } = GameType.Genshin;
        public bool Raw { get; set; } = false;
        public string HttpClientName { get; set; }

        public EnkaClientOptions() { }

        public EnkaClientOptions Clone()
        {
            var clone = (EnkaClientOptions)MemberwiseClone();
            clone.RetryOnStatusCodes = new List<HttpStatusCode>(RetryOnStatusCodes);
            return clone;
        }
    }
}
