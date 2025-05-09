using System;
using System.Net.Http.Headers;

namespace EnkaDotNet.Exceptions
{
    public class RateLimitException : EnkaNetworkException
    {
        public RetryConditionHeaderValue RetryAfter { get; }

        public RateLimitException(string message, RetryConditionHeaderValue retryAfter = null)
            : base(message)
        {
            RetryAfter = retryAfter;
        }
        public RateLimitException(string message, Exception innerException, RetryConditionHeaderValue retryAfter = null)
            : base(message, innerException)
        {
            RetryAfter = retryAfter;
        }
    }
}
