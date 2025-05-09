using EnkaDotNet.Enums;
using System;
using System.Collections.Generic;

namespace EnkaDotNet.Utils
{
    public static class Constants
    {
        private const string DEFAULT_GENSHIN_API_URL = "https://enka.network/api/";
        private const string DEFAULT_GENSHIN_ASSET_CDN_URL = "https://enka.network/ui/";

        private const string DEFAULT_ZZZ_API_URL = "https://enka.network/api/zzz/";
        private const string DEFAULT_ZZZ_ASSET_CDN_URL = "https://enka.network";

        private const string DEFAULT_HSR_API_URL = "https://enka.network/api/hsr/";
        private const string DEFAULT_HSR_ASSET_CDN_URL = "https://enka.network/ui/hsr/";

        public const string DefaultUserAgent = "EnkaDotNet/1.0";

        public static string GetApiBaseUrl(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Genshin: return DEFAULT_GENSHIN_API_URL;
                case GameType.ZZZ: return DEFAULT_ZZZ_API_URL;
                case GameType.HSR: return DEFAULT_HSR_API_URL;
                default: throw new NotSupportedException($"API base URL for game type {gameType} is not supported.");
            }
        }

        public static string GetAssetCdnBaseUrl(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Genshin: return DEFAULT_GENSHIN_ASSET_CDN_URL;
                case GameType.ZZZ: return DEFAULT_ZZZ_ASSET_CDN_URL;
                case GameType.HSR: return DEFAULT_HSR_ASSET_CDN_URL;
                default: throw new NotSupportedException($"Asset CDN base URL for game type {gameType} is not supported.");
            }
        }

        public static string GetUserInfoEndpointFormat(GameType gameType)
        {
            return "uid/{0}";
        }

        public static bool IsGameTypeSupported(GameType gameType)
        {
            return gameType == GameType.Genshin ||
                   gameType == GameType.ZZZ ||
                   gameType == GameType.HSR;
        }

        public static readonly IReadOnlyDictionary<string, string> GenshinAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/text_map.json" },
            { "characters.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/characters.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/namecards.json" },
            { "consts.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/consts.json" },
            { "talents.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/talents.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/pfps.json" }
        };

        public static readonly IReadOnlyDictionary<string, string> HSRAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/hsr.json" },
            { "characters.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_characters.json" },
            { "lightcones.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_weps.json" },
            { "relics.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_relics.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_avatars.json" },
            { "skills.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_skills.json" },
            { "ranks.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_ranks.json" },
            { "skill_tree.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/skill_tree.json" },
            { "meta.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_meta.json" }
        };

        public static readonly IReadOnlyDictionary<string, string> ZZZAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/locs.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/avatars.json" },
            { "weapons.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/weapons.json" },
            { "equipments.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/equipments.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/pfps.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/namecards.json" },
            { "medals.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/medals.json" },
            { "titles.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/titles.json" },
            { "property.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/zzz/property.json" },
            { "equipment_level.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/equipment_level.json" },
            { "weapon_level.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/weapon_level.json" },
            { "weapon_star.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/zzz/weapon_star.json" }
        };

        public static IReadOnlyDictionary<string, string> GetGameAssetFileUrls(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Genshin: return GenshinAssetFileUrls;
                case GameType.HSR: return HSRAssetFileUrls;
                case GameType.ZZZ: return ZZZAssetFileUrls;
                default: throw new NotSupportedException($"Asset file URLs for game type {gameType} are not supported.");
            }
        }
    }
}