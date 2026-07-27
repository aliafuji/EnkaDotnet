namespace EnkaDotNet.Components.EF
{
    public class EFSkill
    {
        public string Id { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int MaxLevel { get; internal set; }
        public int EnhancedLevel { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
        public string Element { get; internal set; } = string.Empty;
        public bool IsCombatSkill { get; internal set; }
    }
}
