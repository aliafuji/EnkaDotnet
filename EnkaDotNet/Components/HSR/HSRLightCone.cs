using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Components.HSR
{
    public class HSRLightCone
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Promotion { get; internal set; }
        public int Rank { get; internal set; }  // Superimposition level

        public PathType Path { get; internal set; }
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;

        public List<HSRStatProperty> Properties { get; internal set; } = new List<HSRStatProperty>();
        public double BaseHP { get; internal set; }
        public double BaseAttack { get; internal set; }
        public double BaseDefense { get; internal set; }

        public override string ToString()
        {
            return $"{Name} (Lv.{Level}/{Promotion}, Rank {Rank})";
        }
    }
}