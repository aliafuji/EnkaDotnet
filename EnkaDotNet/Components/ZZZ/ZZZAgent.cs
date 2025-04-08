using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.ZZZ;
using System.Collections.Generic;

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

        public ZZZWEngine? Weapon { get; internal set; }
        public Dictionary<SkillType, int> SkillLevels { get; internal set; } = new Dictionary<SkillType, int>();
        public List<ZZZDriveDisc> EquippedDiscs { get; internal set; } = new List<ZZZDriveDisc>();

        public Rarity Rarity { get; internal set; }
        public ProfessionType ProfessionType { get; internal set; }
        public List<ElementType> ElementTypes { get; internal set; } = new List<ElementType>();
        public string ImageUrl { get; internal set; } = string.Empty;
        public string CircleIconUrl { get; internal set; } = string.Empty;

        public Dictionary<StatType, double> Stats { get; internal set; } = new Dictionary<StatType, double>();

        private Dictionary<string, ZZZStatValue> _cachedTotalStats;

        public Dictionary<string, ZZZStatValue> GetAllStats()
        {
            if (_cachedTotalStats != null)
                return _cachedTotalStats;

            _cachedTotalStats = new Dictionary<string, ZZZStatValue>();
            var rawTotals = ZZZStatsHelpers.CalculateAllTotalStats(this);

            foreach (var stat in rawTotals)
            {
                bool isPercentage = ZZZStatsHelpers.IsDisplayPercentageStatForGroup(stat.Key);
                bool isEnergyRegen = stat.Key == "Energy Regen";

                _cachedTotalStats[stat.Key] = new ZZZStatValue(
                    stat.Value,
                    isPercentage,
                    isEnergyRegen
                );
            }

            return _cachedTotalStats;
        }

        public ZZZStatValue GetStat(string statName)
        {
            var stats = GetAllStats();
            return stats.TryGetValue(statName, out var stat) ? stat : new ZZZStatValue(0);
        }

        public List<DriveDiscSetInfo> GetEquippedDiscSets()
        {
            return ZZZDriveDiscSetHelper.GetEquippedDiscSets(this);
        }

        public StatBreakdown GetStatBreakdown(string statName)
        {
            return ZZZStatsHelpers.GetStatSourceBreakdown(this, statName);
        }
    }
}