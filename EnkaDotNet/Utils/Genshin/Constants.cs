using EnkaDotNet.Enums;
using System;

namespace EnkaDotNet.Utils.Genshin
{
    public static class Constants
    {
        public static string GetBaseUrl(GameType gameType)
        {
            return gameType switch
            {
                GameType.Genshin => "https://enka.network/api/",
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
                GameType.Genshin => "https://enka.network/ui/",
                _ => throw new NotSupportedException($"Game type {gameType} is not supported.")
            };
        }

        public const string DefaultUserAgent = "EnkaSharp/1.0";
    }
}