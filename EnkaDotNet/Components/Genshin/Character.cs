using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class Character
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Ascension { get; internal set; }
        public int Friendship { get; internal set; }
        public ElementType Element { get; internal set; }
        public Dictionary<StatType, double> Stats { get; internal set; } = new();
        public List<int> UnlockedConstellationIds { get; internal set; } = new();
        public int ConstellationLevel { get; internal set; }
        public List<Talent> Talents { get; internal set; } = new();
        public List<Constellation> Constellations { get; internal set; } = new();
        public Weapon? Weapon { get; internal set; }
        public List<Artifact> Artifacts { get; internal set; } = new();
        public int CostumeId { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public double GetStatValue(StatType statType, out string value)
        {
            if (Stats.TryGetValue(statType, out double RawValue))
            {
                bool isPercent = statType.ToString().Contains("Bonus") ||
                                 statType == StatType.CriticalRate ||
                                 statType == StatType.CriticalDamage ||
                                 statType == StatType.EnergyRecharge;

                if (isPercent)
                {
                    double percentValue = RawValue * 100;
                    if (Math.Abs(percentValue - Math.Round(percentValue)) < 0.001)
                        value = $"{Math.Round(percentValue)}";
                    else
                        value = $"{percentValue:F1}";
                }
                else
                {
                    if (Math.Abs(RawValue - Math.Round(RawValue)) < 0.001)
                        value = $"{Math.Round(RawValue)}";
                    else
                        value = $"{RawValue:F1}";
                }

                return RawValue;
            }
            else
            {
                value = "0";
                return 0.0;
            }
        }
        public Dictionary<string, Dictionary<StatType, (double Raw, string Formatted)>> GetStats()
        {
            var result = new Dictionary<string, Dictionary<StatType, (double Raw, string Formatted)>>();

            result["Base Stats"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Percentage Stats"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Critical Stats"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Energy & Mastery"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Healing"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Resistances"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Damage Bonuses"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Final Stats"] = new Dictionary<StatType, (double Raw, string Formatted)>();
            result["Other"] = new Dictionary<StatType, (double Raw, string Formatted)>();

            foreach (var statPair in Stats)
            {
                StatType type = statPair.Key;
                double rawValue = statPair.Value;
                string formattedValue = string.Empty;
                GetStatValue(type, out formattedValue);

                if (type == StatType.BaseHP || type == StatType.BaseAttack ||
                    type == StatType.BaseDefense || type == StatType.BaseSpeed)
                {
                    result["Base Stats"][type] = (rawValue, formattedValue);
                }
                else if (type == StatType.HPPercentage || type == StatType.AttackPercentage ||
                         type == StatType.DefensePercentage || type == StatType.SpeedPercentage)
                {
                    result["Percentage Stats"][type] = (rawValue, formattedValue);
                }
                else if (type == StatType.CriticalRate || type == StatType.CriticalDamage)
                {
                    result["Critical Stats"][type] = (rawValue, formattedValue);
                }
                else if (type == StatType.EnergyRecharge || type == StatType.ElementalMastery)
                {
                    result["Energy & Mastery"][type] = (rawValue, formattedValue);
                }
                else if (type == StatType.HealingBonus || type == StatType.IncomingHealingBonus)
                {
                    result["Healing"][type] = (rawValue, formattedValue);
                }
                else if (type.ToString().Contains("Resistance"))
                {
                    result["Resistances"][type] = (rawValue, formattedValue);
                }
                else if (type.ToString().Contains("DamageBonus"))
                {
                    result["Damage Bonuses"][type] = (rawValue, formattedValue);
                }
                else if (type == StatType.HP || type == StatType.Attack ||
                         type == StatType.Defense || type == StatType.Speed)
                {
                    result["Final Stats"][type] = (rawValue, formattedValue);
                }
                else
                {
                    result["Other"][type] = (rawValue, formattedValue);
                }
            }

            var emptyCategories = result.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
            foreach (var category in emptyCategories)
            {
                result.Remove(category);
            }

            return result;
        }
    }

}