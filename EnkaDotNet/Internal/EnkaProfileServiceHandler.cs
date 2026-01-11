using System;
using System.Collections.Generic;
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
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

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
#if NET8_0_OR_GREATER
                    response = JsonSerializer.Deserialize<EnkaProfileResponse>(jsonContent, EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                    response = JsonSerializer.Deserialize<EnkaProfileResponse>(jsonContent);
#pragma warning restore IL2026, IL3050
#endif
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

        /// <summary>
        /// Fetches raw builds data for a specific hoyo account from the Enka.Network API.
        /// </summary>
        /// <param name="username">The Enka.Network username</param>
        /// <param name="hoyoHash">The hash identifier for the specific hoyo account</param>
        /// <param name="bypassCache">Whether to bypass the cache</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A dictionary mapping character/agent IDs to lists of raw builds</returns>
        /// <exception cref="ArgumentException">Thrown when username or hoyoHash is null or whitespace</exception>
        /// <exception cref="PlayerNotFoundException">Thrown when the profile or hoyo is not found (404)</exception>
        /// <exception cref="EnkaNetworkException">Thrown when the API request fails</exception>
        public async Task<Dictionary<string, List<RawBuildModel>>> GetRawBuildsByUsernameAsync(
            string username, 
            string hoyoHash, 
            bool bypassCache, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(hoyoHash))
            {
                throw new ArgumentException("Hoyo hash cannot be null or whitespace", nameof(hoyoHash));
            }

            string relativeEndpoint = string.Format(
                Constants.ENKA_BUILDS_ENDPOINT_FORMAT, 
                Uri.EscapeDataString(username), 
                Uri.EscapeDataString(hoyoHash));

            Dictionary<string, List<RawBuildModel>> response = null;

            if (bypassCache || !_options.EnableCaching)
            {
                _logger.LogTrace("Bypassing cache for Enka builds - username: {Username}, hoyoHash: {HoyoHash}", username, hoyoHash);
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_directHttpClient.BaseAddress, relativeEndpoint));
                    HttpResponseMessage httpResponse = await _directHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Enka builds for username {Username} and hoyo {HoyoHash} not found (404)", username, hoyoHash);
                        throw new PlayerNotFoundException(0, $"Enka builds for username '{username}' and hoyo '{hoyoHash}' not found");
                    }

                    httpResponse.EnsureSuccessStatusCode();
                    string jsonContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
#if NET8_0_OR_GREATER
                    response = JsonSerializer.Deserialize<Dictionary<string, List<RawBuildModel>>>(jsonContent, EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                    response = JsonSerializer.Deserialize<Dictionary<string, List<RawBuildModel>>>(jsonContent);
#pragma warning restore IL2026, IL3050
#endif
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP request failed for Enka builds - username: {Username}, hoyoHash: {HoyoHash}", username, hoyoHash);
                    throw new EnkaNetworkException($"Failed to fetch Enka builds for username '{username}' and hoyo '{hoyoHash}'", ex);
                }
            }
            else
            {
                try
                {
                    response = await _httpHelper.Get<Dictionary<string, List<RawBuildModel>>>(relativeEndpoint, bypassCache, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("NotFound"))
                {
                    _logger.LogWarning("Enka builds for username {Username} and hoyo {HoyoHash} not found (404)", username, hoyoHash);
                    throw new PlayerNotFoundException(0, $"Enka builds for username '{username}' and hoyo '{hoyoHash}' not found");
                }
            }

            if (response == null)
            {
                _logger.LogWarning("API for Enka builds returned empty or null content - username: {Username}, hoyoHash: {HoyoHash}", username, hoyoHash);
                // Return empty dictionary instead of throwing - no builds is a valid state
                return new Dictionary<string, List<RawBuildModel>>();
            }

            return response;
        }
    }
}
