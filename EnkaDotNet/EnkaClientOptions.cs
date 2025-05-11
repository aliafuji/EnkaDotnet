using EnkaDotNet.Enums;
using EnkaDotNet.Utils;
using System.Collections.Generic;
using System.Net;
using System;

namespace EnkaDotNet
{
    public class EnkaClientOptions
    {
        private int _maxRetries = 1;
        private int _retryDelayMs = 1000;
        private int _maxRetryDelayMs = 30000;
        private int _timeoutSeconds = 30;
        private int _cacheDurationMinutes = 5;


        public string UserAgent { get; set; } = Constants.DefaultUserAgent;
        public string BaseUrl { get; set; }

        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(TimeoutSeconds), "TimeoutSeconds must be greater than 0.");
                }
                _timeoutSeconds = value;
            }
        }

        public bool EnableCaching { get; set; } = true;

        public int CacheDurationMinutes
        {
            get => _cacheDurationMinutes;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(CacheDurationMinutes), "CacheDurationMinutes cannot be negative.");
                }
                _cacheDurationMinutes = value;
            }
        }

        public int MaxRetries
        {
            get => _maxRetries;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries must be at least 1.");
                }
                _maxRetries = value;
            }
        }

        public int RetryDelayMs
        {
            get => _retryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(RetryDelayMs), "RetryDelayMs cannot be negative.");
                }
                _retryDelayMs = value;
            }
        }

        public bool UseExponentialBackoff { get; set; } = true;

        public int MaxRetryDelayMs
        {
            get => _maxRetryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetryDelayMs), "MaxRetryDelayMs cannot be negative.");
                }
                _maxRetryDelayMs = value;
            }
        }

        public List<HttpStatusCode> RetryOnStatusCodes { get; set; } = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)429
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
