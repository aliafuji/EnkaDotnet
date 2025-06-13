using EnkaDotNet.Utils;
using System.Collections.Generic;
using System.Net;
using System;

namespace EnkaDotNet
{
    /// <summary>
    /// Options for configuring the EnkaDotNet client
    /// </summary>
    public class EnkaClientOptions
    {
        private int _maxRetries = 1;
        private int _retryDelayMs = 1000;
        private int _maxRetryDelayMs = 30000;
        private int _timeoutSeconds = 30;
        private int _cacheDurationMinutes = 5;

        /// <summary>
        /// Gets or sets the User-Agent string for HTTP requests
        /// </summary>
        public string UserAgent { get; set; } = Constants.DefaultUserAgent;

        /// <summary>
        /// Gets or sets the base URL for the Enka.Network API
        /// </summary>
        public string BaseUrl { get; set; } = Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL;

        /// <summary>
        /// Gets or sets the timeout for HTTP requests in seconds
        /// </summary>
        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(TimeoutSeconds), "TimeoutSeconds must be greater than 0");
                }
                _timeoutSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets the cache duration in minutes
        /// </summary>
        public int CacheDurationMinutes
        {
            get => _cacheDurationMinutes;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(CacheDurationMinutes), "CacheDurationMinutes cannot be negative");
                }
                _cacheDurationMinutes = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of retries for failed HTTP requests
        /// </summary>
        public int MaxRetries
        {
            get => _maxRetries;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries must be at least 1");
                }
                _maxRetries = value;
            }
        }

        /// <summary>
        /// Gets or sets the delay between retries in milliseconds
        /// </summary>
        public int RetryDelayMs
        {
            get => _retryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(RetryDelayMs), "RetryDelayMs cannot be negative");
                }
                _retryDelayMs = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use exponential backoff for retries
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum retry delay in milliseconds when using exponential backoff
        /// </summary>
        public int MaxRetryDelayMs
        {
            get => _maxRetryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetryDelayMs), "MaxRetryDelayMs cannot be negative");
                }
                _maxRetryDelayMs = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of HTTP status codes on which to retry
        /// </summary>
        public List<HttpStatusCode> RetryOnStatusCodes { get; set; } = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)429
        };

        /// <summary>
        /// Gets or sets a value indicating whether to return raw stat values or formatted display values
        /// This affects how stats are presented in component models
        /// </summary>
        public bool Raw { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnkaClientOptions"/> class
        /// </summary>
        public EnkaClientOptions() { }

        /// <summary>
        /// Creates a shallow clone of the current options object
        /// </summary>
        public EnkaClientOptions Clone()
        {
            var clone = (EnkaClientOptions)MemberwiseClone();
            clone.RetryOnStatusCodes = new List<HttpStatusCode>(RetryOnStatusCodes);
            return clone;
        }

        /// <summary>
        /// Preloads languages for assets data
        /// </summary>
        public List<string> PreloadLanguages { get; set; } = new List<string>();
    }
}