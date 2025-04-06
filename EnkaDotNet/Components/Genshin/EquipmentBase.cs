namespace EnkaDotNet.Components.Genshin
{
    public abstract class EquipmentBase
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
    }

}