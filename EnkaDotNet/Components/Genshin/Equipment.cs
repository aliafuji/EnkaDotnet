namespace EnkaDotNet.Components.Genshin
{
    public abstract class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Rarity { get; set; }
        public string IconUrl { get; set; } = string.Empty;
    }
}