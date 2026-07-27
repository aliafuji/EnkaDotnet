namespace EnkaDotNet.Components.EF
{
    public class EFMedal
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public bool IsPlated { get; internal set; }
        public int DisplaySlot { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
    }
}
