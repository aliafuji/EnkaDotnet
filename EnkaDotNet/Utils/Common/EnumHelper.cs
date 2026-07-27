using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Enums.HSR;
using GenshinStatType = EnkaDotNet.Enums.Genshin.StatType;

namespace EnkaDotNet.Utils.Common
{
    public static class EnumHelper
    {
        private static readonly HashSet<int> _validZZZStatTypes = new HashSet<int>
        {
            0,
            11101, 11102, 11103,
            12101, 12102, 12103,
            12201, 12202,
            13101, 13102, 13103,
            20101, 20103,
            21101, 21103,
            23101, 23103,
            23201, 23203,
            30501, 30502, 30503,
            31201, 31203,
            31401, 31402, 31403,
            31501, 31503,
            31601, 31603,
            31701, 31703,
            31801, 31803,
            31901, 31903,
            32001, 32002, 32003,
            12301, 12302,
            32201, 32203,
            32301, 32303
        };

        private static readonly HashSet<int> _validZZZSkillTypes = new HashSet<int>
        {
            0, 1, 2, 3, 5, 6
        };

        private static readonly HashSet<int> _validHSRStatPropertyTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
            20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36,
            37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54
        };

        private static readonly HashSet<int> _validHSRTraceTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4
        };

        private static readonly HashSet<int> _validHSRRelicTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 6
        };

        private static readonly HashSet<int> _validGenshinStatTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
            20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            40, 41, 42, 43, 44, 45, 46, 47,
            50, 51, 52, 53, 54, 55, 56,
            60, 61, 62, 64, 65, 67,
            70, 71, 72, 73, 74, 75, 76,
            80, 81,
            1000, 1001, 1002, 1003, 1004, 1005, 1006, 1010,
            2000, 2001, 2002, 2003, 2004, 2005,
            3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009,
            3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017, 3018, 3019,
            3020, 3021, 3022, 3023, 3024, 3025, 3026, 3027, 3028, 3029,
            3030, 3031, 3032, 3033, 3034, 3035, 3036, 3037, 3038, 3039,
            3040, 3041, 3042, 3043, 3044, 3045, 3046
        };

        public static bool IsDefinedZZZStatType(int value)
        {
            return _validZZZStatTypes.Contains(value);
        }

        public static bool IsDefinedZZZSkillType(int value)
        {
            return _validZZZSkillTypes.Contains(value);
        }

        public static bool IsDefinedHSRStatPropertyType(int value)
        {
            return _validHSRStatPropertyTypes.Contains(value);
        }

        public static bool IsDefinedHSRTraceType(int value)
        {
            return _validHSRTraceTypes.Contains(value);
        }

        public static bool IsDefinedHSRRelicType(int value)
        {
            return _validHSRRelicTypes.Contains(value);
        }

        public static bool IsDefinedGenshinStatType(int value)
        {
            return _validGenshinStatTypes.Contains(value);
        }
        /// <summary>
        /// Caches the EnumMember value of every member of an enum type, so the reflection cost is
        /// paid once per type instead of once per call. A member with no EnumMemberAttribute is
        /// stored as null, which keeps a miss from re-reflecting.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> _enumMemberValues =
            new ConcurrentDictionary<Type, Dictionary<string, string>>();

        public static string GetEnumMemberValue(this System.Enum value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();
            var name = System.Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }

            var lookup = _enumMemberValues.GetOrAdd(type, BuildEnumMemberLookup);
            return lookup.TryGetValue(name, out var memberValue) ? memberValue : null;
        }

        private static Dictionary<string, string> BuildEnumMemberLookup(Type enumType)
        {
            var names = System.Enum.GetNames(enumType);
            var lookup = new Dictionary<string, string>(names.Length, StringComparer.Ordinal);
            foreach (var name in names)
            {
                var field = enumType.GetField(name);
                var attribute = field == null
                    ? null
                    : System.Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                lookup[name] = attribute?.Value;
            }
            return lookup;
        }

        public static Enums.Language ParseLanguage(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Enums.Language.English;
            switch (value.ToLowerInvariant())
            {
                case "ru": return Enums.Language.Russian;
                case "vi": return Enums.Language.Vietnamese;
                case "th": return Enums.Language.Thai;
                case "pt": return Enums.Language.Portuguese;
                case "ko": return Enums.Language.Korean;
                case "ja": return Enums.Language.Japanese;
                case "id": return Enums.Language.Indonesian;
                case "fr": return Enums.Language.French;
                case "es": return Enums.Language.Spanish;
                case "de": return Enums.Language.German;
                case "zh-tw": return Enums.Language.TraditionalChinese;
                case "zh-cn": return Enums.Language.SimplifiedChinese;
                case "it": return Enums.Language.Italian;
                case "tr": return Enums.Language.Turkish;
                case "en":
                default:
                    return Enums.Language.English;
            }
        }
    }
}
