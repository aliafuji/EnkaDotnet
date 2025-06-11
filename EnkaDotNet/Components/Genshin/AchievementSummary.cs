namespace EnkaDotNet.Components.Genshin
{
    public class AchievementSummary
    {
        public int Total { get; set; }
        public TowerProgress TowerProgress { get; set; } = new TowerProgress();
    }
}