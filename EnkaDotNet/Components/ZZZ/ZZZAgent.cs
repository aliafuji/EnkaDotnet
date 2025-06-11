using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.ZZZ;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZAgent
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int PromotionLevel { get; internal set; }
        public int TalentLevel { get; internal set; }
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
        public List<Assets.ZZZ.Models.ZZZAvatarColors> Colors { get; internal set; } = new List<Assets.ZZZ.Models.ZZZAvatarColors>();
        public Dictionary<StatType, double> Stats { get; internal set; } = new Dictionary<StatType, double>();

        internal EnkaClientOptions Options { get; set; }
        internal IZZZAssets Assets { get; set; }
        public Dictionary<string, Skin> Skins { get; internal set; } = new Dictionary<string, Skin>();

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
                        if (Math.Abs(numericValue) < 0.01)
                        {
                            displayValue = "0";
                        }
                        else
                        {
                            string formatted = numericValue.ToString("F2", CultureInfo.InvariantCulture);
                            displayValue = formatted.EndsWith("0") ? formatted.TrimEnd('0') : formatted;
                        }

                        if (Math.Abs(baseValue) < 0.01)
                        {
                            displayBase = "0";
                        }
                        else
                        {
                            string formattedBase = baseValue.ToString("F2", CultureInfo.InvariantCulture);
                            displayBase = formattedBase.EndsWith("0") ? formattedBase.TrimEnd('0') : formattedBase;
                        }

                        if (Math.Abs(addedValue) < 0.01)
                        {
                            displayAdded = "0";
                        }
                        else
                        {
                            string formattedAdded = addedValue.ToString("F2", CultureInfo.InvariantCulture);
                            displayAdded = formattedAdded.EndsWith("0") ? formattedAdded.TrimEnd('0') : formattedAdded;
                        }
                    }
                }
                else if (friendlyKey == "Energy Regen" && raw)
                {
                    if (Math.Abs(numericValue) < 0.01)
                    {
                        displayValue = "0";
                    }
                    else
                    {
                        string formatted = numericValue.ToString("F2", CultureInfo.InvariantCulture);
                        displayValue = formatted.EndsWith("0") ? formatted.TrimEnd('0') : formatted;
                    }

                    if (Math.Abs(baseValue) < 0.01)
                    {
                        displayBase = "0";
                    }
                    else
                    {
                        string formattedBase = baseValue.ToString("F2", CultureInfo.InvariantCulture);
                        displayBase = formattedBase.EndsWith("0") ? formattedBase.TrimEnd('0') : formattedBase;
                    }

                    if (Math.Abs(addedValue) < 0.01)
                    {
                        displayAdded = "0";
                    }
                    else
                    {
                        string formattedAdded = addedValue.ToString("F2", CultureInfo.InvariantCulture);
                        displayAdded = formattedAdded.EndsWith("0") ? formattedAdded.TrimEnd('0') : formattedAdded;
                    }
                }
                else
                {
                    displayValue = Math.Floor(numericValue).ToString(invariantCulture);
                    displayBase = Math.Floor(baseValue).ToString(invariantCulture);
                    displayAdded = Math.Floor(addedValue).ToString(invariantCulture);
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

        public List<FormattedDriveDiscSetInfo> GetEquippedDiscSets()
        {
            var result = new List<FormattedDriveDiscSetInfo>();
            bool raw = this.Options?.Raw ?? false;

            if (Assets == null)
            {
                Console.Error.WriteLine($"Warning: ZZZAgent {Name} ({Id}) has null Assets. Cannot get equipped disc sets.");
                return result;
            }
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
                    BonusStats = new List<KeyValuePair<string, string>>()
                };

                if (suitInfo?.SetBonusProps != null && suitInfo.SetBonusProps.Any())
                {
                    foreach (var prop in suitInfo.SetBonusProps)
                    {
                        if (int.TryParse(prop.Key, out int propId) && Enum.IsDefined(typeof(StatType), propId))
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
    public List<KeyValuePair<string, string>> BonusStats { get; set; } = new List<KeyValuePair<string, string>>();
    }

    public class SkinInfo
    {
        public string Image { get; set; } = string.Empty;
        public string CircleIcon { get; set; } = string.Empty;
    }
}
