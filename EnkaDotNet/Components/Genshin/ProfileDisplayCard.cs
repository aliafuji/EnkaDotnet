using System.Collections.Generic;

namespace EnkaDotNet.Components.Genshin
{
    public class ProfileDisplayCard
    {
        public string Nickname { get; }
        public int Level { get; }
        public string Signature { get; }
        public int WorldLevel { get; }
        public string ProfileType { get; }
        public ProfilePicture Avatar { get; }
        public NameCard BackgroundCard { get; }
        public IReadOnlyList<NameCard> Cards { get; }
        public IReadOnlyList<CharacterSummary> Characters { get; }
        public TowerProgress AbyssProgress { get; }
        public int Achievements { get; }

        public ProfileDisplayCard(PlayerProfile profile)
        {
            Nickname = profile.Nickname;
            Level = profile.Level;
            Signature = profile.Signature;
            WorldLevel = profile.WorldLevel;
            ProfileType = profile.DisplayType;
            Avatar = profile.ProfilePicture;
            BackgroundCard = profile.BackgroundNameCard;
            Cards = profile.ShowcaseNameCards;
            Characters = new List<CharacterSummary>(); // Needs to be populated externally
            AbyssProgress = profile.AbyssProgress;
            Achievements = profile.FinishedAchievements;
        }

        public ProfileDisplayCard(PlayerProfile profile, IEnumerable<CharacterSummary> characters)
            : this(profile)
        {
            Characters = characters.ToList();
        }

        public string GetDisplayName() => $"{Nickname} (AR {Level})";
    }
}