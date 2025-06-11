using EnkaDotNet.Enums;

namespace EnkaDotNet.Configuration
{
    /// <summary>
    /// Options for configuring an EnkaClient
    /// </summary>
    public class EnkaClientOptions
    {
        /// <summary>
        /// Path where assets should be stored
        /// </summary>
        public string AssetsBasePath { get; set; } = "enka_assets";

        /// <summary>
        /// Type of game to fetch data for
        /// </summary>
        public GameType GameType { get; set; } = GameType.Genshin;

        /// <summary>
        /// Language code for text assets
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Custom User-Agent header for API requests
        /// </summary>
        public string? CustomUserAgent { get; set; }

        /// <summary>
        /// Cache duration for API responses
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Default constructor
        /// </summary>
        public EnkaClientOptions() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public EnkaClientOptions(EnkaClientOptions options)
        {
            AssetsBasePath = options.AssetsBasePath;
            GameType = options.GameType;
            Language = options.Language;
            CustomUserAgent = options.CustomUserAgent;
            CacheDuration = options.CacheDuration;
        }
    }
}