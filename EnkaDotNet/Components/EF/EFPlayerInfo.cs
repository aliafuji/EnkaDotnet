using System.Collections.Generic;

namespace EnkaDotNet.Components.EF
{
    public class EFPlayerInfo
    {
        public string Nickname { get; internal set; } = string.Empty;
        public int AdminLevel { get; internal set; }
        public int EndfieldLevel { get; internal set; }
        public string Signature { get; internal set; } = string.Empty;
        public long Uid { get; internal set; }
        public string Region { get; internal set; } = string.Empty;
        public int Ttl { get; internal set; }
        public int ProfilePictureId { get; internal set; }
        public string ProfilePictureIcon { get; internal set; } = string.Empty;
        public int NameCardId { get; internal set; }
        public string NameCardIcon { get; internal set; } = string.Empty;
        public int CharCount { get; internal set; }
        public int WeaponCount { get; internal set; }
        public int DocCount { get; internal set; }
        public IReadOnlyList<EFMedal> Medals { get; internal set; } = new List<EFMedal>();
        public IReadOnlyList<EFOperator> ShowcaseOperators { get; internal set; } = new List<EFOperator>();
        public IReadOnlyList<EFCharListEntry> CharList { get; internal set; } = new List<EFCharListEntry>();
    }

    public class EFCharListEntry
    {
        public string TemplateId { get; internal set; } = string.Empty;
        public int? ResolvedTemplateId { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int PotentialLevel { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;
    }
}
