using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Enums.HSR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnkaDotNet.Utils.HSR
{
    public class HSRStatCalculator
    {
        private readonly IHSRAssets _assets;
        private static readonly Dictionary<string, double> DEFAULT_STATS = new Dictionary<string, double>
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
            { "ImaginaryAddedRatio", 0 }
        };

        public HSRStatCalculator(IHSRAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public Dictionary<string, HSRStatValue> CalculateCharacterStats(HSRCharacter character)
        {
            var stats = new Dictionary<string, double>(DEFAULT_STATS);

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
            var avatarStats = _assets.GetAvatarStats(character.Id.ToString(), character.Promotion);

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
            var equipmentStats = _assets.GetEquipmentStats(lightCone.Id.ToString(), lightCone.Promotion);

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
            var skillEffects = _assets.GetEquipmentSkillProps(lightCone.Id.ToString(), lightCone.Rank);
            if (skillEffects != null)
            {
                foreach (var effect in skillEffects)
                {
                    AddStatValue(stats, effect.Key, effect.Value);
                }
            }
        }

        private void ProcessRelics(Dictionary<string, double> stats, List<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0) return;

            foreach (var relic in relics)
            {
                if (relic.MainStat != null && !string.IsNullOrEmpty(relic.MainStat.Type) && relic.MainStat.Type != "None")
                {
                    var mainAffixInfo = _assets.GetRelicMainAffixInfo(relic.Type, relic.MainStat.PropertyType.GetHashCode());
                    if (mainAffixInfo != null)
                    {
                        double value = mainAffixInfo.BaseValue + (mainAffixInfo.LevelAdd * relic.Level);
                        if (!string.IsNullOrEmpty(mainAffixInfo.Property))
                        {
                            AddStatValue(stats, mainAffixInfo.Property, value);
                        }
                    }
                    else
                    {
                        AddStatValue(stats, relic.MainStat.Type, relic.MainStat.BaseValue);
                    }
                }

                if (relic.SubStats != null)
                {
                    foreach (var subStat in relic.SubStats)
                    {
                        if (subStat != null && !string.IsNullOrEmpty(subStat.Type) && subStat.Type != "None")
                        {
                            AddStatValue(stats, subStat.Type, subStat.BaseValue);
                        }
                    }
                }
            }
        }

        private void ApplyRelicSetBonuses(Dictionary<string, double> stats, List<HSRRelic> relics)
        {
            if (relics == null || relics.Count == 0) return;

            var relicSets = new Dictionary<int, int>();
            foreach (var relic in relics)
            {
                if (relic.SetId > 0)
                {
                    if (!relicSets.ContainsKey(relic.SetId))
                    {
                        relicSets[relic.SetId] = 0;
                    }
                    relicSets[relic.SetId]++;
                }
            }

            foreach (var setPair in relicSets)
            {
                int setId = setPair.Key;
                int count = setPair.Value;

                if (count >= 2)
                {
                    var twoPieceEffects = _assets.GetRelicSetEffects(setId, 2);
                    foreach (var effect in twoPieceEffects)
                    {
                        AddStatValue(stats, effect.Key, effect.Value);
                    }
                }

                if (count >= 4)
                {
                    var fourPieceEffects = _assets.GetRelicSetEffects(setId, 4);
                    foreach (var effect in fourPieceEffects)
                    {
                        AddStatValue(stats, effect.Key, effect.Value);
                    }
                }
            }
        }

        private void ApplyTraceEffects(Dictionary<string, double> stats, List<HSRSkillTree> traces)
        {
            if (traces == null || traces.Count == 0) return;

            foreach (var trace in traces)
            {
                if (trace.Level <= 0) continue;

                var traceEffects = _assets.GetSkillTreeProps(trace.PointId.ToString(), trace.Level);
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

            if (stats.ContainsKey(statType))
            {
                stats[statType] += value;
            }
        }

        private Dictionary<string, HSRStatValue> CalculateFinalStats(Dictionary<string, double> stats)
        {
            var finalStats = new Dictionary<string, HSRStatValue>();

            // HP calculation
            double baseHP = stats.GetValueOrDefault("HPBase");
            double hpAddedRatio = stats.GetValueOrDefault("HPAddedRatio");
            double hpDelta = stats.GetValueOrDefault("HPDelta");
            double finalHP = Math.Floor(baseHP * (1.0 + hpAddedRatio) + hpDelta);
            finalStats["HP"] = new HSRStatValue(finalHP, false, 0);

            // ATK calculation
            double baseAtk = stats.GetValueOrDefault("AttackBase");
            double atkAddedRatio = stats.GetValueOrDefault("AttackAddedRatio");
            double atkDelta = stats.GetValueOrDefault("AttackDelta");
            double finalATK = Math.Floor(baseAtk * (1.0 + atkAddedRatio) + atkDelta);
            finalStats["Attack"] = new HSRStatValue(finalATK, false, 0);

            // DEF calculation
            double baseDef = stats.GetValueOrDefault("DefenceBase");
            double defAddedRatio = stats.GetValueOrDefault("DefenceAddedRatio");
            double defDelta = stats.GetValueOrDefault("DefenceDelta");
            double finalDEF = Math.Floor(baseDef * (1.0 + defAddedRatio) + defDelta);
            finalStats["Defense"] = new HSRStatValue(finalDEF, false, 0);

            // SPD calculation
            double baseSpd = stats.GetValueOrDefault("SpeedBase");
            double spdDelta = stats.GetValueOrDefault("SpeedDelta");
            double spdAddedRatio = stats.GetValueOrDefault("SpeedAddedRatio");
            double finalSPD = Math.Round((baseSpd * (1.0 + spdAddedRatio) + spdDelta) * 10.0) / 10.0;
            finalStats["Speed"] = new HSRStatValue(finalSPD, false, 1);

            // CRIT Rate calculation
            double criticalChance = stats.GetValueOrDefault("CriticalChance");
            double criticalChanceBase = stats.GetValueOrDefault("CriticalChanceBase");
            double finalCritRate = Math.Floor((criticalChance + criticalChanceBase) * 1000.0) / 10.0;
            finalStats["CritRate"] = new HSRStatValue(finalCritRate, true, 1);

            // CRIT DMG calculation
            double criticalDamage = stats.GetValueOrDefault("CriticalDamage");
            double criticalDamageBase = stats.GetValueOrDefault("CriticalDamageBase");
            double finalCritDMG = Math.Floor((criticalDamage + criticalDamageBase) * 1000.0) / 10.0;
            finalStats["CritDMG"] = new HSRStatValue(finalCritDMG, true, 1);

            // Break Effect calculation
            double breakDamage = stats.GetValueOrDefault("BreakDamageAddedRatio");
            double breakDamageBase = stats.GetValueOrDefault("BreakDamageAddedRatioBase");
            double finalBreakEffect = Math.Floor((breakDamage + breakDamageBase) * 1000.0) / 10.0;
            finalStats["BreakEffect"] = new HSRStatValue(finalBreakEffect, true, 1);

            // Healing Boost calculation
            double healRatio = stats.GetValueOrDefault("HealRatioBase");
            double finalHealingBoost = Math.Floor(healRatio * 1000.0) / 10.0;
            finalStats["HealingBoost"] = new HSRStatValue(finalHealingBoost, true, 1);

            // Energy Regen Rate calculation
            double spRatio = stats.GetValueOrDefault("SPRatioBase");
            double finalEnergyRegenRate = Math.Floor((1.0 + spRatio) * 1000.0) / 10.0;
            finalStats["EnergyRegenRate"] = new HSRStatValue(finalEnergyRegenRate, true, 1);

            // Effect Hit Rate calculation
            double statusProbability = stats.GetValueOrDefault("StatusProbability");
            double statusProbabilityBase = stats.GetValueOrDefault("StatusProbabilityBase");
            double finalEffectHitRate = Math.Floor((statusProbability + statusProbabilityBase) * 1000.0) / 10.0;
            finalStats["EffectHitRate"] = new HSRStatValue(finalEffectHitRate, true, 1);

            // Effect Resistance calculation
            double statusResistance = stats.GetValueOrDefault("StatusResistance");
            double statusResistanceBase = stats.GetValueOrDefault("StatusResistanceBase");
            double finalEffectResistance = Math.Floor((statusResistance + statusResistanceBase) * 1000.0) / 10.0;
            finalStats["EffectResistance"] = new HSRStatValue(finalEffectResistance, true, 1);

            // Elemental DMG Boosts
            Dictionary<string, string> elementMapping = new Dictionary<string, string>
            {
                {"Physical", "PhysicalDamageBoost"},
                {"Fire", "FireDamageBoost"},
                {"Ice", "IceDamageBoost"},
                {"Thunder", "LightningDamageBoost"},
                {"Wind", "WindDamageBoost"},
                {"Quantum", "QuantumDamageBoost"},
                {"Imaginary", "ImaginaryDamageBoost"}
            };

            foreach (var elem in elementMapping)
            {
                string propName = $"{elem.Key}AddedRatio";
                double valueDecimal = stats.GetValueOrDefault(propName, 0);
                double valuePercent = Math.Floor(valueDecimal * 1000.0) / 10.0;
                finalStats[elem.Value] = new HSRStatValue(valuePercent, true, 1);
            }

            return finalStats;
        }
    }
}