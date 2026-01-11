using System.Collections.Generic;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Enums.HSR;
using GenshinStatType = EnkaDotNet.Enums.Genshin.StatType;

namespace EnkaDotNet.Utils.Common
{
    public static class EnumHelper
    {
        private static readonly HashSet<int> ValidZZZStatTypes = new HashSet<int>
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
            32201, 32203
        };

        private static readonly HashSet<int> ValidZZZSkillTypes = new HashSet<int>
        {
            0, 1, 2, 3, 5, 6
        };

        private static readonly HashSet<int> ValidHSRStatPropertyTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
            20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36,
            37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53
        };

        private static readonly HashSet<int> ValidHSRTraceTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4
        };

        private static readonly HashSet<int> ValidHSRRelicTypes = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 6
        };

        private static readonly HashSet<int> ValidGenshinStatTypes = new HashSet<int>
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
            return ValidZZZStatTypes.Contains(value);
        }

        public static bool IsDefinedZZZSkillType(int value)
        {
            return ValidZZZSkillTypes.Contains(value);
        }

        public static bool IsDefinedHSRStatPropertyType(int value)
        {
            return ValidHSRStatPropertyTypes.Contains(value);
        }

        public static bool IsDefinedHSRTraceType(int value)
        {
            return ValidHSRTraceTypes.Contains(value);
        }

        public static bool IsDefinedHSRRelicType(int value)
        {
            return ValidHSRRelicTypes.Contains(value);
        }

        public static bool IsDefinedGenshinStatType(int value)
        {
            return ValidGenshinStatTypes.Contains(value);
        }
    }
}
