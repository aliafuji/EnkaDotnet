using System.Collections.Generic;
using System.Linq;
using EnkaSharp.Components.Genshin;
using EnkaSharp.Enums.Genshin;

namespace EnkaSharp.Components.Genshin;

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
    public double GetStatValue(StatType statType)
    {
        return Stats.TryGetValue(statType, out double value) ? value : 0.0;
    }
}
