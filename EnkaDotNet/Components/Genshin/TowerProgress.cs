namespace EnkaDotNet.Components.Genshin
{
    public class TowerProgress
    {
        public int Floor { get; set; }
        public int Chamber { get; set; }

        public TowerProgress() { }

        public TowerProgress(int floor, int chamber)
        {
            Floor = floor;
            Chamber = chamber;
        }

        public override string ToString() => $"Floor {Floor}, Chamber {Chamber}";
    }
}