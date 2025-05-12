using System;
using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.HSR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet.Assets
{
    /// <summary>
    /// Factory for creating game-specific asset managers
    /// </summary>
    public static class AssetsFactory
    {
        /// <summary>
        /// Creates and initializes an asset manager for Genshin Impact
        /// </summary>
        /// <param name="language">The language for the assets</param>
        /// <param name="httpClient">The HttpClient instance to use for fetching assets</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>An initialized <see cref="IGenshinAssets"/> instance</returns>
        public static async Task<IGenshinAssets> CreateGenshinAssetsAsync(string language, HttpClient httpClient, ILogger<GenshinAssets> logger = null)
        {
            var assets = new GenshinAssets(language, httpClient, logger ?? NullLogger<GenshinAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        /// <summary>
        /// Creates and initializes an asset manager for Zenless Zone Zero
        /// </summary>
        /// <param name="language">The language for the assets</param>
        /// <param name="httpClient">The HttpClient instance to use for fetching assets</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>An initialized <see cref="IZZZAssets"/> instance</returns>
        public static async Task<IZZZAssets> CreateZZZAssetsAsync(string language, HttpClient httpClient, ILogger<ZZZAssets> logger = null)
        {
            var assets = new ZZZAssets(language, httpClient, logger ?? NullLogger<ZZZAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        /// <summary>
        /// Creates and initializes an asset manager for Honkai: Star Rail
        /// </summary>
        /// <param name="language">The language for the assets</param>
        /// <param name="httpClient">The HttpClient instance to use for fetching assets</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>An initialized <see cref="IHSRAssets"/> instance</returns>
        public static async Task<IHSRAssets> CreateHSRAssetsAsync(string language, HttpClient httpClient, ILogger<HSRAssets> logger = null)
        {
            var assets = new HSRAssets(language, httpClient, logger ?? NullLogger<HSRAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }
    }
}
