using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EnkaDotNet;
using EnkaDotNet.Caching.Providers;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EnkaDotNet.Tests.Http
{
    public class HttpHelperRetryTests
    {
        private const string validJson = "{\"nickname\":\"TestPlayer\",\"level\":60}";

        private static HttpHelper CreateHelper(FakeHttpMessageHandler fake, EnkaClientOptions? opts = null)
        {
            opts ??= new EnkaClientOptions { MaxRetries = 3, RetryDelayMs = 0, UseExponentialBackoff = false };
            var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://enka.network/") };
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new HttpHelper(
                httpClient,
                cache,
                Options.Create(opts),
                NullLogger<HttpHelper>.Instance);
        }

        [Fact]
        public async Task Get_CacheHit_DoesNotCallHttp()
        {
            var fake = new FakeHttpMessageHandler();
            using var helper = CreateHelper(fake, new EnkaClientOptions { MaxRetries = 1, RetryDelayMs = 0 });

            fake.Enqueue(HttpStatusCode.OK, validJson);
            await helper.Get<PlayerInfoModel>("test/url");

            int callsBefore = fake.CallCount;
            var result = await helper.Get<PlayerInfoModel>("test/url");

            Assert.Equal(callsBefore, fake.CallCount);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Get_500Response_RetriesAndEventuallyThrows()
        {
            var fake = new FakeHttpMessageHandler();
            for (int i = 0; i < 4; i++)
                fake.Enqueue(HttpStatusCode.InternalServerError, "{}");

            using var helper = CreateHelper(fake);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                helper.Get<PlayerInfoModel>("fail/endpoint"));

            Assert.Equal(4, fake.CallCount);
        }

        [Fact]
        public async Task Get_RetrySucceeds_ReturnResult()
        {
            var fake = new FakeHttpMessageHandler();
            fake.Enqueue(HttpStatusCode.InternalServerError);
            fake.Enqueue(HttpStatusCode.InternalServerError);
            fake.Enqueue(HttpStatusCode.OK, validJson);

            using var helper = CreateHelper(fake);
            var result = await helper.Get<PlayerInfoModel>("retry/success");

            Assert.Equal(3, fake.CallCount);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Get_404_ThrowsPlayerNotFoundException_WithoutRetrying()
        {
            var fake = new FakeHttpMessageHandler();
            fake.Enqueue(HttpStatusCode.NotFound);

            using var helper = CreateHelper(fake);

            await Assert.ThrowsAsync<PlayerNotFoundException>(() =>
                helper.Get<PlayerInfoModel>("uid/123456789"));

            Assert.Equal(1, fake.CallCount);
        }

        [Fact]
        public async Task Get_429WithRetryAfterHeader_RetriesAndThrowsRateLimitAfterExhaustion()
        {
            var fake = new FakeHttpMessageHandler();
            for (int i = 0; i < 3; i++)
            {
                var r429 = new HttpResponseMessage((HttpStatusCode)429);
                r429.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                fake.Enqueue(r429);
            }

            using var helper = CreateHelper(fake, new EnkaClientOptions
            {
                MaxRetries = 2,
                RetryDelayMs = 0,
                UseExponentialBackoff = false
            });

            await Assert.ThrowsAsync<RateLimitException>(() =>
                helper.Get<PlayerInfoModel>("rate/limited"));

            Assert.Equal(3, fake.CallCount);
        }

        [Fact]
        public async Task Get_429ThenSuccess_ReturnsResultAfterRetry()
        {
            var fake = new FakeHttpMessageHandler();
            var r429 = new HttpResponseMessage((HttpStatusCode)429);
            r429.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
            fake.Enqueue(r429);
            fake.Enqueue(HttpStatusCode.OK, validJson);

            using var helper = CreateHelper(fake, new EnkaClientOptions
            {
                MaxRetries = 3,
                RetryDelayMs = 0,
                UseExponentialBackoff = false
            });

            var result = await helper.Get<PlayerInfoModel>("rate/retry/success");

            Assert.Equal(2, fake.CallCount);
            Assert.NotNull(result);
        }
    }
}
