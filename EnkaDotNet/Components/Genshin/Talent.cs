namespace EnkaDotNet.Components.Genshin
{
    public class Talent
    {
        internal EnkaClientOptions Options { get; set; }
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int BaseLevel { get; internal set; }
        public int ExtraLevel { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;

        public override string ToString()
        {
            bool raw = Options?.Raw ?? false;
            if (raw) return Id.ToString();

            string levelInfo = ExtraLevel > 0 ? $"{BaseLevel}+{ExtraLevel}={Level}" : $"{Level}";
            return $"{Name} Lv.{levelInfo}";
        }
    }
}