using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Components.Genshin;

public class Weapon : EquipmentBase
{
    public WeaponType Type { get; internal set; }
    public int Ascension { get; internal set; }
    public int Refinement { get; internal set; }
    public double BaseAttack { get; internal set; }
    public StatProperty? SecondaryStat { get; internal set; }
}
