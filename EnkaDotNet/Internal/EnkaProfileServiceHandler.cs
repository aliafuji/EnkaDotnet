using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Components.EnkaProfile;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.Enka;
using EnkaDotNet.Utils;
using EnkaDotNet.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace EnkaDotNet.Internal
{
    /// <summary>
    /// Handles fetching and processing of Enka.Network user profile data
    /// </summary>
    internal class EnkaProfileServiceHandler
    {
        private readonly EnkaDataMapper _dataMapper;
        private readonly EnkaClientOptions _options;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger<EnkaProfileServiceHandler> _logger;
        private readonly HttpClient _directHttpClient;

        public EnkaProfileServiceHandler(
            EnkaClientOptions options,
            IHttpHelper httpHelper,
            EnkaDataMapper dataMapper,
            ILogger<EnkaProfileServiceHandler> logger,
            HttpClient httpClient)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
            _logger = logger ?? NullLogger<EnkaProfileServiceHandler>.Instance;
            _directHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (_directHttpClient.BaseAddress == null)
            {
                _directHttpClient.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);
            }
            if (!_directHttpClient.DefaultRequestHeaders.UserAgent.ToString().Contains(Constants.DefaultUserAgent))
            {
                _directHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent ?? Constants.DefaultUserAgent);
            }
        }

        public async Task<EnkaProfileResponse> GetRawEnkaProfileByUsernameAsync(string username, bool bypassCache, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace", nameof(username));
            }

            string relativeEndpoint = string.Format(Constants.ENKA_PROFILE_ENDPOINT_FORMAT, Uri.EscapeDataString(username));
            EnkaProfileResponse response = null;

            if (bypassCache || !_options.EnableCaching)
            {
                _logger.LogTrace("Bypassing cache for Enka Profile username {Username}", username);
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_directHttpClient.BaseAddress, relativeEndpoint));
                    HttpResponseMessage httpResponse = await _directHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Enka Profile for username {Username} not found (404)", username);
                        throw new PlayerNotFoundException(0, $"Enka Profile for username {username} not found");
                    }

                    httpResponse.EnsureSuccessStatusCode();
                    string jsonContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    response = JsonSerializer.Deserialize<EnkaProfileResponse>(jsonContent);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP request failed for Enka Profile username {Username}", username);
                    throw new EnkaNetworkException($"Failed to fetch Enka Profile for {username}", ex);
                }
            }
            else
            {
                response = await _httpHelper.Get<EnkaProfileResponse>(relativeEndpoint, bypassCache, cancellationToken).ConfigureAwait(false);
            }


            if (response == null)
            {
                _logger.LogWarning("API for Enka Profile username {Username} returned empty or null content after successful HTTP request", username);
                throw new PlayerNotFoundException(0, $"Enka Profile for username {username} not found or API returned no parsable content");
            }
            return response;
        }

        public async Task<EnkaUserProfile> GetEnkaProfileByUsernameAsync(string username, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawEnkaProfileByUsernameAsync(username, bypassCache, cancellationToken).ConfigureAwait(false);
            return _dataMapper.MapEnkaUserProfile(rawResponse);
        }
    }
}
