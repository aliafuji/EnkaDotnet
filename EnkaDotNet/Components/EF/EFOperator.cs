using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Utils.EF;

namespace EnkaDotNet.Components.EF
{
    public class EFOperator
    {
        public int Id { get; internal set; }
        public string StrId { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int PotentialLevel { get; internal set; }
        public int Rarity { get; internal set; }
        public string Element { get; internal set; } = string.Empty;
        public string Profession { get; internal set; } = string.Empty;
        public string WeaponType { get; internal set; } = string.Empty;
        public string IconUrl { get; internal set; } = string.Empty;
        public string RoundIconUrl { get; internal set; } = string.Empty;
        public string SplashArtUrl { get; internal set; } = string.Empty;
        public string SilhouetteUrl { get; internal set; } = string.Empty;
        public string ProfessionIconUrl { get; internal set; } = string.Empty;
        public ConcurrentDictionary<EFAttrType, double> Stats { get; internal set; } = new ConcurrentDictionary<EFAttrType, double>();
        public ConcurrentDictionary<int, double> RawAttrs { get; internal set; } = new ConcurrentDictionary<int, double>();
        public double Hp { get; internal set; }
        public double Atk { get; internal set; }
        public IReadOnlyList<EFSkill> Skills { get; internal set; } = new List<EFSkill>();
        public IReadOnlyList<EFTalent> Talents { get; internal set; } = new List<EFTalent>();
        public EFWeapon Weapon { get; internal set; }
        public IReadOnlyList<EFEquip> Equips { get; internal set; } = new List<EFEquip>();
        public IReadOnlyList<string> AttrNodes { get; internal set; } = new List<string>();
        public IReadOnlyList<string> PassiveSkillNodes { get; internal set; } = new List<string>();
        public IReadOnlyList<string> FactorySkillNodes { get; internal set; } = new List<string>();
        public string LatestBreakNode { get; internal set; } = string.Empty;
        public int EquipMedicineId { get; internal set; }

        internal EnkaClientOptions Options { get; set; }
        internal IEFAssets Assets { get; set; }

        public Dictionary<string, EFStatTotal> CalculateAllTotalStats()
        {
            if (Assets == null)
            {
                return new Dictionary<string, EFStatTotal>(StringComparer.Ordinal);
            }

            return EFStatsHelpers.CalculateAllTotalStats(this, Assets);
        }

        public Dictionary<string, double> GetFinalStats()
        {
            return GetFinalStats(Options?.UseRawStatValues ?? false);
        }

        public Dictionary<string, double> GetFinalStats(bool raw)
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var pair in CalculateAllTotalStats())
            {
                result[pair.Key] = ResolveNumericValue(pair.Value, raw);
            }
            return result;
        }

        public Dictionary<string, string> GetAllStats()
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            bool raw = Options?.UseRawStatValues ?? false;
            foreach (var stat in CalculateAllTotalStats().Values)
            {
                string key = raw ? stat.Key : stat.LocalizedName;
                result[key] = FormatStatValue(stat, raw);
            }
            return result;
        }

        private static double ResolveNumericValue(EFStatTotal stat, bool raw)
        {
            if (stat.IsPercentage)
            {
                return raw
                    ? stat.Value / 100.0
                    : Math.Round(stat.Value, 1, MidpointRounding.AwayFromZero);
            }

            return raw ? stat.Value : Math.Floor(stat.Value + 1e-9);
        }

        private static string FormatStatValue(EFStatTotal stat, bool raw)
        {
            if (stat.IsPercentage)
            {
                if (raw)
                {
                    return (stat.Value / 100.0).ToString("F4", CultureInfo.InvariantCulture);
                }

                return stat.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%";
            }

            return Math.Floor(stat.Value + 1e-9).ToString(CultureInfo.InvariantCulture);
        }
    }
}
