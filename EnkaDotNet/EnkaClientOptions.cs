using EnkaDotNet.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet
{
    public class EnkaClientOptions
    {
        public string UserAgent { get; set; }
        public string BaseUrl { get; set; }
        public string AssetBaseUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 5;
        public int MaxRetries { get; set; } = 0;
        public int RetryDelayMs { get; set; } = 1000;
        public bool EnableVerboseLogging { get; set; } = false;
        public string Language { get; set; } = "en";
        public GameType GameType { get; set; } = GameType.Genshin;
        public bool Raw { get; set; } = false;

        public EnkaClientOptions() { }

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
                GameType = this.GameType,
                Raw = this.Raw
            };
        }
    }
}