using System;
using EnkaDotNet.Enums;

namespace EnkaDotNet
{
    /// <summary>
    /// Configuration options for EnkaClient
    /// </summary>
    public class EnkaClientOptions
    {
        /// <summary>
        /// User agent string to use for API requests and asset downloads.
        /// If null, uses a default user agent.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Custom base URL for the Enka Network API.
        /// If null, uses the default URL.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Custom base URL for asset images.
        /// If null, uses the default URL.
        /// </summary>
        public string? AssetBaseUrl { get; set; }

        /// <summary>
        /// Timeout for HTTP requests in seconds. Default is 30 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to enable request caching. Default is true.
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Default cache duration in minutes. Default is 5 minutes.
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Maximum number of retries for failed requests. Default is 0 (no retries).
        /// </summary>
        public int MaxRetries { get; set; } = 0;

        /// <summary>
        /// Delay between retries in milliseconds. Default is 1000ms.
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Whether to log detailed diagnostic information. Default is false.
        /// </summary>
        public bool EnableVerboseLogging { get; set; } = false;

        /// <summary>
        /// The language to use for text localization. Default is "en".
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// The game type to use. Default is Genshin Impact.
        /// Currently, only Genshin Impact is supported.
        /// </summary>
        public GameType GameType { get; set; } = GameType.Genshin;

        /// <summary>
        /// Creates a new instance of EnkaClientOptions with default values.
        /// </summary>
        public EnkaClientOptions() { }

        /// <summary>
        /// Creates a copy of the current options.
        /// </summary>
        public EnkaClientOptions Clone()
        {
            return new EnkaClientOptions
            {
                UserAgent = this.UserAgent,
                BaseUrl = this.BaseUrl,
                AssetBaseUrl = this.AssetBaseUrl,
                TimeoutSeconds = this.TimeoutSeconds,
                EnableCaching = this.EnableCaching,
                CacheDurationMinutes = this.CacheDurationMinutes,
                MaxRetries = this.MaxRetries,
                RetryDelayMs = this.RetryDelayMs,
                EnableVerboseLogging = this.EnableVerboseLogging,
                Language = this.Language,
                GameType = this.GameType
            };
        }
    }
}