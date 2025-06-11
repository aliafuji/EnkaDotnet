using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnkaDotNet.Components.Genshin
{
    public class PlayerShowcaseCard
    {
        public string Nickname { get; }
        public int Level { get; }
        public string Signature { get; }
        public int WorldLevel { get; }
        public string ProfileType { get; }
        public Profile ProfilePicture { get; }
        public IReadOnlyList<NameCard> ShowcaseNameCards { get; }
        public IReadOnlyList<CharacterSummary> ShowcaseCharacters { get; }
        public NameCard BackgroundNameCard { get; }
        public AchievementSummary Achievements { get; }

        public PlayerShowcaseCard(PlayerInfo info)
        {
            Nickname = info.Nickname;
            Level = info.Level;
            Signature = info.Signature;
            WorldLevel = info.WorldLevel;
            ProfileType = info.DisplayType;
            ProfilePicture = info.GetProfilePicture() ?? new Profile();
            ShowcaseNameCards = info.ShowcaseNameCards;
            ShowcaseCharacters = new ReadOnlyCollection<CharacterSummary>(
                new List<CharacterSummary>());
            BackgroundNameCard = new NameCard(info.NameCardId, info.NameCardIcon);
            Achievements = new AchievementSummary
            {
                Total = info.FinishedAchievements,
                TowerProgress = new TowerProgress(info.TowerFloor, info.TowerChamber)
            };
        }

        public PlayerShowcaseCard(PlayerInfo info, IEnumerable<CharacterSummary> characters) : this(info)
        {
            ShowcaseCharacters = new ReadOnlyCollection<CharacterSummary>(characters.ToList());
        }

        public string GetDisplayName() => $"{Nickname} (AR {Level})";
    }
}