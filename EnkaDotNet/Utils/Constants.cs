using System;
using System.Collections.Generic;

namespace EnkaDotNet.Utils
{
    /// <summary>
    /// Provides constant values used throughout the library
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default API base URL for Genshin Impact data
        /// </summary>
        public const string DEFAULT_GENSHIN_API_URL = "https://enka.network/api/";
        /// <summary>
        /// Default CDN base URL for Genshin Impact assets
        /// </summary>
        public const string DEFAULT_GENSHIN_ASSET_CDN_URL = "https://enka.network/ui/";

        /// <summary>
        /// Default API base URL for Zenless Zone Zero data
        /// </summary>
        public const string DEFAULT_ZZZ_API_URL = "https://enka.network/api/zzz/";
        /// <summary>
        /// Default CDN base URL for Zenless Zone Zero assets
        /// </summary>
        public const string DEFAULT_ZZZ_ASSET_CDN_URL = "https://enka.network";

        /// <summary>
        /// Default API base URL for Honkai: Star Rail data
        /// </summary>
        public const string DEFAULT_HSR_API_URL = "https://enka.network/api/hsr/";
        /// <summary>
        /// Default CDN base URL for Honkai: Star Rail assets
        /// </summary>
        public const string DEFAULT_HSR_ASSET_CDN_URL = "https://enka.network/ui/hsr/";

        /// <summary>
        /// Default API base URL for Enka.Network user profiles
        /// </summary>
        public const string DEFAULT_ENKA_PROFILE_API_BASE_URL = "https://enka.network/api/";
        /// <summary>
        /// Endpoint format for Enka.Network user profiles
        /// </summary>
        public const string ENKA_PROFILE_ENDPOINT_FORMAT = "profile/{0}/?format=json";

        /// <summary>
        /// Endpoint format for Enka.Network user profile builds
        /// Format parameters: {0} = username, {1} = hoyo hash
        /// </summary>
        public const string ENKA_BUILDS_ENDPOINT_FORMAT = "profile/{0}/hoyos/{1}/builds/";

        /// <summary>
        /// Default endpoint format for game-specific user information, typically by UID
        /// </summary>
        public const string DEFAULT_GAME_SPECIFIC_USER_INFO_ENDPOINT_FORMAT = "uid/{0}";

        /// <summary>
        /// Default User-Agent string for HTTP requests
        /// </summary>
        public const string DefaultUserAgent = "EnkaDotNet/1.0";

        /// <summary>
        /// URLs for Genshin Impact asset files
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> GenshinAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/text_map.json" },
            { "characters.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/characters.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/namecards.json" },
            { "consts.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/consts.json" },
            { "talents.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/talents.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/pfps.json" }
        };

        /// <summary>
        /// URLs for Honkai: Star Rail asset files
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> HSRAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/hsr.json" },
            { "characters.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_characters.json" },
            { "lightcones.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_weps.json" },
            { "relics.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_relics.json" },
            { "avatars.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_avatars.json" },
            { "skills.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_skills.json" },
            { "ranks.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_ranks.json" },
            { "skill_tree.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/skill_tree.json" },
            { "meta.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_meta.json" }
        };

        /// <summary>
        /// URLs for Zenless Zone Zero asset files
        /// </summary>
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
    }
}
