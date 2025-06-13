using System.Collections.Generic;

namespace EnkaDotNet.Components.Genshin
{
    public class PlayerInfo
    {
        public string Uid { get; internal set; } = string.Empty;
        public string TTL { get; internal set; }

        public string Nickname { get; internal set; } = string.Empty;

        public int Level { get; internal set; }

        public string Signature { get; internal set; } = string.Empty;

        public string IconUrl { get; internal set; } = string.Empty;

        public int WorldLevel { get; internal set; }

        public int NameCardId { get; internal set; }

        public string NameCardIcon { get; internal set; } = string.Empty;

        public int FinishedAchievements { get; internal set; }

        public ChallengeData Challenge { get; internal set; }

        public IReadOnlyList<int> ShowcaseCharacterIds { get; internal set; } = new List<int>();

        public IReadOnlyList<NameCard> ShowcaseNameCards { get; internal set; } = new List<NameCard>();

        public int ProfilePictureCharacterId { get; internal set; }
    }

    public class NameCard
    {
        private readonly int _id;
        private readonly string _iconUrl;

        public int Id => _id;
        public string IconUrl => _iconUrl;

        public NameCard(int id, string iconUrl)
        {
            _id = id;
            _iconUrl = iconUrl ?? string.Empty;
        }
    }

    public class ChallengeData
    {
        public SpiralAbyssData SpiralAbyss { get; set; }
        public TheatreData Theater { get; set; }
        public class SpiralAbyssData
        {
            public int Floor { get; internal set; }
            public int Chamber { get; internal set; }
            public int Star { get; internal set; }
        }

        public class TheatreData
        {
            public int Act { get; internal set; }
            public int Star { get; internal set; }
        }
    }
}
