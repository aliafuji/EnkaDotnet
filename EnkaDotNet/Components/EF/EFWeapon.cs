using System.Collections.Generic;

namespace EnkaDotNet.Components.EF
{
    public class EFWeapon
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int MaxLevel { get; internal set; }
        public int Rarity { get; internal set; }
        public int BreakthroughLevel { get; internal set; }
        public int RefineLevel { get; internal set; }
        public double BaseAtk { get; internal set; }
        public string WeaponType { get; internal set; } = string.Empty;
        public string IconUrl { get; internal set; } = string.Empty;
        public EFGem Gem { get; internal set; }
        public IReadOnlyList<EFStat> SubStats { get; internal set; } = new List<EFStat>();
    }
}
