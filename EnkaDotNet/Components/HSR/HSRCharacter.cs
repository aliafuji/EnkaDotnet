using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Utils.HSR;


namespace EnkaDotNet.Components.HSR
{
    public class SetBonusEffectDetail
    {
        public string PropertyName { get; set; } = string.Empty;
        public string FormattedValue { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public double RawValue { get; set; }
    }

    public class HSRCharacter
    {
        private IHSRAssets _assets;

        internal void SetAssets(IHSRAssets assets)
        {
            _assets = assets;
        }

        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Promotion { get; internal set; }
        public int Rank { get; internal set; }
        public List<HSRSkillTree> SkillTreeList { get; internal set; } = new List<HSRSkillTree>();
        public bool IsAssist { get; internal set; }
        public int Position { get; internal set; }
        public ElementType Element { get; internal set; }
        public PathType Path { get; internal set; }
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public string AvatarIconUrl { get; internal set; } = string.Empty;
        public HSRLightCone Equipment { get; internal set; }
        public List<HSRRelic> RelicList { get; internal set; } = new List<HSRRelic>();
        public List<Eidolon> Eidolons { get; internal set; } = new List<Eidolon>();
        public Dictionary<string, HSRStatValue> Stats { get; internal set; } = new Dictionary<string, HSRStatValue>();

        public HSRStatValue HP => GetStat("HP");
        public HSRStatValue Attack => GetStat("Attack");
        public HSRStatValue Defense => GetStat("Defense");
        public HSRStatValue Speed => GetStat("Speed");
        public HSRStatValue CritRate => GetStat("CritRate");
        public HSRStatValue CritDMG => GetStat("CritDMG");
        public HSRStatValue BreakEffect => GetStat("BreakEffect");
        public HSRStatValue OutgoingHealingBoost => GetStat("HealingBoost");
        public HSRStatValue EnergyRegenRate => GetStat("EnergyRegenRate");
        public HSRStatValue EffectHitRate => GetStat("EffectHitRate");
        public HSRStatValue EffectResistance => GetStat("EffectResistance");
        public HSRStatValue PhysicalDamageBoost => GetStat("PhysicalDamageBoost");
        public HSRStatValue FireDamageBoost => GetStat("FireDamageBoost");
        public HSRStatValue IceDamageBoost => GetStat("IceDamageBoost");
        public HSRStatValue LightningDamageBoost => GetStat("LightningDamageBoost");
        public HSRStatValue WindDamageBoost => GetStat("WindDamageBoost");
        public HSRStatValue QuantumDamageBoost => GetStat("QuantumDamageBoost");
        public HSRStatValue ImaginaryDamageBoost => GetStat("ImaginaryDamageBoost");


        public HSRStatValue GetStat(string statName)
        {
            return Stats.TryGetValue(statName, out var statValue)
                ? statValue
                : new HSRStatValue(0);
        }

        public List<HSRRelicSetBonus> GetEquippedRelicSets()
        {
            if (_assets == null)
            {
                Console.WriteLine($"Warning: IHSRAssets instance not available in HSRCharacter {Name} ({Id}) for GetEquippedRelicSets.");
                return new List<HSRRelicSetBonus>();
            }
            if (RelicList == null || RelicList.Count == 0) return new List<HSRRelicSetBonus>();

            var setCount = new Dictionary<int, int>();
            var setNames = new Dictionary<int, string>();

            foreach (var relic in RelicList)
            {
                if (relic.SetId > 0)
                {
                    if (!setCount.ContainsKey(relic.SetId))
                    {
                        setCount[relic.SetId] = 0;
                        setNames[relic.SetId] = relic.SetName;
                    }
                    setCount[relic.SetId]++;
                }
            }

            var setBonusesResult = new List<HSRRelicSetBonus>();
            foreach (var entry in setCount)
            {
                int setId = entry.Key;
                int count = entry.Value;
                string setName = setNames.TryGetValue(setId, out var name) ? name : $"Set {setId}";

                var currentSetBonus = new HSRRelicSetBonus
                {
                    SetId = setId,
                    SetName = setName,
                    PieceCount = count,
                    Effects = new List<SetBonusEffectDetail>()
                };

                Action<int> addEffects = (pieceReq) =>
                {
                    var effectsDict = _assets.GetRelicSetEffects(setId, pieceReq);
                    if (effectsDict != null && effectsDict.Count > 0)
                    {
                        foreach (var kvp in effectsDict)
                        {
                            string propertyType = kvp.Key;
                            double rawValue = kvp.Value;
                            string displayName = HSRStatPropertyUtils.GetDisplayName(propertyType);
                            string formattedValue = HSRStatPropertyUtils.FormatPropertyValue(propertyType, rawValue);

                            currentSetBonus.Effects.Add(new SetBonusEffectDetail
                            {
                                PropertyType = propertyType,
                                PropertyName = displayName,
                                RawValue = rawValue,
                                FormattedValue = formattedValue
                            });
                        }
                    }
                };

                if (count >= 2)
                {
                    addEffects(2);
                }

                if (count >= 4)
                {
                    addEffects(4);
                }

                if (currentSetBonus.Effects.Any())
                {
                    setBonusesResult.Add(currentSetBonus);
                }
            }

            return setBonusesResult;
        }
    }

    public class HSRRelicSetBonus
    {
        public int SetId { get; set; }
        public string SetName { get; set; } = string.Empty;
        public int PieceCount { get; set; }
        public List<SetBonusEffectDetail> Effects { get; set; } = new List<SetBonusEffectDetail>();
    }

    public class Eidolon
    {
        public int Id { get; internal set; }
        public string Icon { get; internal set; } = string.Empty;
        public bool Unlocked { get; internal set; }
    }
}
