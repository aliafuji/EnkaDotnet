using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Enums.HSR;
using System.Globalization;

namespace EnkaDotNet.Utils.HSR
{
    public class HSRStatCalculator
    {
        private readonly IHSRAssets _assets;
        private readonly EnkaClientOptions _options;
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

        public HSRStatCalculator(IHSRAssets assets, EnkaClientOptions options)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
            else
            {
            }
        }

        private Dictionary<string, HSRStatValue> CalculateFinalStats(Dictionary<string, double> stats)
        {
            var finalStats = new Dictionary<string, HSRStatValue>();

            double baseHP = stats.ContainsKey("HPBase") ? stats["HPBase"] : 0;
            double hpAddedRatio = stats.ContainsKey("HPAddedRatio") ? stats["HPAddedRatio"] : 0;
            double hpDelta = stats.ContainsKey("HPDelta") ? stats["HPDelta"] : 0;
            double finalHP = Math.Floor(baseHP * (1.0 + hpAddedRatio) + hpDelta);
            finalStats["HP"] = new HSRStatValue(finalHP, _options, false, 0);

            double baseAtk = stats.ContainsKey("AttackBase") ? stats["AttackBase"] : 0;
            double atkAddedRatio = stats.ContainsKey("AttackAddedRatio") ? stats["AttackAddedRatio"] : 0;
            double atkDelta = stats.ContainsKey("AttackDelta") ? stats["AttackDelta"] : 0;
            double finalATK = Math.Floor(baseAtk * (1.0 + atkAddedRatio) + atkDelta);
            finalStats["Attack"] = new HSRStatValue(finalATK, _options, false, 0);

            double baseDef = stats.ContainsKey("DefenceBase") ? stats["DefenceBase"] : 0;
            double defAddedRatio = stats.ContainsKey("DefenceAddedRatio") ? stats["DefenceAddedRatio"] : 0;
            double defDelta = stats.ContainsKey("DefenceDelta") ? stats["DefenceDelta"] : 0;
            double finalDEF = Math.Floor(baseDef * (1.0 + defAddedRatio) + defDelta);
            finalStats["Defense"] = new HSRStatValue(finalDEF, _options, false, 0);

            double baseSpd = stats.ContainsKey("SpeedBase") ? stats["SpeedBase"] : 0;
            double spdDelta = stats.ContainsKey("SpeedDelta") ? stats["SpeedDelta"] : 0;
            double spdAddedRatio = stats.ContainsKey("SpeedAddedRatio") ? stats["SpeedAddedRatio"] : 0;
            double finalSPD = Math.Round((baseSpd * (1.0 + spdAddedRatio) + spdDelta), 1);
            finalStats["Speed"] = new HSRStatValue(finalSPD, _options, false, 1);

            double criticalChance = stats.ContainsKey("CriticalChance") ? stats["CriticalChance"] : 0;
            double criticalChanceBase = stats.ContainsKey("CriticalChanceBase") ? stats["CriticalChanceBase"] : 0;
            double finalCritRate = Math.Round((criticalChance + criticalChanceBase) * 100.0, 1);
            finalStats["CritRate"] = new HSRStatValue(finalCritRate, _options, true, 1);

            double criticalDamage = stats.ContainsKey("CriticalDamage") ? stats["CriticalDamage"] : 0;
            double criticalDamageBase = stats.ContainsKey("CriticalDamageBase") ? stats["CriticalDamageBase"] : 0;
            double finalCritDMG = Math.Round((criticalDamage + criticalDamageBase) * 100.0, 1);
            finalStats["CritDMG"] = new HSRStatValue(finalCritDMG, _options, true, 1);

            double breakDamage = stats.ContainsKey("BreakDamageAddedRatio") ? stats["BreakDamageAddedRatio"] : 0;
            double breakDamageBase = stats.ContainsKey("BreakDamageAddedRatioBase") ? stats["BreakDamageAddedRatioBase"] : 0;
            double finalBreakEffect = Math.Round((breakDamage + breakDamageBase) * 100.0, 1);
            finalStats["BreakEffect"] = new HSRStatValue(finalBreakEffect, _options, true, 1);

            double healRatio = stats.ContainsKey("HealRatioBase") ? stats["HealRatioBase"] : 0;
            double finalHealingBoost = Math.Round(healRatio * 100.0, 1);
            finalStats["HealingBoost"] = new HSRStatValue(finalHealingBoost, _options, true, 1);

            double spRatio = stats.ContainsKey("SPRatioBase") ? stats["SPRatioBase"] : 0;
            double finalEnergyRegenRate = Math.Round((1.0 + spRatio) * 100.0, 1);
            finalStats["EnergyRegenRate"] = new HSRStatValue(finalEnergyRegenRate, _options, true, 1);

            double statusProbability = stats.ContainsKey("StatusProbability") ? stats["StatusProbability"] : 0;
            double statusProbabilityBase = stats.ContainsKey("StatusProbabilityBase") ? stats["StatusProbabilityBase"] : 0;
            double finalEffectHitRate = Math.Round((statusProbability + statusProbabilityBase) * 100.0, 1);
            finalStats["EffectHitRate"] = new HSRStatValue(finalEffectHitRate, _options, true, 1);

            double statusResistance = stats.ContainsKey("StatusResistance") ? stats["StatusResistance"] : 0;
            double statusResistanceBase = stats.ContainsKey("StatusResistanceBase") ? stats["StatusResistanceBase"] : 0;
            double finalEffectResistance = Math.Round((statusResistance + statusResistanceBase) * 100.0, 1);
            finalStats["EffectResistance"] = new HSRStatValue(finalEffectResistance, _options, true, 1);

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
                double valueDecimal = stats.ContainsKey(propName) ? stats[propName] : 0;
                double valuePercent = Math.Round(valueDecimal * 100.0, 1);
                finalStats[elem.Value] = new HSRStatValue(valuePercent, _options, true, 1);
            }

            return finalStats;
        }
    }
}