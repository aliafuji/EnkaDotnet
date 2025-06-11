using System.Collections.ObjectModel;

namespace EnkaDotNet.Components.Genshin
{
    public class PlayerProfile
    {
        public string Nickname { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Signature { get; set; } = string.Empty;
        public int WorldLevel { get; set; }

        public int FinishedAchievements { get; set; }
        public TowerProgress AbyssProgress { get; set; } = new();

        public NameCard BackgroundNameCard { get; set; } = new();
        public ProfilePicture ProfilePicture { get; set; } = new();
        public List<int> ShowcaseCharacterIds { get; set; } = new();
        public List<int> ShowcaseNameCardIds { get; set; } = new();
        public List<string> ShowcaseNameCardIcons { get; set; } = new();

        // Display Type
        public string DisplayType => HasShowcaseItems ? "Profile" : "Challenge";
        public bool HasShowcaseItems => (ShowcaseCharacterIds?.Count > 0 || ShowcaseNameCardIds?.Count > 0);

        // Derived properties
        public IReadOnlyList<NameCard> ShowcaseNameCards
        {
            get
            {
                if (ShowcaseNameCardIcons == null || ShowcaseNameCardIds == null)
                    return new ReadOnlyCollection<NameCard>(new List<NameCard>());

                try
                {
                    var list = ShowcaseNameCardIds
                        .Select((id, index) => new { id, index })
                        .Where(x => x.index < ShowcaseNameCardIcons.Count)
                        .Select(x => new NameCard
                        {
                            Id = x.id.ToString(),
                            Icon = ShowcaseNameCardIcons[x.index]
                        })
                        .ToList();

                    return new ReadOnlyCollection<NameCard>(list);
                }
                catch (Exception)
                {
                    return new ReadOnlyCollection<NameCard>(new List<NameCard>());
                }
            }
        }

        // Returns detailed profile card for display
        public ProfileDisplayCard ToDisplayCard() => new(this);
    }
}