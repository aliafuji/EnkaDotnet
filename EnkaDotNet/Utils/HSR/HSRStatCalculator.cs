using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Components.HSR;
using Microsoft.Extensions.Options;

namespace EnkaDotNet.Utils.HSR
{
    // Acronym casing matches the rest of the HSR surface (HSRAssets, HSRDataMapper, ...).
    public class HSRStatCalculator // NOSONAR
    {
        private const string HpBase = "HPBase";
        private const string AttackBase = "AttackBase";
        private const string DefenceBase = "DefenceBase";
        private const string SpeedBase = "SpeedBase";
        private const string HpDelta = "HPDelta";
        private const string AttackDelta = "AttackDelta";
        private const string DefenceDelta = "DefenceDelta";
        private const string SpeedDelta = "SpeedDelta";
        private const string HpAddedRatio = "HPAddedRatio";
        private const string AttackAddedRatio = "AttackAddedRatio";
        private const string DefenceAddedRatio = "DefenceAddedRatio";
        private const string SpeedAddedRatio = "SpeedAddedRatio";

        private readonly IHSRAssets _assets;
        private readonly EnkaClientOptions _options;
        private static readonly Dictionary<string, double> _defaultStats = new Dictionary<string, double>
        {
            { HpBase, 0 }, { HpDelta, 0 }, { HpAddedRatio, 0 },
            { AttackBase, 0 }, { AttackDelta, 0 }, { AttackAddedRatio, 0 },
            { DefenceBase, 0 }, { DefenceDelta, 0 }, { DefenceAddedRatio, 0 },
            { SpeedBase, 0 }, { SpeedDelta, 0 }, { SpeedAddedRatio, 0 },
            { "CriticalChance", 0 }, { "CriticalChanceBase", 0 },
            { "CriticalDamage", 0 }, { "CriticalDamageBase", 0 },
            { "BreakDamageAddedRatio", 0 }, { "BreakDamageAddedRatioBase", 0 },
            { "HealRatioBase", 0 },
            { "SPRatioBase", 0 },
            { "StatusProbability", 0 }, { "StatusProbabilityBase", 0 },
            { "StatusResistance", 0 }, { "StatusResistanceBase", 0 },
            { "PhysicalAddedRatio", 0 },
            { "FireAddedRatio", 0 },
            { "IceAddedRatio", 0 },
            { "ThunderAddedRatio", 0 },
            { "WindAddedRatio", 0 },
            { "QuantumAddedRatio", 0 },
            { "ImaginaryAddedRatio", 0 },
            { "ElationDamageAddedRatio", 0 },
            { "ElationDamageAddedRatioBase", 0 }
        };

        /// <summary>
        /// Maps each element's raw added-ratio stat key to the final stat name it is reported as.
        /// Every key here must also exist in <see cref="_defaultStats"/>, otherwise
        /// <see cref="AddStatValue"/> discards the incoming value.
        /// Elation is handled separately (Base + AddedRatio) like BreakEffect.
        /// </summary>
        private static readonly Dictionary<string, string> _elementDamageBoostStats = new Dictionary<string, string>
        {
            { "PhysicalAddedRatio", "PhysicalDamageBoost" },
            { "FireAddedRatio", "FireDamageBoost" },
            { "IceAddedRatio", "IceDamageBoost" },
            { "ThunderAddedRatio", "LightningDamageBoost" },
            { "WindAddedRatio", "WindDamageBoost" },
            { "QuantumAddedRatio", "QuantumDamageBoost" },
            { "ImaginaryAddedRatio", "ImaginaryDamageBoost" }
        };

        /// <summary>
        /// Number of non element entries <see cref="CalculateFinalStats"/> writes
        /// (includes <c>ElationDamageBoost</c>).
        /// </summary>
        private const int FinalStatCount = 12;

        public HSRStatCalculator(IHSRAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public HSRStatCalculator(IHSRAssets assets, IOptions<EnkaClientOptions> options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Dictionary<string, HSRStatValue> CalculateCharacterStats(HSRCharacter character)
        {
            var stats = new Dictionary<string, double>(_defaultStats);
            AddCharacterBaseStats(stats, character);
            if (character.Equipment != null)
            {
                AddLightConeBaseStats(stats, character.Equipment);
                AddLightConePassiveStats(stats, character.Equipment);
                AddLightConeSkillEffects(stats, character.Equipment);
            }
            ProcessRelics(stats, character.RelicList);
            ApplyTraceEffects(stats, character.SkillTreeList);
            ApplyRelicSetBonuses(stats, character.RelicList);
            return CalculateFinalStats(stats);
        }

        private void AddCharacterBaseStats(Dictionary<string, double> stats, HSRCharacter character)
        {
            var avatarStats = _assets.GetAvatarStats(character.Id.ToString(CultureInfo.InvariantCulture), character.Promotion);
            if (avatarStats == null)
            {
                return;
            }

            stats[HpBase] = avatarStats.HPBase + (avatarStats.HPAdd * (character.Level - 1));
            stats[AttackBase] = avatarStats.AttackBase + (avatarStats.AttackAdd * (character.Level - 1));
            stats[DefenceBase] = avatarStats.DefenceBase + (avatarStats.DefenceAdd * (character.Level - 1));
            stats[SpeedBase] = avatarStats.SpeedBase;
            stats["CriticalChanceBase"] = avatarStats.CriticalChance;
            stats["CriticalDamageBase"] = avatarStats.CriticalDamage;
        }

        private void AddLightConeBaseStats(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            var equipmentStats = _assets.GetEquipmentStats(lightCone.Id.ToString(CultureInfo.InvariantCulture), lightCone.Promotion);
            if (equipmentStats == null)
            {
                return;
            }

            double lcHP = equipmentStats.BaseHP + (equipmentStats.HPAdd * (lightCone.Level - 1));
            double lcAttack = equipmentStats.BaseAttack + (equipmentStats.AttackAdd * (lightCone.Level - 1));
            double lcDefence = equipmentStats.BaseDefence + (equipmentStats.DefenceAdd * (lightCone.Level - 1));
            stats[HpBase] += lcHP;
            stats[AttackBase] += lcAttack;
            stats[DefenceBase] += lcDefence;
            lightCone.BaseHP = lcHP;
            lightCone.BaseAttack = lcAttack;
            lightCone.BaseDefense = lcDefence;
        }

        private static void AddLightConePassiveStats(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            foreach (var prop in lightCone.Properties)
            {
                if (prop.Type == "BaseHP" || prop.Type == "BaseAttack" || prop.Type == "BaseDefence")
                {
                    continue;
                }

                AddStatValue(stats, prop.Type, prop.Value);
            }
        }

        private void AddLightConeSkillEffects(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            var skillEffects = _assets.GetEquipmentSkillProps(lightCone.Id.ToString(CultureInfo.InvariantCulture), lightCone.Rank);
            ApplyEffectMap(stats, skillEffects);
        }

        private void ProcessRelics(Dictionary<string, double> stats, IReadOnlyList<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0)
            {
                return;
            }

            foreach (var relic in relics)
            {
                ApplyRelicMainStat(stats, relic);
                ApplyRelicSubStats(stats, relic);
            }
        }

        private void ApplyRelicMainStat(Dictionary<string, double> stats, HSRRelic relic)
        {
            if (relic.MainStat == null || string.IsNullOrEmpty(relic.MainStat.Type) || relic.MainStat.Type == "None")
            {
                return;
            }

            var mainAffixInfo = _assets.GetRelicMainAffixInfo(relic.Type, (int)relic.MainStat.PropertyType);
            if (mainAffixInfo != null && !string.IsNullOrEmpty(mainAffixInfo.Property))
            {
                double value = mainAffixInfo.BaseValue + (mainAffixInfo.LevelAdd * relic.Level);
                AddStatValue(stats, mainAffixInfo.Property, value);
                return;
            }

            AddStatValue(stats, relic.MainStat.Type, relic.MainStat.Value);
        }

        private static void ApplyRelicSubStats(Dictionary<string, double> stats, HSRRelic relic)
        {
            if (relic.SubStats == null)
            {
                return;
            }

            foreach (var subStat in relic.SubStats)
            {
                if (subStat == null || string.IsNullOrEmpty(subStat.Type) || subStat.Type == "None")
                {
                    continue;
                }

                AddStatValue(stats, subStat.Type, subStat.Value);
            }
        }

        private void ApplyRelicSetBonuses(Dictionary<string, double> stats, IReadOnlyList<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0)
            {
                return;
            }

            var relicSets = new Dictionary<int, int>(relics.Count);
            foreach (var setId in relics.Where(r => r.SetId > 0).Select(r => r.SetId))
            {
                relicSets.TryGetValue(setId, out int pieceCount);
                relicSets[setId] = pieceCount + 1;
            }

            foreach (var setPair in relicSets)
            {
                ApplySetEffects(stats, setPair.Key, setPair.Value, requiredPieces: 2);
                ApplySetEffects(stats, setPair.Key, setPair.Value, requiredPieces: 4);
            }
        }

        private void ApplySetEffects(Dictionary<string, double> stats, int setId, int pieceCount, int requiredPieces)
        {
            if (pieceCount < requiredPieces)
            {
                return;
            }

            ApplyEffectMap(stats, _assets.GetRelicSetEffects(setId, requiredPieces));
        }

        private void ApplyTraceEffects(Dictionary<string, double> stats, IReadOnlyList<HSRSkillTree> traces)
        {
            if (traces == null || traces.Count == 0)
            {
                return;
            }

            foreach (var trace in traces)
            {
                if (trace.Level <= 0)
                {
                    continue;
                }

                var traceEffects = _assets.GetSkillTreeProps(
                    trace.PointId.ToString(CultureInfo.InvariantCulture),
                    trace.Level);
                ApplyEffectMap(stats, traceEffects);
            }
        }

        private static void ApplyEffectMap(Dictionary<string, double> stats, Dictionary<string, double> effects)
        {
            if (effects == null)
            {
                return;
            }

            foreach (var effect in effects)
            {
                AddStatValue(stats, effect.Key, effect.Value);
            }
        }

        private static void AddStatValue(Dictionary<string, double> stats, string statType, double value)
        {
            if (string.IsNullOrEmpty(statType) || statType == "None")
            {
                return;
            }

            // Legacy alias seen in older tooling; game/meta uses ElationDamageAddedRatio.
            if (statType == "ElationAddedRatio")
            {
                statType = "ElationDamageAddedRatio";
            }

            // Only keys seeded from _defaultStats are accumulated: anything else is a stat the
            // library does not model yet and is intentionally ignored rather than surfaced raw.
            if (stats.TryGetValue(statType, out var current))
            {
                stats[statType] = current + value;
            }
        }

        private Dictionary<string, HSRStatValue> CalculateFinalStats(Dictionary<string, double> stats)
        {
            var finalStats = new Dictionary<string, HSRStatValue>(FinalStatCount + _elementDamageBoostStats.Count);

            AddScaledBaseStat(finalStats, "HP", GetStat(stats, HpBase), GetStat(stats, HpAddedRatio), GetStat(stats, HpDelta));
            AddScaledBaseStat(finalStats, "Attack", GetStat(stats, AttackBase), GetStat(stats, AttackAddedRatio), GetStat(stats, AttackDelta));
            AddScaledBaseStat(finalStats, "Defense", GetStat(stats, DefenceBase), GetStat(stats, DefenceAddedRatio), GetStat(stats, DefenceDelta));

            double rawSpeed = GetStat(stats, SpeedBase) * (1.0 + GetStat(stats, SpeedAddedRatio)) + GetStat(stats, SpeedDelta);
            finalStats["Speed"] = FlatStat(Floor1(rawSpeed), decimalPlaces: 1);

            AddCombinedPercentStat(finalStats, "CritRate", GetStat(stats, "CriticalChance"), GetStat(stats, "CriticalChanceBase"));
            AddCombinedPercentStat(finalStats, "CritDMG", GetStat(stats, "CriticalDamage"), GetStat(stats, "CriticalDamageBase"));
            AddCombinedPercentStat(finalStats, "BreakEffect", GetStat(stats, "BreakDamageAddedRatio"), GetStat(stats, "BreakDamageAddedRatioBase"));
            AddCombinedPercentStat(finalStats, "EffectHitRate", GetStat(stats, "StatusProbability"), GetStat(stats, "StatusProbabilityBase"));
            AddCombinedPercentStat(finalStats, "EffectResistance", GetStat(stats, "StatusResistance"), GetStat(stats, "StatusResistanceBase"));

            finalStats["HealingBoost"] = PercentStat(Floor1(GetStat(stats, "HealRatioBase") * 100.0));
            finalStats["EnergyRegenRate"] = PercentStat(Floor1((1.0 + GetStat(stats, "SPRatioBase")) * 100.0));

            foreach (var elem in _elementDamageBoostStats)
            {
                finalStats[elem.Value] = PercentStat(Floor1(GetStat(stats, elem.Key) * 100.0));
            }

            AddCombinedPercentStat(
                finalStats,
                "ElationDamageBoost",
                GetStat(stats, "ElationDamageAddedRatio"),
                GetStat(stats, "ElationDamageAddedRatioBase"));

            return finalStats;
        }

        private void AddScaledBaseStat(
            Dictionary<string, HSRStatValue> finalStats,
            string name,
            double baseValue,
            double addedRatio,
            double delta)
        {
            finalStats[name] = FlatStat(Math.Floor(baseValue * (1.0 + addedRatio) + delta));
        }

        private void AddCombinedPercentStat(
            Dictionary<string, HSRStatValue> finalStats,
            string name,
            double primary,
            double secondary)
        {
            finalStats[name] = PercentStat(Floor1((primary + secondary) * 100.0));
        }

        private static double GetStat(Dictionary<string, double> stats, string key)
            => stats.TryGetValue(key, out var value) ? value : 0;

        private static double Floor1(double value) => Math.Floor(value * 10) / 10;

        private HSRStatValue FlatStat(double value, int decimalPlaces = 0)
            => new HSRStatValue(value, _options, false, decimalPlaces);

        private HSRStatValue PercentStat(double value)
            => new HSRStatValue(value, _options, true, 1);
    }
}
