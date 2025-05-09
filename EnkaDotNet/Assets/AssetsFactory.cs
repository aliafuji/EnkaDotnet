using System;
using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet.Assets
{
    public static class AssetsFactory
    {
        public static async Task<IAssets> CreateAsync(string language, GameType gameType, HttpClient httpClient, ILogger logger)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            ILogger effectiveLogger = logger ?? NullLogger.Instance;
            BaseAssets assetsInstance;

            switch (gameType)
            {
                case GameType.Genshin:
                    assetsInstance = new GenshinAssets(language, httpClient, (ILogger<GenshinAssets>)effectiveLogger);
                    break;
                case GameType.ZZZ:
                    assetsInstance = new ZZZAssets(language, httpClient, (ILogger<ZZZAssets>)effectiveLogger);
                    break;
                case GameType.HSR:
                    assetsInstance = new HSRAssets(language, httpClient, (ILogger<HSRAssets>)effectiveLogger);
                    break;
                default:
                    throw new UnsupportedGameTypeException(gameType, $"Game type {gameType} is not supported.");
            }
            await assetsInstance.EnsureInitializedAsync().ConfigureAwait(false);
            return assetsInstance;
        }

        public static async Task<IGenshinAssets> CreateGenshinAsync(string language, HttpClient httpClient, ILogger<GenshinAssets> logger)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            var assets = new GenshinAssets(language, httpClient, logger ?? NullLogger<GenshinAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        public static async Task<IZZZAssets> CreateZZZAsync(string language, HttpClient httpClient, ILogger<ZZZAssets> logger)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            var assets = new ZZZAssets(language, httpClient, logger ?? NullLogger<ZZZAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        public static async Task<IHSRAssets> CreateHSRAsync(string language, HttpClient httpClient, ILogger<HSRAssets> logger)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            var assets = new HSRAssets(language, httpClient, logger ?? NullLogger<HSRAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }
    }
}