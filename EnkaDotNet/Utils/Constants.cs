using EnkaDotNet.Enums;
using System;

namespace EnkaDotNet.Utils
{
    public static class Constants
    {
        private const string DEFAULT_GENSHIN_API_URL = "https://enka.network/api/";
        private const string DEFAULT_GENSHIN_ASSET_URL = "https://enka.network/ui/";

        private const string DEFAULT_ZZZ_API_URL = "https://enka.network/api/zzz/";
        private const string DEFAULT_ZZZ_ASSET_URL = "https://enka.network";

        public const string DefaultUserAgent = "EnkaDotNet/1.0";

        public static string GetBaseUrl(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => DEFAULT_GENSHIN_API_URL,
                GameType.ZZZ => DEFAULT_ZZZ_API_URL,
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static string GetUserInfoEndpointFormat(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => "uid/{0}",
                GameType.ZZZ => "uid/{0}",
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static string GetAssetBaseUrl(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => DEFAULT_GENSHIN_ASSET_URL,
                GameType.ZZZ => DEFAULT_ZZZ_ASSET_URL,
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static bool IsGameTypeSupported(GameType gameType)
        {
            return gameType == GameType.Genshin || gameType == GameType.ZZZ;
        }
    }
}