using EnkaDotNet.Enums;
using System;

namespace EnkaDotNet.Utils
{
    public static class Constants
    {
        private const string DEFAULT_GENSHIN_API_URL = "https://enka.network/api/";
        private const string DEFAULT_GENSHIN_ASSET_URL = "https://enka.network/ui/";

        public const string DefaultUserAgent = "EnkaDotNet/1.0";

        public static string GetBaseUrl(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => DEFAULT_GENSHIN_API_URL,
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static string GetUserInfoEndpointFormat(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => "uid/{0}",
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static string GetAssetBaseUrl(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => DEFAULT_GENSHIN_ASSET_URL,
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public static bool IsGameTypeSupported(GameType gameType)
        {
            return gameType == GameType.Genshin;
        }
    }
}