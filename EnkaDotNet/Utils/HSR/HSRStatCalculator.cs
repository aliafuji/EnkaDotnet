using System;
using System.Collections.Generic;
using System.Globalization;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Components.HSR;
using Microsoft.Extensions.Options;

namespace EnkaDotNet.Utils.HSR
{
    public class HSRStatCalculator
    {
        private readonly IHSRAssets _assets;
        private readonly EnkaClientOptions _options;
        private static readonly Dictionary<string, double> _defaultStats = new Dictionary<string, double>
        {
            { "HPBase", 0 }, { "HPDelta", 0 }, { "HPAddedRatio", 0 },
            { "AttackBase", 0 }, { "AttackDelta", 0 }, { "AttackAddedRatio", 0 },
            { "DefenceBase", 0 }, { "DefenceDelta", 0 }, { "DefenceAddedRatio", 0 },
            { "SpeedBase", 0 }, { "SpeedDelta", 0 }, { "SpeedAddedRatio", 0 },
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
            { "ElationAddedRatio", 0 }
        };

        /// <summary>
        /// Maps each element's raw added-ratio stat key to the final stat name it is reported as.
        /// Every key here must also exist in <see cref="_defaultStats"/>, otherwise
        /// <see cref="AddStatValue"/> discards the incoming value.
        /// </summary>
        private static readonly Dictionary<string, string> _elementDamageBoostStats = new Dictionary<string, string>
        {
            { "PhysicalAddedRatio", "PhysicalDamageBoost" },
            { "FireAddedRatio", "FireDamageBoost" },
            { "IceAddedRatio", "IceDamageBoost" },
            { "ThunderAddedRatio", "LightningDamageBoost" },
            { "WindAddedRatio", "WindDamageBoost" },
            { "QuantumAddedRatio", "QuantumDamageBoost" },
            { "ImaginaryAddedRatio", "ImaginaryDamageBoost" },
            { "ElationAddedRatio", "ElationDamageBoost" }
        };

        /// <summary>
        /// Number of entries <see cref="CalculateFinalStats"/> writes on top of <see cref="_elementDamageBoostStats"/>.
        /// </summary>
        private const int FinalStatCount = 11;

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
            if (avatarStats != null)
            {
                stats["HPBase"] = avatarStats.HPBase + (avatarStats.HPAdd * (character.Level - 1));
                stats["AttackBase"] = avatarStats.AttackBase + (avatarStats.AttackAdd * (character.Level - 1));
                stats["DefenceBase"] = avatarStats.DefenceBase + (avatarStats.DefenceAdd * (character.Level - 1));
                stats["SpeedBase"] = avatarStats.SpeedBase;
                stats["CriticalChanceBase"] = avatarStats.CriticalChance;
                stats["CriticalDamageBase"] = avatarStats.CriticalDamage;
            }
        }

        private void AddLightConeBaseStats(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            var equipmentStats = _assets.GetEquipmentStats(lightCone.Id.ToString(CultureInfo.InvariantCulture), lightCone.Promotion);
            if (equipmentStats != null)
            {
                double lcHP = equipmentStats.BaseHP + (equipmentStats.HPAdd * (lightCone.Level - 1));
                double lcAttack = equipmentStats.BaseAttack + (equipmentStats.AttackAdd * (lightCone.Level - 1));
                double lcDefence = equipmentStats.BaseDefence + (equipmentStats.DefenceAdd * (lightCone.Level - 1));
                stats["HPBase"] += lcHP;
                stats["AttackBase"] += lcAttack;
                stats["DefenceBase"] += lcDefence;
                lightCone.BaseHP = lcHP;
                lightCone.BaseAttack = lcAttack;
                lightCone.BaseDefense = lcDefence;
            }
        }

        private void AddLightConePassiveStats(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            foreach (var prop in lightCone.Properties)
            {
                if (prop.Type == "BaseHP" || prop.Type == "BaseAttack" || prop.Type == "BaseDefence") continue;
                AddStatValue(stats, prop.Type, prop.Value);
            }
        }

        private void AddLightConeSkillEffects(Dictionary<string, double> stats, HSRLightCone lightCone)
        {
            var skillEffects = _assets.GetEquipmentSkillProps(lightCone.Id.ToString(CultureInfo.InvariantCulture), lightCone.Rank);
            if (skillEffects != null)
            {
                foreach (var effect in skillEffects)
                {
                    AddStatValue(stats, effect.Key, effect.Value);
                }
            }
        }

        private void ProcessRelics(Dictionary<string, double> stats, IReadOnlyList<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0) return;
            foreach (var relic in relics)
            {
                if (relic.MainStat != null && !string.IsNullOrEmpty(relic.MainStat.Type) && relic.MainStat.Type != "None")
                {
                    var mainAffixInfo = _assets.GetRelicMainAffixInfo(relic.Type, (int)relic.MainStat.PropertyType);
                    if (mainAffixInfo != null && !string.IsNullOrEmpty(mainAffixInfo.Property))
                    {
                        double value = mainAffixInfo.BaseValue + (mainAffixInfo.LevelAdd * relic.Level);
                        AddStatValue(stats, mainAffixInfo.Property, value);
                    }
                    else
                    {
                        AddStatValue(stats, relic.MainStat.Type, relic.MainStat.Value);
                    }
                }
                if (relic.SubStats != null)
                {
                    foreach (var subStat in relic.SubStats)
                    {
                        if (subStat != null && !string.IsNullOrEmpty(subStat.Type) && subStat.Type != "None")
                        {
                            AddStatValue(stats, subStat.Type, subStat.Value);
                        }
                    }
                }
            }
        }

        private void ApplyRelicSetBonuses(Dictionary<string, double> stats, IReadOnlyList<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0) return;
            var relicSets = new Dictionary<int, int>(relics.Count);
            foreach (var relic in relics)
            {
                if (relic.SetId > 0)
                {
                    relicSets.TryGetValue(relic.SetId, out int pieceCount);
                    relicSets[relic.SetId] = pieceCount + 1;
                }
            }
            foreach (var setPair in relicSets)
            {
                int setId = setPair.Key;
                int count = setPair.Value;
                if (count >= 2)
                {
                    var twoPieceEffects = _assets.GetRelicSetEffects(setId, 2);
                    if (twoPieceEffects != null)
                    {
                        foreach (var effect in twoPieceEffects)
                        {
                            AddStatValue(stats, effect.Key, effect.Value);
                        }
                    }
                }
                if (count >= 4)
                {
                    var fourPieceEffects = _assets.GetRelicSetEffects(setId, 4);
                    if (fourPieceEffects != null)
                    {
                        foreach (var effect in fourPieceEffects)
                        {
                            AddStatValue(stats, effect.Key, effect.Value);
                        }
                    }
                }
            }
        }

        private void ApplyTraceEffects(Dictionary<string, double> stats, IReadOnlyList<HSRSkillTree> traces)
        {
            if (traces == null || traces.Count == 0) return;
            foreach (var trace in traces)
            {
                if (trace.Level <= 0) continue;
                var traceEffects = _assets.GetSkillTreeProps(trace.PointId.ToString(CultureInfo.InvariantCulture), trace.Level);
                if (traceEffects != null)
                {
                    foreach (var effect in traceEffects)
                    {
                        AddStatValue(stats, effect.Key, effect.Value);
                    }
                }
            }
        }

        private void AddStatValue(Dictionary<string, double> stats, string statType, double value)
        {
            if (string.IsNullOrEmpty(statType) || statType == "None") return;

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
            double baseHP = stats.TryGetValue("HPBase", out var hpBaseValue) ? hpBaseValue : 0;
            double hpAddedRatio = stats.TryGetValue("HPAddedRatio", out var hpAddedRatioValue) ? hpAddedRatioValue : 0;
            double hpDelta = stats.TryGetValue("HPDelta", out var hpDeltaValue) ? hpDeltaValue : 0;
            double finalHP = Math.Floor(baseHP * (1.0 + hpAddedRatio) + hpDelta);
            finalStats["HP"] = new HSRStatValue(finalHP, _options, false, 0);
            double baseAtk = stats.TryGetValue("AttackBase", out var atkBaseValue) ? atkBaseValue : 0;
            double atkAddedRatio = stats.TryGetValue("AttackAddedRatio", out var atkAddedRatioValue) ? atkAddedRatioValue : 0;
            double atkDelta = stats.TryGetValue("AttackDelta", out var atkDeltaValue) ? atkDeltaValue : 0;
            double finalATK = Math.Floor(baseAtk * (1.0 + atkAddedRatio) + atkDelta);
            finalStats["Attack"] = new HSRStatValue(finalATK, _options, false, 0);
            double baseDef = stats.TryGetValue("DefenceBase", out var defBaseValue) ? defBaseValue : 0;
            double defAddedRatio = stats.TryGetValue("DefenceAddedRatio", out var defAddedRatioValue) ? defAddedRatioValue : 0;
            double defDelta = stats.TryGetValue("DefenceDelta", out var defDeltaValue) ? defDeltaValue : 0;
            double finalDEF = Math.Floor(baseDef * (1.0 + defAddedRatio) + defDelta);
            finalStats["Defense"] = new HSRStatValue(finalDEF, _options, false, 0);
            double baseSpd = stats.TryGetValue("SpeedBase", out var spdBaseValue) ? spdBaseValue : 0;
            double spdDelta = stats.TryGetValue("SpeedDelta", out var spdDeltaValue) ? spdDeltaValue : 0;
            double spdAddedRatio = stats.TryGetValue("SpeedAddedRatio", out var spdAddedRatioValue) ? spdAddedRatioValue : 0;
            double rawSpeed = (baseSpd * (1.0 + spdAddedRatio) + spdDelta);
            double finalSPD = Math.Floor(rawSpeed * 10) / 10;
            finalStats["Speed"] = new HSRStatValue(finalSPD, _options, false, 1);
            double criticalChance = stats.TryGetValue("CriticalChance", out var criticalChanceValue) ? criticalChanceValue : 0;
            double criticalChanceBase = stats.TryGetValue("CriticalChanceBase", out var criticalChanceBaseValue) ? criticalChanceBaseValue : 0;
            double rawCritRate = (criticalChance + criticalChanceBase) * 100.0;
            double finalCritRate = Math.Floor(rawCritRate * 10) / 10;
            finalStats["CritRate"] = new HSRStatValue(finalCritRate, _options, true, 1);
            double criticalDamage = stats.TryGetValue("CriticalDamage", out var criticalDamageValue) ? criticalDamageValue : 0;
            double criticalDamageBase = stats.TryGetValue("CriticalDamageBase", out var criticalDamageBaseValue) ? criticalDamageBaseValue : 0;
            double rawCritDMG = (criticalDamage + criticalDamageBase) * 100.0;
            double finalCritDMG = Math.Floor(rawCritDMG * 10) / 10;
            finalStats["CritDMG"] = new HSRStatValue(finalCritDMG, _options, true, 1);
            double breakDamage = stats.TryGetValue("BreakDamageAddedRatio", out var breakDamageValue) ? breakDamageValue : 0;
            double breakDamageBase = stats.TryGetValue("BreakDamageAddedRatioBase", out var breakDamageBaseValue) ? breakDamageBaseValue : 0;
            double rawValue = (breakDamage + breakDamageBase) * 100.0;
            double finalBreakEffect = Math.Floor(rawValue * 10) / 10;
            finalStats["BreakEffect"] = new HSRStatValue(finalBreakEffect, _options, true, 1);
            double healRatio = stats.TryGetValue("HealRatioBase", out var healRatioValue) ? healRatioValue : 0;
            double rawHealingBoost = healRatio * 100.0;
            double finalHealingBoost = Math.Floor(rawHealingBoost * 10) / 10;
            finalStats["HealingBoost"] = new HSRStatValue(finalHealingBoost, _options, true, 1);
            double spRatio = stats.TryGetValue("SPRatioBase", out var spRatioValue) ? spRatioValue : 0;
            double rawEnergyRegenRate = (1.0 + spRatio) * 100.0;
            double finalEnergyRegenRate = Math.Floor(rawEnergyRegenRate * 10) / 10;
            finalStats["EnergyRegenRate"] = new HSRStatValue(finalEnergyRegenRate, _options, true, 1);
            double statusProbability = stats.TryGetValue("StatusProbability", out var statusProbabilityValue) ? statusProbabilityValue : 0;
            double statusProbabilityBase = stats.TryGetValue("StatusProbabilityBase", out var statusProbabilityBaseValue) ? statusProbabilityBaseValue : 0;
            double rawEffectHitRate = (statusProbability + statusProbabilityBase) * 100.0;
            double finalEffectHitRate = Math.Floor(rawEffectHitRate * 10) / 10;
            finalStats["EffectHitRate"] = new HSRStatValue(finalEffectHitRate, _options, true, 1);
            double statusResistance = stats.TryGetValue("StatusResistance", out var statusResistanceValue) ? statusResistanceValue : 0;
            double statusResistanceBase = stats.TryGetValue("StatusResistanceBase", out var statusResistanceBaseValue) ? statusResistanceBaseValue : 0;
            double rawEffectResistance = (statusResistance + statusResistanceBase) * 100.0;
            double finalEffectResistance = Math.Floor(rawEffectResistance * 10) / 10;
            finalStats["EffectResistance"] = new HSRStatValue(finalEffectResistance, _options, true, 1);
            foreach (var elem in _elementDamageBoostStats)
            {
                double valueDecimal = stats.TryGetValue(elem.Key, out var elementRatio) ? elementRatio : 0;
                double valuePercent = valueDecimal * 100.0;
                double finalValue = Math.Floor(valuePercent * 10) / 10;
                finalStats[elem.Value] = new HSRStatValue(finalValue, _options, true, 1);
            }
            return finalStats;
        }
    }
}