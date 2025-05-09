using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;

namespace EnkaDotNet
{
    /// <summary>
    /// Client interface for interacting with the Enka.Network API for HoYoverse games.
    /// </summary>
    public interface IEnkaClient : IDisposable
    {
        /// <summary>
        /// Gets the configured game type for this client instance.
        /// </summary>
        GameType GameType { get; }

        /// <summary>
        /// Gets a clone of the options this client was configured with.
        /// </summary>
        EnkaClientOptions Options { get; }

        /// <summary>
        /// Retrieves the raw API response for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the raw API response.</returns>
        /// <exception cref="ArgumentException">Thrown if UID is not a positive integer.</exception>
        /// <exception cref="EnkaNetworkException">Thrown if a network or API error occurs.</exception>
        /// <exception cref="PlayerNotFoundException">Thrown if the player with the specified UID is not found.</exception>
        /// <exception cref="ProfilePrivateException">Thrown if the player's profile or character details are private.</exception>
        /// <exception cref="NotSupportedException">Thrown if this method is called on a client not configured for Genshin Impact.</exception>
        Task<ApiResponse> GetRawUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the player's information.</returns>
        Task<PlayerInfo> GetPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of characters for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of the player's characters.</returns>
        Task<IReadOnlyList<Character>> GetCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves both player information and character list for a Genshin Impact user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Genshin Impact player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with player information and a read-only list of characters.</returns>
        Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetUserProfileAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the raw API response.</returns>
        Task<HSRApiResponse> GetRawHSRUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the player's information.</returns>
        Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of characters for a Honkai: Star Rail user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Honkai: Star Rail player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of the player's characters.</returns>
        Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the raw API response for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the raw API response.</returns>
        Task<ZZZApiResponse> GetRawZZZUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves player information for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the player's information.</returns>
        Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of agents for a Zenless Zone Zero user.
        /// </summary>
        /// <param name="uid">The User ID (UID) of the Zenless Zone Zero player.</param>
        /// <param name="bypassCache">Whether to bypass the cache for this request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of the player's agents.</returns>
        Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics about the current state of the cache.
        /// </summary>
        /// <returns>A tuple containing the current entry count and an indicator for expired count availability.</returns>
        (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats();

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        void ClearCache();
    }
}
