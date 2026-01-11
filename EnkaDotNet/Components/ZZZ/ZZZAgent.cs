using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.ZZZ;
using EnkaDotNet.Utils.Common;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZAgent
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int PromotionLevel { get; internal set; }
        public int TalentLevel { get; internal set; }
        public IReadOnlyList<int> CoreSkillEnhancements { get; internal set; } = new List<int>();
        public int CoreSkillEnhancement { get; internal set; }

        public int PotentialId { get; internal set; }
        public int Potential => PotentialId == 0 ? 0 : (PotentialId % 10) + 1;
        
        public IReadOnlyList<int> TalentToggles { get; internal set; } = new List<int>();
        public WEngineEffectState WeaponEffectState { get; internal set; }
        public bool IsHidden { get; internal set; }
        public IReadOnlyList<int> ClaimedRewards { get; internal set; } = new List<int>();
        public DateTimeOffset ObtainmentTimestamp { get; internal set; }

        public ZZZWEngine Weapon { get; internal set; }
        public ConcurrentDictionary<SkillType, int> SkillLevels { get; internal set; } = new ConcurrentDictionary<SkillType, int>();
        public IReadOnlyList<ZZZDriveDisc> EquippedDiscs { get; internal set; } = new List<ZZZDriveDisc>();

        public Rarity Rarity { get; internal set; }
        public ProfessionType ProfessionType { get; internal set; }
        public IReadOnlyList<ElementType> ElementTypes { get; internal set; } = new List<ElementType>();
        public string ImageUrl { get; internal set; } = string.Empty;
        public string CircleIconUrl { get; internal set; } = string.Empty;
        public IReadOnlyList<Assets.ZZZ.Models.ZZZAvatarColors> Colors { get; internal set; } = new List<Assets.ZZZ.Models.ZZZAvatarColors>();
        public ConcurrentDictionary<StatType, double> Stats { get; internal set; } = new ConcurrentDictionary<StatType, double>();

        internal EnkaClientOptions Options { get; set; }
        internal IZZZAssets Assets { get; set; }
        public ConcurrentDictionary<string, Skin> Skins { get; internal set; } = new ConcurrentDictionary<string, Skin>();

        public Dictionary<string, FormattedStatValues> GetAllStats()
        {
            bool raw = this.Options?.Raw ?? false;
            var resultStats = new Dictionary<string, FormattedStatValues>();

            if (this.Assets == null)
            {
                Console.Error.WriteLine($"Warning: ZZZAgent {Name} ({Id}) has null Assets. Cannot calculate stats.");
                return resultStats;
            }
            var calculatedStats = ZZZStatsHelpers.CalculateAllTotalStats(this, this.Assets);

            foreach (var statPair in calculatedStats)
            {
                string friendlyKey = statPair.Key;
                double numericValue = statPair.Value.FinalValue;
                double baseValue = statPair.Value.BaseValue;
                double addedValue = statPair.Value.AddedValue;
                bool isPercentageDisplay = ZZZStatsHelpers.IsDisplayPercentageStatForGroup(friendlyKey);

                string displayKey = raw ? ZZZStatsHelpers.GetStatTypeFromFriendlyName(friendlyKey, isPercentageDisplay, friendlyKey == "Energy Regen").ToString() : friendlyKey;
                string displayValue;
                string displayBase;
                string displayAdded;

                CultureInfo invariantCulture = CultureInfo.InvariantCulture;

                if (isPercentageDisplay)
                {
                    displayValue = numericValue.ToString("F1", invariantCulture) + (raw ? "" : "%");
                    displayBase = baseValue.ToString("F1", invariantCulture) + (raw ? "" : "%");
                    displayAdded = addedValue.ToString("F1", invariantCulture) + (raw ? "" : "%");
                    
                    if (friendlyKey == "Energy Regen" && !raw)
                    {
                        displayValue = Math.Abs(numericValue) < 0.01 ? "0" : (numericValue.ToString("F2", invariantCulture).TrimEnd('0'));
                        displayBase = Math.Abs(baseValue) < 0.01 ? "0" : (baseValue.ToString("F2", invariantCulture).TrimEnd('0'));
                        displayAdded = Math.Abs(addedValue) < 0.01 ? "0" : (addedValue.ToString("F2", invariantCulture).TrimEnd('0'));
                    }
                }
                else if (friendlyKey == "Energy Regen" && raw)
                {
                    displayValue = Math.Abs(numericValue) < 0.01 ? "0" : (numericValue.ToString("F2", invariantCulture).TrimEnd('0'));
                    displayBase = Math.Abs(baseValue) < 0.01 ? "0" : (baseValue.ToString("F2", invariantCulture).TrimEnd('0'));
                    displayAdded = Math.Abs(addedValue) < 0.01 ? "0" : (addedValue.ToString("F2", invariantCulture).TrimEnd('0'));
                }
                else
                {
                    displayValue = numericValue.ToString("F0", invariantCulture);
                    displayBase = baseValue.ToString("F0", invariantCulture);
                    displayAdded = addedValue.ToString("F0", invariantCulture);
                }

                resultStats[displayKey] = new FormattedStatValues
                {
                    Final = displayValue,
                    Base = displayBase,
                    Added = displayAdded
                };
            }
            return resultStats;
        }

        /// <summary>
        /// Gets the final calculated stats as raw numeric values.
        /// Returns a dictionary with stat names as keys and final values as doubles.
        /// </summary>
        public Dictionary<string, double> GetFinalStats()
        {
            var finalStats = new Dictionary<string, double>();

            if (this.Assets == null)
            {
                Console.Error.WriteLine($"Warning: ZZZAgent {Name} ({Id}) has null Assets. Cannot calculate stats.");
                return finalStats;
            }

            var calculatedStats = ZZZStatsHelpers.CalculateAllTotalStats(this, this.Assets);
            foreach (var stat in calculatedStats)
            {
                finalStats[stat.Key] = stat.Value.FinalValue;
            }

            return finalStats;
        }

        public IReadOnlyList<FormattedDriveDiscSetInfo> GetEquippedDiscSets()
        {
            var result = new List<FormattedDriveDiscSetInfo>();
            bool raw = this.Options?.Raw ?? false;

            if (Assets == null)
            {
                Console.Error.WriteLine($"Warning: ZZZAgent {Name} ({Id}) has null Assets. Cannot get equipped disc sets.");
                return result;
            }
            if (EquippedDiscs.Count < 2) return result;

            var discSetsGrouped = new Dictionary<int, List<ZZZDriveDisc>>();
            foreach (var disc in EquippedDiscs)
            {
                if (!discSetsGrouped.ContainsKey(disc.SuitId))
                {
                    discSetsGrouped[disc.SuitId] = new List<ZZZDriveDisc>();
                }
                discSetsGrouped[disc.SuitId].Add(disc);
            }


            foreach (var group in discSetsGrouped.Values)
            {
                if (group.Count < 2) continue;

                var firstDisc = group[0];
                var suitInfo = Assets.GetDiscSetInfo(firstDisc.SuitId.ToString());

                var setInfo = new FormattedDriveDiscSetInfo
                {
                    SuitName = firstDisc.SuitName,
                    PieceCount = group.Count,
                    BonusStats = new List<KeyValuePair<string, string>>()
                };

                if (suitInfo?.SetBonusProps != null && suitInfo.SetBonusProps.Count > 0)
                {
                    foreach (var prop in suitInfo.SetBonusProps)
                    {
                        if (int.TryParse(prop.Key, out int propId) && EnumHelper.IsDefinedZZZStatType(propId))
                        {
                            var statType = (StatType)propId;
                            double numericValue = prop.Value;
                            string key = raw ? statType.ToString() : ZZZStatsHelpers.GetStatCategoryDisplay(statType);
                            string value;

                            bool isDisplayPercent = ZZZStatsHelpers.IsDisplayPercentageStat(statType);

                            if (raw)
                            {
                                value = (isDisplayPercent ? numericValue / 100.0 : numericValue).ToString("F1", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                value = isDisplayPercent ? $"{(numericValue / 100.0):F1}%" : $"{Math.Floor(numericValue)}";
                            }
                            setInfo.BonusStats.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
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
        public ICollection<KeyValuePair<string, string>> BonusStats { get; set; } = new List<KeyValuePair<string, string>>();
    }
}
