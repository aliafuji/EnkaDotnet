using EnkaDotNet.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Utils
{
    public static class Constants
    {
        private const string DEFAULT_GENSHIN_API_URL = "https://enka.network/api/";
        private const string DEFAULT_GENSHIN_ASSET_URL = "https://enka.network/ui/";

        private const string DEFAULT_ZZZ_API_URL = "https://enka.network/api/zzz/";
        private const string DEFAULT_ZZZ_ASSET_URL = "https://enka.network";

        private const string DEFAULT_HSR_API_URL = "https://enka.network/api/hsr/";
        private const string DEFAULT_HSR_ASSET_URL = "https://enka.network/ui/hsr/";

        public const string DefaultUserAgent = "EnkaDotNet/1.0";

        public static string GetBaseUrl(GameType gameType)
        {
            if (gameType == GameType.Genshin)
                return DEFAULT_GENSHIN_API_URL;
            if (gameType == GameType.ZZZ)
                return DEFAULT_ZZZ_API_URL;
            if (gameType == GameType.HSR)
                return DEFAULT_HSR_API_URL;

            throw new NotSupportedException($"Game type {gameType} is not supported.");
        }

        public static string GetUserInfoEndpointFormat(GameType gameType)
        {
            if (gameType == GameType.Genshin)
                return "uid/{0}";
            if (gameType == GameType.ZZZ)
                return "uid/{0}";
            if (gameType == GameType.HSR)
                return "uid/{0}";

            throw new NotSupportedException($"Game type {gameType} is not supported.");
        }

        public static string GetAssetBaseUrl(GameType gameType)
        {
            if (gameType == GameType.Genshin)
                return DEFAULT_GENSHIN_ASSET_URL;
            if (gameType == GameType.ZZZ)
                return DEFAULT_ZZZ_ASSET_URL;
            if (gameType == GameType.HSR)
                return DEFAULT_HSR_ASSET_URL;

            throw new NotSupportedException($"Game type {gameType} is not supported.");
        }

        public static bool IsGameTypeSupported(GameType gameType)
        {
            return gameType == GameType.Genshin ||
                   gameType == GameType.ZZZ ||
                   gameType == GameType.HSR;
        }
    }
}