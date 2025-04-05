using System.Collections.Generic;
using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Components.Genshin;

public class Artifact : EquipmentBase
{
    public string SetName { get; internal set; } = string.Empty;
    public ArtifactSlot Slot { get; internal set; }
    public StatProperty? MainStat { get; internal set; }
    public List<StatProperty> SubStats { get; internal set; } = new();
}
