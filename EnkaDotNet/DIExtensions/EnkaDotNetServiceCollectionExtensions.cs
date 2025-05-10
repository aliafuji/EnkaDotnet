using System;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

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

            services.AddHttpClient<IHttpHelper, HttpHelper>((serviceProvider, client) =>
            {
                var opts = serviceProvider.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                string baseUrl = opts.BaseUrl ?? Constants.GetApiBaseUrl(opts.GameType);

                if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                {
                    client.BaseAddress = baseUri;
                }
                else
                {
                    client.BaseAddress = new Uri(new Uri(Constants.GetApiBaseUrl(opts.GameType)), baseUrl);
                }
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent ?? Constants.DefaultUserAgent);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            });

            Action<HttpClient> configureAssetClient = client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
            };

            services.AddHttpClient(nameof(GenshinAssets), configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            services.AddHttpClient(nameof(HSRAssets), configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            services.AddHttpClient(nameof(ZZZAssets), configureAssetClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

            switch (optionsInstance.GameType)
            {
                case GameType.Genshin:
                    services.TryAddSingleton<Task<IGenshinAssets>>(async sp =>
                    {
                        var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient(nameof(GenshinAssets));
                        var logger = sp.GetService<ILogger<GenshinAssets>>() ?? NullLogger<GenshinAssets>.Instance;
                        return await AssetsFactory.CreateGenshinAsync(opts.Language, httpClient, logger).ConfigureAwait(false);
                    });
                    break;
                case GameType.HSR:
                    services.TryAddSingleton<Task<IHSRAssets>>(async sp =>
                    {
                        var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient(nameof(HSRAssets));
                        var logger = sp.GetService<ILogger<HSRAssets>>() ?? NullLogger<HSRAssets>.Instance;
                        return await AssetsFactory.CreateHSRAsync(opts.Language, httpClient, logger).ConfigureAwait(false);
                    });
                    break;
                case GameType.ZZZ:
                    services.TryAddSingleton<Task<IZZZAssets>>(async sp =>
                    {
                        var opts = sp.GetRequiredService<IOptions<EnkaClientOptions>>().Value;
                        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient(nameof(ZZZAssets));
                        var logger = sp.GetService<ILogger<ZZZAssets>>() ?? NullLogger<ZZZAssets>.Instance;
                        return await AssetsFactory.CreateZZZAsync(opts.Language, httpClient, logger).ConfigureAwait(false);
                    });
                    break;
                default:
                    throw new UnsupportedGameTypeException(optionsInstance.GameType, $"Game type {optionsInstance.GameType} is not supported for DI registration.");
            }

            services.TryAddScoped<IEnkaClient, EnkaClient>();
            return services;
        }
    }
}
