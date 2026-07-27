namespace EnkaDotNet.Components.EF
{
    public class EFTalent
    {
        public string Id { get; internal set; } = string.Empty;
        public int Index { get; internal set; }
        public int Rank { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public string IconUrl { get; internal set; } = string.Empty;
        public string SkillId { get; internal set; } = string.Empty;
    }
}
