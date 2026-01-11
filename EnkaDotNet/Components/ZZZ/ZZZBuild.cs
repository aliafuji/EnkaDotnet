namespace EnkaDotNet.Components.ZZZ
{
    /// <summary>
    /// Represents a saved Zenless Zone Zero agent build from an Enka.Network profile.
    /// </summary>
    public class ZZZBuild
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
        /// The agent ID this build belongs to.
        /// </summary>
        public int AgentId { get; internal set; }

        /// <summary>
        /// The full agent data for this build.
        /// </summary>
        public ZZZAgent Agent { get; internal set; }
    }
}
