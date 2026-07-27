using System;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Caching;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

            services.TryAddSingleton<IEnkaCache>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;

                // Resolved here rather than at registration time so an opt in provider package can
                // be added before or after this call without changing the outcome.
                var providerFactory = sp.GetService<IEnkaCacheProviderFactory>();
                return providerFactory != null
                    ? providerFactory.CreateCache(opts)
                    : CacheFactory.CreateCache(opts, sp.GetService<IMemoryCache>());
            });

            services.AddHttpClient<IHttpHelper, HttpHelper>((serviceProvider, client) =>
            {
                var opts = serviceProvider.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl ?? Constants.DefaultEnkaProfileApiBaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent ?? Constants.DefaultUserAgent);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            services.AddHttpClient("EnkaProfileClient", client =>
            {
                var tempOptions = new EnkaClientOptions();
                configureOptionsAction?.Invoke(tempOptions);
                client.BaseAddress = new Uri(Constants.DefaultEnkaProfileApiBaseUrl);
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
            services.AddHttpClient("EFAssetClient", configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            services.TryAddSingleton<Func<string, Task<IGenshinAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("GenshinAssetClient");
                    var logger = sp.GetService<ILogger<GenshinAssets>>() ?? NullLogger<GenshinAssets>.Instance;
                    var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                    return await AssetsFactory.CreateGenshinAssetsAsync(language, httpClient, logger, opts.AssetFallbackDirectory).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<Func<string, Task<IHSRAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("HSRAssetClient");
                    var logger = sp.GetService<ILogger<HSRAssets>>() ?? NullLogger<HSRAssets>.Instance;
                    var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                    return await AssetsFactory.CreateHSRAssetsAsync(language, httpClient, logger, opts.AssetFallbackDirectory).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<Func<string, Task<IZZZAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("ZZZAssetClient");
                    var logger = sp.GetService<ILogger<ZZZAssets>>() ?? NullLogger<ZZZAssets>.Instance;
                    var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                    return await AssetsFactory.CreateZZZAssetsAsync(language, httpClient, logger, opts.AssetFallbackDirectory).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<Func<string, Task<IEFAssets>>>(sp =>
            {
                return async (language) =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("EFAssetClient");
                    var logger = sp.GetService<ILogger<EFAssets>>() ?? NullLogger<EFAssets>.Instance;
                    var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                    return await AssetsFactory.CreateEFAssetsAsync(language, httpClient, logger, opts.AssetFallbackDirectory).ConfigureAwait(false);
                };
            });

            services.TryAddSingleton<EnkaClient>();

            services.TryAddSingleton<IEnkaClient>(sp => sp.GetRequiredService<EnkaClient>());

            services.AddHostedService<EnkaClientStartupService>();

            return services;
        }
    }
}
