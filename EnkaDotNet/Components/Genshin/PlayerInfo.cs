using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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

        public List<int> ShowcaseCharacterIds { get; internal set; } = new List<int>();

        public List<int> ShowcaseNameCardIds { get; internal set; } = new List<int>();

        public List<string> ShowcaseNameCardIcons { get; internal set; } = new List<string>();

        public int ProfilePictureCharacterId { get; internal set; }

        public IReadOnlyList<NameCard> ShowcaseNameCards
        {
            get
            {
                if (ShowcaseNameCardIcons == null || ShowcaseNameCardIds == null)
                {
                    Console.WriteLine($"Warning: {nameof(ShowcaseNameCardIcons)} or {nameof(ShowcaseNameCardIds)} is null.");
                    return new ReadOnlyCollection<NameCard>(new List<NameCard>());
                }

                try
                {
                    var list = ShowcaseNameCardIds
                        .Select((id, index) => new { id, index })
                        .Where(x => x.index < ShowcaseNameCardIcons.Count)
                        .Select(x => new NameCard(x.id, ShowcaseNameCardIcons[x.index]))
                        .ToList();

                    return new ReadOnlyCollection<NameCard>(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error creating {nameof(ShowcaseNameCards)}: {ex.Message}");
                    return new ReadOnlyCollection<NameCard>(new List<NameCard>());
                }
            }
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