namespace EnkaDotNet.Assets.Caching
{
    public class AssetCache<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _cache = new();

        public bool TryGetValue(TKey key, out TValue? value) => _cache.TryGetValue(key, out value);

        public void AddOrUpdate(TKey key, TValue value) => _cache[key] = value;

        public void Clear() => _cache.Clear();

        public int Count => _cache.Count;
    }
}