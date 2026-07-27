#nullable enable

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Implemented by the opt in cache provider packages (EnkaDotNet.Caching.Sqlite,
    /// EnkaDotNet.Caching.Redis) to supply an <see cref="IEnkaCache"/> without the core package
    /// having to reference their dependencies.
    /// </summary>
    /// <remarks>
    /// The core registration resolves this lazily, which is what makes
    /// <c>AddEnkaSqliteCache</c> and <c>AddEnkaNetClient</c> order independent.
    /// </remarks>
    public interface IEnkaCacheProviderFactory
    {
        /// <summary>
        /// Creates the cache described by <paramref name="options"/>.
        /// </summary>
        IEnkaCache CreateCache(EnkaClientOptions options);
    }
}
