namespace EnkaDotNet.Components.Genshin
{
    /// <summary>
    /// Represents a saved Genshin Impact character build from an Enka.Network profile.
    /// </summary>
    public class GenshinBuild
    {
        /// <summary>
        /// The unique identifier for this build.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The user-defined name for this build.
        /// </summary>
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// The display order of this build.
        /// </summary>
        public decimal Order { get; internal set; }

        /// <summary>
        /// Whether this is the live/active build from the game.
        /// </summary>
        public bool IsLive { get; internal set; }

        /// <summary>
        /// The character ID this build belongs to.
        /// </summary>
        public int CharacterId { get; internal set; }

        /// <summary>
        /// The full character data for this build.
        /// </summary>
        public Character Character { get; internal set; }
    }
}
