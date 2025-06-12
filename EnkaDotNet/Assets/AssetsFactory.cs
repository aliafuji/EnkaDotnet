using System.Net.Http;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.HSR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet.Assets
{
    public static class AssetsFactory
    {
        public static async Task<IGenshinAssets> CreateGenshinAssetsAsync(string language, HttpClient httpClient, ILogger<GenshinAssets> logger = null)
        {
            var assets = new GenshinAssets(language, httpClient, logger ?? NullLogger<GenshinAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        public static async Task<IZZZAssets> CreateZZZAssetsAsync(string language, HttpClient httpClient, ILogger<ZZZAssets> logger = null)
        {
            var assets = new ZZZAssets(language, httpClient, logger ?? NullLogger<ZZZAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }

        public static async Task<IHSRAssets> CreateHSRAssetsAsync(string language, HttpClient httpClient, ILogger<HSRAssets> logger = null)
        {
            var assets = new HSRAssets(language, httpClient, logger ?? NullLogger<HSRAssets>.Instance);
            await assets.EnsureInitializedAsync().ConfigureAwait(false);
            return assets;
        }
    }
}
