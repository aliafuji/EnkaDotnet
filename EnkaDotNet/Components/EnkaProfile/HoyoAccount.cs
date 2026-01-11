using EnkaDotNet.Enums;

namespace EnkaDotNet.Components.EnkaProfile
{
    /// <summary>
    /// Represents a linked game account (hoyo) within an Enka.Network profile.
    /// </summary>
    public class HoyoAccount
    {
        /// <summary>
        /// The unique hash identifier for this hoyo account.
        /// Used to fetch builds for this account.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// The in-game UID for this account.
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// The game type (Genshin, HSR, ZZZ) for this account.
        /// </summary>
        public GameType GameType { get; set; }

        /// <summary>
        /// The player's nickname/username in the game.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The player's level in the game.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The region/server for this account.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Whether this account has been verified on Enka.Network.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Whether this account's builds are publicly visible.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// The order/priority of this hoyo in the profile.
        /// </summary>
        public int Order { get; set; }
    }
}
