namespace EnkaDotNet.Assets.Loading
{
    public class AssetLoader
    {
        private readonly HttpClient _httpClient;
        private readonly string _basePath;

        public AssetLoader(string basePath, HttpClient? httpClient = null)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> LoadJsonAsync(string url, string filePath)
        {
            string fullPath = Path.Combine(_basePath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? _basePath);

            if (File.Exists(fullPath))
                return await File.ReadAllTextAsync(fullPath);

            var response = await _httpClient.GetStringAsync(url);
            await File.WriteAllTextAsync(fullPath, response);
            return response;
        }
    }
}