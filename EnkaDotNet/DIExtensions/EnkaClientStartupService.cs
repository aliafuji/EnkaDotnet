using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EnkaDotNet.DIExtensions
{
    /// <summary>
    /// Hosted service that preloads Enka assets asynchronously on application startup.
    /// Registered automatically by <see cref="EnkaDotNetServiceCollectionExtensions.AddEnkaNetClient"/>
    /// when <see cref="EnkaClientOptions.PreloadLanguages"/> is non empty.
    /// Using <see cref="IHostedService"/> avoids blocking the DI container thread with
    /// synchronous <c>GetAwaiter().GetResult()</c> calls, which can deadlock in ASP.NET Core.
    /// </summary>
    internal sealed class EnkaClientStartupService : IHostedService
    {
        private readonly IEnkaClient _client;
        private readonly EnkaClientOptions _options;

        public EnkaClientStartupService(IEnkaClient client, IOptions<EnkaClientOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.PreloadLanguages == null || _options.PreloadLanguages.Count == 0)
                return Task.CompletedTask;

            return _client.PreloadAssetsAsync(_options.PreloadLanguages);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
