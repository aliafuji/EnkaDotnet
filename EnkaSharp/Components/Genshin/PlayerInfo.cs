using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EnkaSharp.Components.Genshin;

public class PlayerInfo
{
    public string Nickname { get; internal set; } = string.Empty;
    public int Level { get; internal set; }
    public string Signature { get; internal set; } = string.Empty;
    public int WorldLevel { get; internal set; }
    public int NameCardId { get; internal set; }
    public string NameCardIcon { get; internal set; } = string.Empty;
    public int FinishedAchievements { get; internal set; }
    public int TowerFloor { get; internal set; }
    public int TowerChamber { get; internal set; }
    public List<int> ShowcaseCharacterIds { get; internal set; } = new();
    public List<int> ShowcaseNameCardIds { get; internal set; } = new();
    public List<string> ShowcaseNameCardIcons { get; internal set; } = new();
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