using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets;
using EnkaDotNet.Tests.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EnkaDotNet.Tests.Assets
{
    public class BaseAssetsLifetimeTests
    {
        private const string TextMapJson = @"{""en"":{""1001"":""Hello""},""ja"":{""1001"":""Konnichiwa""}}";

        /// <summary>
        /// Minimal concrete BaseAssets that only loads the text map, so tests exercise the
        /// shared initialization, cancellation and disposal behaviour.
        /// </summary>
        private sealed class TestAssets : BaseAssets
        {
            private readonly IReadOnlyDictionary<string, string> _urls;

            public TestAssets(string language, HttpClient httpClient, string fallbackDirectory = null, string url = "https://example.test/text_map.json")
                : base(language, "test", httpClient, NullLogger.Instance, fallbackDirectory)
            {
                _urls = new Dictionary<string, string> { { "text_map.json", url } };
            }

            protected override IReadOnlyDictionary<string, string> GetAssetFileUrls() => _urls;

            protected override Task LoadAssetsInternalAsync() => Task.CompletedTask;
        }

        private static HttpClient ClientReturning(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new FakeHttpMessageHandler();
            handler.Enqueue(statusCode, json);
            return new HttpClient(handler);
        }

        [Fact]
        public async Task Dispose_DoesNotBreakOtherInstances()
        {
            // The initialization semaphore used to be static, so disposing any assets instance
            // permanently broke initialization for every other instance in the process.
            var first = new TestAssets("en", ClientReturning(TextMapJson));
            await first.EnsureInitializedAsync();
            first.Dispose();

            var second = new TestAssets("en", ClientReturning(TextMapJson));
            await second.EnsureInitializedAsync();

            Assert.Equal("Hello", second.GetText("1001"));
        }

        [Fact]
        public async Task EnsureInitializedAsync_AfterDispose_Throws()
        {
            var assets = new TestAssets("en", ClientReturning(TextMapJson));
            assets.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => assets.EnsureInitializedAsync());
        }

        [Fact]
        public async Task EnsureInitializedAsync_HonoursCallerCancellation()
        {
            var handler = new BlockingHandler();
            var assets = new TestAssets("en", new HttpClient(handler));

            using var cts = new CancellationTokenSource();
            var initialization = assets.EnsureInitializedAsync(cts.Token);

            await handler.RequestStarted.Task;
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => initialization);
        }

        [Fact]
        public async Task EnsureInitializedAsync_MissingRequestedLanguage_FallsBackToEnglish()
        {
            var assets = new TestAssets("fr", ClientReturning(TextMapJson));

            await assets.EnsureInitializedAsync();

            Assert.Equal("Hello", assets.GetText("1001"));
        }

        [Fact]
        public async Task EnsureInitializedAsync_EnglishRequestedButAbsent_FallsBackToFirstAvailable()
        {
            // Previously this path was unreachable: the fallback search was skipped whenever the
            // requested language was already "en", so initialization threw instead of falling back.
            var assets = new TestAssets("en", ClientReturning(@"{""ja"":{""1001"":""Konnichiwa""}}"));

            await assets.EnsureInitializedAsync();

            Assert.Equal("Konnichiwa", assets.GetText("1001"));
        }

        [Fact]
        public async Task FetchAsset_WritesFallbackFileAndReusesItWhenTheNetworkFails()
        {
            string directory = Path.Combine(Path.GetTempPath(), "enka-fallback-" + Guid.NewGuid().ToString("N"));
            try
            {
                var online = new TestAssets("en", ClientReturning(TextMapJson), directory);
                await online.EnsureInitializedAsync();

                Assert.True(File.Exists(Path.Combine(directory, "test", "text_map.json")));

                var offline = new TestAssets("en", new HttpClient(new ThrowingHandler()), directory);
                await offline.EnsureInitializedAsync();

                Assert.Equal("Hello", offline.GetText("1001"));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public async Task FetchAsset_LeavesNoTemporaryFilesBehind()
        {
            string directory = Path.Combine(Path.GetTempPath(), "enka-fallback-" + Guid.NewGuid().ToString("N"));
            try
            {
                var assets = new TestAssets("en", ClientReturning(TextMapJson), directory);
                await assets.EnsureInitializedAsync();

                Assert.Empty(Directory.GetFiles(Path.Combine(directory, "test"), "*.tmp"));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new HttpRequestException("network down");
        }

        private sealed class BlockingHandler : HttpMessageHandler
        {
            public TaskCompletionSource<bool> RequestStarted { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestStarted.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
                throw new InvalidOperationException("unreachable");
            }
        }
    }
}
