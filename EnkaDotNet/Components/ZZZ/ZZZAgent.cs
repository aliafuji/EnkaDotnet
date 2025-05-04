using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.ZZZ;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.ZZZ.Models;
using System.Globalization;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZAgent
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int PromotionLevel { get; internal set; }
        public int TalentLevel { get; internal set; }
        public int SkinId { get; internal set; }
        public List<int> CoreSkillEnhancements { get; internal set; } = new List<int>();
        public int CoreSkillEnhancement { get; internal set; }
        public List<int> TalentToggles { get; internal set; } = new List<int>();
        public WEngineEffectState WeaponEffectState { get; internal set; }
        public bool IsHidden { get; internal set; }
        public List<int> ClaimedRewards { get; internal set; } = new List<int>();
        public DateTimeOffset ObtainmentTimestamp { get; internal set; }

        public ZZZWEngine Weapon { get; internal set; }
        public Dictionary<SkillType, int> SkillLevels { get; internal set; } = new Dictionary<SkillType, int>();
        public List<ZZZDriveDisc> EquippedDiscs { get; internal set; } = new List<ZZZDriveDisc>();

        public Rarity Rarity { get; internal set; }
        public ProfessionType ProfessionType { get; internal set; }
        public List<ElementType> ElementTypes { get; internal set; } = new List<ElementType>();
        public string ImageUrl { get; internal set; } = string.Empty;
        public string CircleIconUrl { get; internal set; } = string.Empty;

        public List<ZZZAvatarColors> Colors { get; internal set; } = new List<ZZZAvatarColors>();

        public Dictionary<StatType, double> Stats { get; internal set; } = new Dictionary<StatType, double>();

        internal EnkaClientOptions Options { get; set; }
        internal IZZZAssets Assets { get; set; }

        public Dictionary<string, string> GetAllStats()
        {
            bool raw = this.Options?.Raw ?? false;
            var resultStats = new Dictionary<string, string>();
            var calculatedStats = ZZZStatsHelpers.CalculateAllTotalStats(this);

            foreach (var statPair in calculatedStats)
            {
                string friendlyKey = statPair.Key;
                double numericValue = statPair.Value;
                bool isPercentage = ZZZStatsHelpers.IsDisplayPercentageStatForGroup(friendlyKey);
                bool isEnergyRegen = friendlyKey == "Energy Regen";
                StatType statType = ZZZStatsHelpers.GetStatTypeFromFriendlyName(friendlyKey, isPercentage, isEnergyRegen);

                string displayKey;
                string displayValue;

                if (raw)
                {
                    displayKey = statType.ToString();
                    if (isEnergyRegen)
                    {
                        string formatted = numericValue.ToString("F2", CultureInfo.InvariantCulture);
                        displayValue = formatted.EndsWith("0") ? formatted.TrimEnd('0') : formatted;
                    }
                    else if (isPercentage) displayValue = numericValue.ToString("F1", CultureInfo.InvariantCulture);
                    else displayValue = Math.Floor(numericValue).ToString();
                }
                else
                {
                    displayKey = friendlyKey;
                    if (isEnergyRegen)
                    {
                        string formatted = numericValue.ToString("F2", CultureInfo.InvariantCulture);
                        displayValue = formatted.EndsWith("0") ? formatted.TrimEnd('0') : formatted;
                    }
                    else if (isPercentage) displayValue = numericValue.ToString("F1", CultureInfo.InvariantCulture) + "%";
                    else displayValue = Math.Floor(numericValue).ToString();
                }
                resultStats[displayKey] = displayValue;
            }
            return resultStats;
        }

        public List<FormattedDriveDiscSetInfo> GetEquippedDiscSets()
        {
            var result = new List<FormattedDriveDiscSetInfo>();
            bool raw = this.Options?.Raw ?? false;

            if (Assets == null) return result;
            if (EquippedDiscs.Count < 2) return result;

            var discSetsGrouped = EquippedDiscs
                .GroupBy(d => d.SuitId)
                .Where(g => g.Count() >= 2)
                .Select(g => new { SuitId = g.Key, Count = g.Count(), Discs = g.ToList() })
                .ToList();

            foreach (var set in discSetsGrouped)
            {
                var firstDisc = set.Discs.First();
                var suitInfo = Assets.GetDiscSetInfo(set.SuitId.ToString());

                var setInfo = new FormattedDriveDiscSetInfo
                {
                    SuitName = firstDisc.SuitName,
                    PieceCount = set.Count,
                };

                if (suitInfo?.SetBonusProps != null && suitInfo.SetBonusProps.Any())
                {
                    setInfo.BonusStats = suitInfo.SetBonusProps
                        .Where(prop => int.TryParse(prop.Key, out int propId) && Enum.IsDefined(typeof(StatType), propId))
                        .Select(prop =>
                        {
                            var statType = (StatType)int.Parse(prop.Key);
                            string key;
                            string value;
                            double numericValue = prop.Value;

                            if (raw)
                            {
                                key = statType.ToString();
                                if (statType == StatType.EnergyRegenPercent) value = Math.Floor(numericValue).ToString();
                                else if (ZZZStatsHelpers.IsCalculationPercentageStat(statType)) value = (numericValue).ToString("F1", CultureInfo.InvariantCulture);
                                else value = Math.Floor(numericValue).ToString();
                            }
                            else
                            {
                                key = ZZZStatsHelpers.GetStatCategoryDisplay(statType);
                                if (statType == StatType.EnergyRegenPercent) value = $"{(numericValue / 100.0):F1}%";
                                else if (ZZZStatsHelpers.IsDisplayPercentageStat(statType)) value = $"{(numericValue / 100.0):F1}%";
                                else value = $"{Math.Floor(numericValue)}";
                            }
                            return new KeyValuePair<string, string>(key, value);
                        }).ToList();
                }
                result.Add(setInfo);
            }
            return result;
        }
    }

    public class FormattedDriveDiscSetInfo
    {
        public string SuitName { get; set; } = string.Empty;
        public int PieceCount { get; set; }
        public List<KeyValuePair<string, string>> BonusStats { get; set; } = new List<KeyValuePair<string, string>>();
    }
}