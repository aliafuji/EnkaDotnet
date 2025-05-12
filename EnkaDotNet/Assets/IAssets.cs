namespace EnkaDotNet.Assets
{
    /// <summary>
    /// Interface for accessing game specific assets
    /// </summary>
    public interface IAssets
    {
        /// <summary>
        /// Gets the language of the loaded assets
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Gets an identifier for the game these assets belong to (e.g, "genshin", "hsr")
        /// </summary>
        string GameIdentifier { get; }

        /// <summary>
        /// Gets the localized text for a given hash or key
        /// </summary>
        /// <param name="hash">The hash or key for the text</param>
        /// <returns>The localized text, or the hash itself if not found</returns>
        string GetText(string hash);
    }
}
