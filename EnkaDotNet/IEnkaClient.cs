using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Components.EnkaProfile;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Models.EnkaProfile;

namespace EnkaDotNet
{
    /// <summary>
    /// Client interface for interacting with the Enka.Network API.
    /// </summary>
    public interface IEnkaClient : IDisposable
    {
        /// <summary>
        /// Gets a clone of the options this client was configured with.
        /// </summary>
        EnkaClientOptions Options { get; }

        /// <summary>
        /// Retrieves the Enka.Network profile information for a user by username. This is game-agnostic.
        /// </summary>
        /// <param name="username">The username on Enka.Network.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user's Enka.Network profile information.</returns>
        Task<EnkaUserProfile> GetEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for an Enka.Network profile by username. This is game-agnostic.
        /// </summary>
        /// <param name="username">The username on Enka.Network.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the raw API response.</returns>
        Task<EnkaProfileResponse> GetRawEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="language">Optional language code for localized data (e.g., "en", "ja"). Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<ApiResponse> GetGenshinRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="language">Optional language code for localized data (e.g., "en", "ja"). Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<PlayerInfo> GetGenshinPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of characters for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="language">Optional language code for localized data (e.g., "en", "ja"). Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<IReadOnlyList<Character>> GetGenshinCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves both player information and character list for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="language">Optional language code for localized data (e.g., "en", "ja"). Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetGenshinUserProfileAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<HSRApiResponse> GetHSRRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of characters for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<ZZZApiResponse> GetZZZRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of agents for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves Genshin Impact character builds by Enka.Network username and hoyo hash.
        /// </summary>
        /// <param name="username">The username on Enka.Network.</param>
        /// <param name="hoyoHash">The hash identifier for the linked game account (hoyo).</param>
        /// <param name="language">Optional language code for localized data (e.g., "en", "ja"). Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A dictionary mapping character IDs to lists of builds for that character.</returns>
        Task<Dictionary<string, List<GenshinBuild>>> GetGenshinBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves Honkai: Star Rail character builds by Enka.Network username and hoyo hash.
        /// </summary>
        /// <param name="username">The username on Enka.Network.</param>
        /// <param name="hoyoHash">The hash identifier for the linked game account (hoyo).</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A dictionary mapping character IDs to lists of builds for that character.</returns>
        Task<Dictionary<string, List<HSRBuild>>> GetHSRBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves Zenless Zone Zero agent builds by Enka.Network username and hoyo hash.
        /// </summary>
        /// <param name="username">The username on Enka.Network.</param>
        /// <param name="hoyoHash">The hash identifier for the linked game account (hoyo).</param>
        /// <param name="language">Optional language code for localized data. Defaults to "en" if null.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A dictionary mapping agent IDs to lists of builds for that agent.</returns>
        Task<Dictionary<string, List<ZZZBuild>>> GetZZZBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics about the current state of the cache.
        /// </summary>
        (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats();

        /// <summary>
        /// Preloads assets for specified games and languages to prevent on-demand loading during the first API call.
        /// </summary>
        /// <param name="languages">A collection of language codes to load assets for (e.g., "en", "ja").</param>
        Task PreloadAssetsAsync(IEnumerable<string> languages);

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        void ClearCache();
    }
}
