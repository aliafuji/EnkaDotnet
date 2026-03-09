using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnkaDotNet.Tests.Http
{
    public class PreloadAssetsConcurrencyTests
    {
        private static EnkaClient BuildClient(
            Func<string, Task<IGenshinAssets>> genshinFactory,
            Func<string, Task<IHSRAssets>> hsrFactory,
            Func<string, Task<IZZZAssets>> zzzFactory)
        {
            var services = new ServiceCollection();
            services.AddSingleton(genshinFactory);
            services.AddSingleton(hsrFactory);
            services.AddSingleton(zzzFactory);
            var mockHelper = new Mock<IHttpHelper>();
            services.AddSingleton(mockHelper.Object);
            var mockFactory = new Mock<IHttpClientFactory>();
            var fakeHttpClient = new HttpClient(new FakeHttpMessageHandler()) { BaseAddress = new Uri("https://enka.network/") };
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);
            services.AddSingleton(mockFactory.Object);

            var sp = services.BuildServiceProvider();

            return new EnkaClient(
                Options.Create(new EnkaClientOptions()),
                sp.GetRequiredService<IHttpHelper>(),
                sp,
                sp.GetRequiredService<IHttpClientFactory>());
        }

        /// <summary>
        /// Verifies that 10 concurrent PreloadAssetsAsync calls for the same language
        /// </summary>
        [Fact]
        public async Task PreloadAssetsAsync_SameLanguageConcurrently_LoadsOnce()
        {
            int genshinCount = 0;
            int hsrCount = 0;
            int zzzCount = 0;

            var mock = new Mock<IGenshinAssets>();
            var mockHSR = new Mock<IHSRAssets>();
            var mockZZZ = new Mock<IZZZAssets>();

            var client = BuildClient(
                lang => { Interlocked.Increment(ref genshinCount); return Task.FromResult(mock.Object); },
                lang => { Interlocked.Increment(ref hsrCount); return Task.FromResult(mockHSR.Object); },
                lang => { Interlocked.Increment(ref zzzCount); return Task.FromResult(mockZZZ.Object); });

            var concurrent = new Task[10];
            for (int i = 0; i < 10; i++)
                concurrent[i] = client.PreloadAssetsAsync(new[] { Language.English });

            await Task.WhenAll(concurrent);

            Assert.Equal(1, genshinCount);
            Assert.Equal(1, hsrCount);
            Assert.Equal(1, zzzCount);
        }

        [Fact]
        public async Task PreloadAssetsAsync_DifferentLanguages_LoadsEachOnce()
        {
            int genshinCount = 0;
            var mock = new Mock<IGenshinAssets>();
            var mockHSR = new Mock<IHSRAssets>();
            var mockZZZ = new Mock<IZZZAssets>();

            var client = BuildClient(
                lang => { Interlocked.Increment(ref genshinCount); return Task.FromResult(mock.Object); },
                lang => Task.FromResult(mockHSR.Object),
                lang => Task.FromResult(mockZZZ.Object));

            await client.PreloadAssetsAsync(new[] { Language.English, Language.Japanese, Language.SimplifiedChinese });

            Assert.Equal(3, genshinCount);
        }

        [Fact]
        public async Task PreloadAssetsAsync_DuplicateLanguages_DeduplicatesAndLoadsOnce()
        {
            int genshinCount = 0;
            var mock = new Mock<IGenshinAssets>();
            var mockHSR = new Mock<IHSRAssets>();
            var mockZZZ = new Mock<IZZZAssets>();

            var client = BuildClient(
                lang => { Interlocked.Increment(ref genshinCount); return Task.FromResult(mock.Object); },
                lang => Task.FromResult(mockHSR.Object),
                lang => Task.FromResult(mockZZZ.Object));

            // Test if passing the same enum multiple times dedupes correctly
            await client.PreloadAssetsAsync(new[] { Language.English, Language.English, Language.English });

            Assert.Equal(1, genshinCount);
        }
    }
}
