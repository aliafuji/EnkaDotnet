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
        public const string DefaultGenshinApiUrl = "https://enka.network/api/";
        /// <summary>
        /// Default CDN base URL for Genshin Impact assets
        /// </summary>
        public const string DefaultGenshinAssetCdnUrl = "https://enka.network/ui/";

        /// <summary>
        /// Default API base URL for Zenless Zone Zero data
        /// </summary>
        public const string DefaultZZZApiUrl = "https://enka.network/api/zzz/";
        /// <summary>
        /// Default CDN base URL for Zenless Zone Zero assets
        /// </summary>
        public const string DefaultZZZAssetCdnUrl = "https://enka.network";

        /// <summary>
        /// Default API base URL for Honkai: Star Rail data
        /// </summary>
        public const string DefaultHSRApiUrl = "https://enka.network/api/hsr/";
        /// <summary>
        /// Default CDN base URL for Honkai: Star Rail assets
        /// </summary>
        public const string DefaultHSRAssetCdnUrl = "https://enka.network/ui/hsr/";

        /// <summary>
        /// Default API base URL for Arknights: Endfield data
        /// </summary>
        public const string DefaultEFApiUrl = "https://enka.network/api/ef/";
        /// <summary>
        /// Default CDN base URL for Arknights: Endfield assets
        /// </summary>
        public const string DefaultEFAssetCdnUrl = "https://enka.network";

        /// <summary>
        /// Default API base URL for Enka.Network user profiles
        /// </summary>
        public const string DefaultEnkaProfileApiBaseUrl = "https://enka.network/api/";
        /// <summary>
        /// Endpoint format for Enka.Network user profiles
        /// </summary>
        public const string EnkaProfileEndpointFormat = "profile/{0}/?format=json";

        /// <summary>
        /// Endpoint format for Enka.Network user profile builds
        /// Format parameters: {0} = username, {1} = hoyo hash
        /// </summary>
        public const string EnkaBuildsEndpointFormat = "profile/{0}/hoyos/{1}/builds/";

        /// <summary>
        /// Default endpoint format for game-specific user information, typically by UID
        /// </summary>
        public const string DefaultGameSpecificUserInfoEndpointFormat = "uid/{0}";

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
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/hsr.json" },
            { "characters.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_characters.json" },
            { "lightcones.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_weps.json" },
            { "relics.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_relics.json" },
            { "avatars.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_avatars.json" },
            { "skills.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_skills.json" },
            { "ranks.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_ranks.json" },
            { "skill_tree.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/skill_tree.json" },
            { "meta.json", "https://raw.githubusercontent.com/pizza-studio/EnkaDBGenerator/refs/heads/main/Sources/EnkaDBFiles/Resources/Specimen/HSR/honker_meta.json" },
            { "relic_set.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/relic_set.json" }
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

        /// <summary>
        /// URLs for Arknights: Endfield asset files
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> EFAssetFileUrls = new Dictionary<string, string>()
        {
            { "text_map.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/locs.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/avatars.json" },
            { "weapons.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/weapons.json" },
            { "equips.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/equips.json" },
            { "gems.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/gems.json" },
            { "skills.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/skills.json" },
            { "weapon_meta.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/weapon_meta.json" },
            { "pfps.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/pfps.json" },
            { "namecards.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/namecards.json" },
            { "medals.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/ef/medals.json" }
        };
    }
}
