using System.Collections.Generic;
using EnkaSharp.Components.Genshin;
using EnkaSharp.Enums.Genshin;

namespace EnkaSharp.Components.Genshin;

public class Artifact : EquipmentBase
{
    public string SetName { get; internal set; } = string.Empty;
    public ArtifactSlot Slot { get; internal set; }
    public StatProperty? MainStat { get; internal set; }
    public List<StatProperty> SubStats { get; internal set; } = new();
}
