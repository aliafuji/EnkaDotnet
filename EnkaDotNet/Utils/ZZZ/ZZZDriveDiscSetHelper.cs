using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public static class ZZZDriveDiscSetHelper
    {
        private const int MIN_SET_PIECES = 2;
        private static readonly IZZZAssets assets = new ZZZAssets();

        public static List<DriveDiscSetInfo> GetEquippedDiscSets(this ZZZAgent agent)
        {
            var result = new List<DriveDiscSetInfo>();

            if (agent.EquippedDiscs.Count < MIN_SET_PIECES)
                return result;

            var discSets = agent.EquippedDiscs
                .GroupBy(d => d.SuitId)
                .Where(g => g.Count() >= MIN_SET_PIECES)
                .Select(g => new { SuitId = g.Key, Count = g.Count(), Discs = g.ToList() })
                .ToList();

            foreach (var set in discSets)
            {
                var firstDisc = set.Discs.First();
                var suitInfo = assets.GetDiscSetInfo(set.SuitId.ToString());

                var setInfo = new DriveDiscSetInfo
                {
                    SuitId = set.SuitId,
                    SuitName = firstDisc.SuitName,
                    PieceCount = set.Count,
                    Discs = set.Discs
                };

                if (suitInfo?.SetBonusProps != null && suitInfo.SetBonusProps.Any())
                {
                    var bonusDescriptions = new List<string>();

                    foreach (var prop in suitInfo.SetBonusProps)
                    {
                        string formattedValue = string.Empty;
                        if (int.TryParse(prop.Key, out int propId) && Enum.IsDefined(typeof(StatType), propId))
                        {
                            string propName = assets.GetPropertyName(propId);
                            double rawValue = prop.Value;
                            StatType statType = (StatType)propId;

                            if (ZZZStatsHelpers.IsDisplayPercentageStat(statType))
                            {
                                formattedValue = $"{(rawValue / 100):F0}%";
                            }
                            else
                            {
                                formattedValue = $"{Math.Round(rawValue):F0}";
                            }
                            bonusDescriptions.Add($"{propName} +{formattedValue}");
                        }
                    }
                    setInfo.BonusDescription = string.Join(", ", bonusDescriptions);
                    setInfo.BonusStats = suitInfo.SetBonusProps
                        .Where(prop => int.TryParse(prop.Key, out int propId) && Enum.IsDefined(typeof(StatType), propId))
                        .Select(prop =>
                        {
                            var statType = (StatType)int.Parse(prop.Key);
                            string valueDisplay;
                            if (ZZZStatsHelpers.IsDisplayPercentageStat(statType))
                            {
                                valueDisplay = $"{(prop.Value / 100.0):F1}%"; // Show decimal for percentages
                            }
                            else
                            {
                                valueDisplay = $"{Math.Floor((double)prop.Value)}"; // Floor for non-percentages
                            }

                            return new BonusStats
                            {
                                StatType = statType,
                                Value = valueDisplay,
                                Description = setInfo.BonusDescription
                            };

                        }).ToList();
                }
                else
                {
                    setInfo.BonusDescription = "No set bonus information available";
                }

                result.Add(setInfo);
            }

            return result;
        }
    }

    public class DriveDiscSetInfo
    {
        public int SuitId { get; set; }
        public string SuitName { get; set; } = string.Empty;
        public int PieceCount { get; set; }
        public List<ZZZDriveDisc> Discs { get; set; } = new List<ZZZDriveDisc>();
        public List<BonusStats> BonusStats { get; set; } = new List<BonusStats>();
        public string BonusDescription { get; set; } = string.Empty;
    }

    public class BonusStats
    {
        public StatType StatType { get; set; }
        public string Value { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}