using System;
using System.Collections.Generic;
using System.Globalization;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Enums.EF;

namespace EnkaDotNet.Utils.EF
{
    public static class EFStatsHelpers
    {
        private const string StatCriticalRate = "Critical Rate";
        private const string StatCriticalDmg = "Critical DMG";
        private const string StatTreatmentReceivedBonus = "Treatment Received Bonus";
        private const string StatPhysicalDmgBonus = "Physical DMG Bonus";
        private const string StatHeatDmgBonus = "Heat DMG Bonus";
        private const string StatElectricDmgBonus = "Electric DMG Bonus";
        private const string StatCryoDmgBonus = "Cryo DMG Bonus";
        private const string StatNatureDmgBonus = "Nature DMG Bonus";
        private const string StatAetherDmgBonus = "Æther DMG Bonus";
        private const string StatUltimateDmgBonus = "Ultimate DMG Bonus";
        private const string StatArtsIntensity = "Arts Intensity";

        private static readonly HashSet<int> _percentageAttrIds = new HashSet<int>
        {
            9, 17, 25, 29, 44, 49, 50, 51, 52, 53, 54, 55
        };

        private static readonly Dictionary<int, string> _attrLocKeys = new Dictionary<int, string>
        {
            [1] = "Hp",
            [2] = "Atk",
            [3] = "Def",
            [9] = "CriticalRate",
            [17] = "PhysicalAndSpellInflictionEnhance",
            [29] = "HealOutputIncrease",
            [39] = "Str",
            [40] = "Agi",
            [41] = "Wisd",
            [42] = "Will",
            [44] = "UltimateSkillDamageIncrease",
            [50] = "PhysicalDamageIncrease",
            [51] = "FireDamageIncrease",
            [52] = "PulseDamageIncrease",
            [53] = "CrystDamageIncrease",
            [54] = "NaturalDamageIncrease",
            [55] = "EtherDamageIncrease",
            [87] = "PhysicalAndSpellInflictionEnhance"
        };

        private static readonly Dictionary<int, string> _attrFallbackNames = new Dictionary<int, string>
        {
            [1] = "HP",
            [2] = "Attack",
            [3] = "Defense",
            [9] = StatCriticalRate,
            [17] = StatArtsIntensity,
            [29] = "Treatment Bonus",
            [39] = "Strength",
            [40] = "Agility",
            [41] = "Intellect",
            [42] = "Will",
            [44] = StatUltimateDmgBonus,
            [50] = StatPhysicalDmgBonus,
            [51] = StatHeatDmgBonus,
            [52] = StatElectricDmgBonus,
            [53] = StatCryoDmgBonus,
            [54] = StatNatureDmgBonus,
            [55] = StatAetherDmgBonus,
            [87] = StatArtsIntensity
        };

        private static readonly Dictionary<string, string> _elementDamageNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cryst"] = StatCryoDmgBonus,
            ["Fire"] = StatHeatDmgBonus,
            ["Pulse"] = StatElectricDmgBonus,
            ["Natural"] = StatNatureDmgBonus,
            ["Physical"] = StatPhysicalDmgBonus
        };

        private static readonly Dictionary<string, string> _elementDamageLocKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cryst"] = "CrystDamageIncrease",
            ["Fire"] = "FireDamageIncrease",
            ["Pulse"] = "PulseDamageIncrease",
            ["Natural"] = "NaturalDamageIncrease",
            ["Physical"] = "PhysicalDamageIncrease"
        };

        private static readonly Dictionary<string, string> _statKeyLocKeys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["HP"] = "MaxHp",
            ["ATK"] = "Atk",
            ["Strength"] = "Str",
            ["Agility"] = "Agi",
            ["Intellect"] = "Wisd",
            ["Will"] = "Will",
            ["Defense"] = "Def",
            [StatCriticalRate] = "CriticalRate",
            [StatCriticalDmg] = "CriticalDamageIncrease",
            [StatTreatmentReceivedBonus] = "HealTakenIncrease",
            [StatCryoDmgBonus] = "CrystDamageIncrease",
            [StatHeatDmgBonus] = "FireDamageIncrease",
            [StatElectricDmgBonus] = "PulseDamageIncrease",
            [StatNatureDmgBonus] = "NaturalDamageIncrease",
            [StatPhysicalDmgBonus] = "PhysicalDamageIncrease",
            [StatAetherDmgBonus] = "EtherDamageIncrease"
        };

        private static readonly HashSet<string> _percentageStatKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            StatCriticalRate,
            StatCriticalDmg,
            StatTreatmentReceivedBonus,
            StatCryoDmgBonus,
            StatHeatDmgBonus,
            StatElectricDmgBonus,
            StatNatureDmgBonus,
            StatPhysicalDmgBonus,
            StatAetherDmgBonus,
            StatUltimateDmgBonus,
            StatArtsIntensity
        };

        public static bool IsPercentageAttr(int attrId)
        {
            return _percentageAttrIds.Contains(attrId);
        }

        public static bool IsPercentageStatKey(string englishKey)
        {
            if (string.IsNullOrEmpty(englishKey)) return false;
            if (_percentageStatKeys.Contains(englishKey)) return true;
            return englishKey.IndexOf("DMG", StringComparison.OrdinalIgnoreCase) >= 0
                || englishKey.IndexOf("Bonus", StringComparison.OrdinalIgnoreCase) >= 0
                || englishKey.IndexOf("Rate", StringComparison.OrdinalIgnoreCase) >= 0
                || englishKey.IndexOf("Intensity", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Dictionary<string, EFStatTotal> CalculateAllTotalStats(EFOperator op, IEFAssets assets)
        {
            var result = new Dictionary<string, EFStatTotal>(StringComparer.Ordinal);
            if (op == null || assets == null)
            {
                return result;
            }

            var calculated = EFStatsCalculator.CalculateOperatorStats(op, assets);
            double will = Math.Floor(GetAttr(calculated.Attrs, EFAttrType.Will));

            AddStat(result, "HP", calculated.Hp, assets, isPercentage: false);
            AddStat(result, "ATK", calculated.Atk, assets, isPercentage: false);
            AddStat(result, "Strength", Math.Floor(GetAttr(calculated.Attrs, EFAttrType.Strength)), assets, isPercentage: false);
            AddStat(result, "Agility", Math.Floor(GetAttr(calculated.Attrs, EFAttrType.Agility)), assets, isPercentage: false);
            AddStat(result, "Intellect", Math.Floor(GetAttr(calculated.Attrs, EFAttrType.Intellect)), assets, isPercentage: false);
            AddStat(result, "Will", will, assets, isPercentage: false);
            AddStat(result, "Defense", GetAttr(calculated.Attrs, EFAttrType.Defense), assets, isPercentage: false);
            AddStat(result, StatCriticalRate, GetAttr(calculated.Attrs, EFAttrType.CritRate) * 100.0, assets, isPercentage: true);
            AddStat(result, StatCriticalDmg, 50.0, assets, isPercentage: true);
            AddStat(result, StatTreatmentReceivedBonus, will * 0.1, assets, isPercentage: true);

            AddElementDamageBonus(result, op.Element, calculated.Attrs, assets);
            return result;
        }

        private static void AddElementDamageBonus(
            Dictionary<string, EFStatTotal> result,
            string element,
            Dictionary<int, double> attrs,
            IEFAssets assets)
        {
            string elementKey = GetElementDamageBonusKey(element);
            double elementBonus = ResolveElementBonus(attrs, ref elementKey);
            if (elementBonus != 0)
            {
                AddStat(result, elementKey, elementBonus, assets, isPercentage: true);
            }
        }

        private static double ResolveElementBonus(Dictionary<int, double> attrs, ref string elementKey)
        {
            if (attrs.TryGetValue((int)EFAttrType.CryoDMG, out double cryst) && cryst != 0)
            {
                return cryst * 100.0;
            }

            foreach (int dmgAttr in new[] { 50, 51, 52, 53, 54, 55 })
            {
                if (!attrs.TryGetValue(dmgAttr, out double bonus) || bonus == 0) continue;
                if (_attrFallbackNames.TryGetValue(dmgAttr, out string name))
                {
                    elementKey = name;
                }
                return bonus * 100.0;
            }

            return 0;
        }

        public static string GetLocalizedStatKey(string englishKey, IEFAssets assets = null)
        {
            if (string.IsNullOrEmpty(englishKey)) return string.Empty;

            if (_statKeyLocKeys.TryGetValue(englishKey, out string locKey) && assets != null)
            {
                string localized = assets.GetLocalizedText(locKey);
                if (!string.IsNullOrEmpty(localized) && !string.Equals(localized, locKey, StringComparison.Ordinal))
                {
                    return localized;
                }
            }

            return englishKey;
        }

        public static string GetAttrName(int attrId, IEFAssets assets = null)
        {
            if (_attrLocKeys.TryGetValue(attrId, out string locKey) && assets != null)
            {
                string localized = assets.GetLocalizedText(locKey);
                if (!string.IsNullOrEmpty(localized) && !string.Equals(localized, locKey, StringComparison.Ordinal))
                {
                    return localized;
                }
            }

            if (_attrFallbackNames.TryGetValue(attrId, out string fallback))
            {
                return fallback;
            }

            var mapped = EFStatsCalculator.MapAttrType(attrId);
            if (mapped != EFAttrType.Unknown)
            {
                return mapped.ToString();
            }

            return $"Attr {attrId}";
        }

        public static string FormatAttrValue(int attrId, double value, bool raw = false)
        {
            if (raw)
            {
                return value.ToString("G", CultureInfo.InvariantCulture);
            }

            if (IsPercentageAttr(attrId) || (Math.Abs(value) > 0 && Math.Abs(value) < 1))
            {
                return (value * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%";
            }

            return Math.Floor(value + 1e-9).ToString(CultureInfo.InvariantCulture);
        }

        public static string GetElementDamageBonusKey(string element)
        {
            if (!string.IsNullOrEmpty(element) && _elementDamageNames.TryGetValue(element, out string name))
            {
                return name;
            }

            return "Elemental DMG Bonus";
        }

        public static string GetElementDamageBonusName(string element, IEFAssets assets = null)
        {
            string englishKey = GetElementDamageBonusKey(element);
            if (_elementDamageLocKeys.TryGetValue(element ?? string.Empty, out string locKey) && assets != null)
            {
                string localized = assets.GetLocalizedText(locKey);
                if (!string.IsNullOrEmpty(localized) && !string.Equals(localized, locKey, StringComparison.Ordinal))
                {
                    return localized;
                }
            }

            return GetLocalizedStatKey(englishKey, assets);
        }

        private static void AddStat(
            Dictionary<string, EFStatTotal> result,
            string englishKey,
            double value,
            IEFAssets assets,
            bool isPercentage)
        {
            result[englishKey] = new EFStatTotal
            {
                Key = englishKey,
                LocalizedName = GetLocalizedStatKey(englishKey, assets),
                Value = value,
                IsPercentage = isPercentage
            };
        }

        private static double GetAttr(IDictionary<int, double> attrs, EFAttrType type)
        {
            return attrs != null && attrs.TryGetValue((int)type, out double value) ? value : 0;
        }

        public static string GetSkillDisplayName(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return string.Empty;

            if (skillId.EndsWith("_NormalAttack", StringComparison.Ordinal)) return "Basic Attack";
            if (skillId.EndsWith("_NormalSkill", StringComparison.Ordinal)) return "Battle Skill";
            if (skillId.EndsWith("_UltimateSkill", StringComparison.Ordinal)) return "Ultimate";
            if (skillId.EndsWith("_ComboSkill", StringComparison.Ordinal)) return "Combo Skill";
            int talentMarker = skillId.LastIndexOf("_talent_", StringComparison.Ordinal);
            if (talentMarker >= 0)
            {
                string talentTail = skillId.Substring(talentMarker + "_talent_".Length);
                if (int.TryParse(talentTail, NumberStyles.Integer, CultureInfo.InvariantCulture, out int talentIndex))
                {
                    return $"Talent {talentIndex + 1}";
                }
                return "Talent";
            }

            int lastUnderscore = skillId.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore < skillId.Length - 1)
            {
                return skillId.Substring(lastUnderscore + 1);
            }

            return skillId;
        }

        public static string GetEquipSlotName(EFEquipSlot slot)
        {
            switch (slot)
            {
                case EFEquipSlot.Slot0: return "Hand";
                case EFEquipSlot.Slot1: return "Body";
                case EFEquipSlot.Slot2: return "EDC";
                case EFEquipSlot.Slot3: return "EDC";
                default: return slot.ToString();
            }
        }

        public static int GetWeaponMaxLevel(int breakthroughLevel, int curveLength)
        {
            int byBreak = (breakthroughLevel + 1) * 20;
            if (curveLength <= 0) return byBreak;
            return Math.Min(curveLength, byBreak);
        }

    }
}
