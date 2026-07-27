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
            [9] = "Critical Rate",
            [17] = "Arts Intensity",
            [29] = "Treatment Bonus",
            [39] = "Strength",
            [40] = "Agility",
            [41] = "Intellect",
            [42] = "Will",
            [44] = "Ultimate DMG Bonus",
            [50] = "Physical DMG Bonus",
            [51] = "Heat DMG Bonus",
            [52] = "Electric DMG Bonus",
            [53] = "Cryo DMG Bonus",
            [54] = "Nature DMG Bonus",
            [55] = "Æther DMG Bonus",
            [87] = "Arts Intensity"
        };

        private static readonly Dictionary<string, string> _elementDamageNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cryst"] = "Cryo DMG Bonus",
            ["Fire"] = "Heat DMG Bonus",
            ["Pulse"] = "Electric DMG Bonus",
            ["Natural"] = "Nature DMG Bonus",
            ["Physical"] = "Physical DMG Bonus"
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
            ["Critical Rate"] = "CriticalRate",
            ["Critical DMG"] = "CriticalDamageIncrease",
            ["Treatment Received Bonus"] = "HealTakenIncrease",
            ["Cryo DMG Bonus"] = "CrystDamageIncrease",
            ["Heat DMG Bonus"] = "FireDamageIncrease",
            ["Electric DMG Bonus"] = "PulseDamageIncrease",
            ["Nature DMG Bonus"] = "NaturalDamageIncrease",
            ["Physical DMG Bonus"] = "PhysicalDamageIncrease",
            ["Æther DMG Bonus"] = "EtherDamageIncrease"
        };

        private static readonly HashSet<string> _percentageStatKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "Critical Rate",
            "Critical DMG",
            "Treatment Received Bonus",
            "Cryo DMG Bonus",
            "Heat DMG Bonus",
            "Electric DMG Bonus",
            "Nature DMG Bonus",
            "Physical DMG Bonus",
            "Æther DMG Bonus",
            "Ultimate DMG Bonus",
            "Arts Intensity"
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
            AddStat(result, "Critical Rate", GetAttr(calculated.Attrs, EFAttrType.CritRate) * 100.0, assets, isPercentage: true);
            AddStat(result, "Critical DMG", 50.0, assets, isPercentage: true);
            AddStat(result, "Treatment Received Bonus", will * 0.1, assets, isPercentage: true);

            string elementKey = GetElementDamageBonusKey(op.Element);
            double elementBonus = 0;
            if (calculated.Attrs.TryGetValue((int)EFAttrType.CryoDMG, out double cryst) && cryst != 0)
            {
                elementBonus = cryst * 100.0;
            }
            else
            {
                foreach (int dmgAttr in new[] { 50, 51, 52, 53, 54, 55 })
                {
                    if (calculated.Attrs.TryGetValue(dmgAttr, out double bonus) && bonus != 0)
                    {
                        elementKey = _attrFallbackNames.TryGetValue(dmgAttr, out string name) ? name : elementKey;
                        elementBonus = bonus * 100.0;
                        break;
                    }
                }
            }

            if (elementBonus != 0)
            {
                AddStat(result, elementKey, elementBonus, assets, isPercentage: true);
            }

            return result;
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
