using System;
using EnkaDotNet.Caching;

#nullable enable

namespace EnkaDotNet.Exceptions
{
    /// <summary>
    /// Exception thrown when a cache operation fails.
    /// </summary>
    public class CacheException : Exception
    {
        /// <summary>
        /// Gets the cache provider that caused the exception.
        /// </summary>
        public CacheProvider Provider { get; }

        /// <summary>
        /// Gets the name of the configuration property that caused the exception, if applicable.
        /// </summary>
        public string? ConfigurationProperty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        public CacheException() : base("A cache operation failed.")
        {
            Provider = CacheProvider.Memory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CacheException(string message) : base(message)
        {
            Provider = CacheProvider.Memory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CacheException(string message, Exception innerException) : base(message, innerException)
        {
            Provider = CacheProvider.Memory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with provider information.
        /// </summary>
        /// <param name="provider">The cache provider that caused the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        public CacheException(CacheProvider provider, string message) : base(message)
        {
            Provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with provider and configuration information.
        /// </summary>
        /// <param name="provider">The cache provider that caused the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="configurationProperty">The name of the configuration property that caused the exception.</param>
        public CacheException(CacheProvider provider, string message, string? configurationProperty) : base(message)
        {
            Provider = provider;
            ConfigurationProperty = configurationProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with provider, configuration, and inner exception information.
        /// </summary>
        /// <param name="provider">The cache provider that caused the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="configurationProperty">The name of the configuration property that caused the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CacheException(CacheProvider provider, string message, string? configurationProperty, Exception innerException) 
            : base(message, innerException)
        {
            Provider = provider;
            ConfigurationProperty = configurationProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class with provider and inner exception information.
        /// </summary>
        /// <param name="provider">The cache provider that caused the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CacheException(CacheProvider provider, string message, Exception innerException) 
            : base(message, innerException)
        {
            Provider = provider;
        }

        /// <summary>
        /// Gets a message that describes the current exception including provider details.
        /// </summary>
        public override string Message
        {
            get
            {
                var baseMessage = base.Message;
                var providerInfo = $"[{Provider} Cache]";
                
                if (!string.IsNullOrEmpty(ConfigurationProperty))
                {
                    return $"{providerInfo} Configuration error in '{ConfigurationProperty}': {baseMessage}";
                }
                
                return $"{providerInfo} {baseMessage}";
            }
        }
    }
}
