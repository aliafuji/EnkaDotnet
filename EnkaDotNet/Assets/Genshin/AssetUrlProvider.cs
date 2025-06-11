namespace EnkaDotNet.Assets.Genshin
{
    public static class AssetUrlProvider
    {
        public static Dictionary<string, string> GetGenshinAssetUrls() => new()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/text_map.json" },
            { "characters.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/characters.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/namecards.json" },
            { "consts.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/consts.json" },
            { "talents.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/talents.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/pfps.json" },
            { "affixes.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/affixes.json" }
        };

        public static string GetGenshinIconBaseUrl() => "https://enka.network/ui/";
    }
}