using System;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using EnkaDotNet.Assets;

namespace EnkaDotNet.DIExtensions
{
    public static class EnkaDotNetServiceCollectionExtensions
    {
        public static IServiceCollection AddEnkaNetClient(
            this IServiceCollection services,
            Action<EnkaClientOptions> configureOptionsAction = null)
        {
            var optionsInstance = new EnkaClientOptions();
            configureOptionsAction?.Invoke(optionsInstance);

            services.AddSingleton(Options.Create(optionsInstance));
            services.TryAddSingleton<IMemoryCache>(sp => new MemoryCache(new MemoryCacheOptions()));
            services.AddHttpClient();

            services.AddHttpClient<IHttpHelper, HttpHelper>((serviceProvider, client) =>
            {
                var opts = serviceProvider.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl ?? Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent ?? Constants.DefaultUserAgent);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            services.AddHttpClient("EnkaProfileClient", client =>
            {
                var tempOptions = new EnkaClientOptions();
                configureOptionsAction?.Invoke(tempOptions);
                client.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(tempOptions.UserAgent ?? Constants.DefaultUserAgent);
                client.Timeout = TimeSpan.FromSeconds(tempOptions.TimeoutSeconds);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            Action<HttpClient> configureAssetClient = client =>
            {
                var tempOptions = new EnkaClientOptions();
                configureOptionsAction?.Invoke(tempOptions);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(tempOptions.UserAgent ?? Constants.DefaultUserAgent);
                client.Timeout = TimeSpan.FromSeconds(tempOptions.TimeoutSeconds);
            };

            services.AddHttpClient("GenshinAssetClient", configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            services.AddHttpClient("HSRAssetClient", configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            services.AddHttpClient("ZZZAssetClient", configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            services.TryAddSingleton<Func<string, Task<IGenshinAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("GenshinAssetClient");
                    var logger = sp.GetService<ILogger<GenshinAssets>>() ?? NullLogger<GenshinAssets>.Instance;
                    return await AssetsFactory.CreateGenshinAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<Func<string, Task<IHSRAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("HSRAssetClient");
                    var logger = sp.GetService<ILogger<HSRAssets>>() ?? NullLogger<HSRAssets>.Instance;
                    return await AssetsFactory.CreateHSRAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<Func<string, Task<IZZZAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("ZZZAssetClient");
                    var logger = sp.GetService<ILogger<ZZZAssets>>() ?? NullLogger<ZZZAssets>.Instance;
                    return await AssetsFactory.CreateZZZAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                };
            });

            services.TryAddScoped<IEnkaClient, EnkaClient>();
            return services;
        }
    }
}
