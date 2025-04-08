using System.Collections.Generic;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZPlayerInfo
    {
        public string Uid { get; internal set; } = string.Empty;
        public string TTL { get; internal set; } = string.Empty;

        public string Nickname { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public string Signature { get; internal set; } = string.Empty;
        public int ProfilePictureId { get; internal set; }
        public string ProfilePictureIcon { get; internal set; } = string.Empty;
        public int TitleId { get; internal set; }
        public string TitleText { get; internal set; } = string.Empty;
        public int NameCardId { get; internal set; }
        public string NameCardIcon { get; internal set; } = string.Empty;
        public int MainCharacterId { get; internal set; }

        public List<ZZZMedal> Medals { get; internal set; } = new List<ZZZMedal>();

        public List<ZZZAgent> ShowcaseAgents { get; internal set; } = new List<ZZZAgent>();
    }

    public class ZZZMedal
    {
        public MedalType Type { get; internal set; }
        public int Value { get; internal set; }
        public string Icon { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
    }
}