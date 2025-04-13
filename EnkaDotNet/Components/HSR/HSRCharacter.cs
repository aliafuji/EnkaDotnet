using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Components.HSR
{
    public class HSRCharacter
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Promotion { get; internal set; }
        public int Rank { get; internal set; }  // Eidolon level
        public List<HSRSkillTree> SkillTreeList { get; internal set; } = new List<HSRSkillTree>();

        public bool IsAssist { get; internal set; }
        public int Position { get; internal set; }

        public ElementType Element { get; internal set; }
        public PathType Path { get; internal set; }
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public string AvatarIconUrl { get; internal set; } = string.Empty;

        public HSRLightCone? Equipment { get; internal set; }
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
            var setCount = new Dictionary<int, int>();
            var setNames = new Dictionary<int, string>();

            foreach (var relic in RelicList)
            {
                if (relic.SetId > 0)
                {
                    if (setCount.ContainsKey(relic.SetId))
                    {
                        setCount[relic.SetId]++;
                    }
                    else
                    {
                        setCount[relic.SetId] = 1;
                        setNames[relic.SetId] = relic.SetName;
                    }
                }
            }

            var setBonuses = new List<HSRRelicSetBonus>();
            foreach (var entry in setCount)
            {
                if (entry.Value >= 2)
                {
                    setBonuses.Add(new HSRRelicSetBonus
                    {
                        SetId = entry.Key,
                        SetName = setNames.TryGetValue(entry.Key, out var name) ? name : $"Set {entry.Key}",
                        PieceCount = entry.Value,
                        Effects = new List<string>()
                    });
                }
            }

            return setBonuses;
        }
    }

    public class HSRRelicSetBonus
    {
        public int SetId { get; set; }
        public string SetName { get; set; } = string.Empty;
        public int PieceCount { get; set; }
        public List<string> Effects { get; set; } = new List<string>();
    }

    public class Eidolon
    {
        public int Id { get; internal set; }
        public string Icon { get; internal set; } = string.Empty;
        public bool Unlocked { get; internal set; }
    }
}