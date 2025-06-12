using System.Collections.Generic;

namespace EnkaDotNet.Components.HSR
{
    public class HSRPlayerInfo
    {
        public string Uid { get; internal set; } = string.Empty;
        public string TTL { get; internal set; } = string.Empty;

        public string Nickname { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public string Signature { get; internal set; } = string.Empty;
        public bool IsDisplayAvatar { get; internal set; }
        public int WorldLevel { get; internal set; }
        public int FriendCount { get; internal set; }
        public string Platform { get; internal set; } = string.Empty;

        public int ProfilePictureId { get; internal set; }
        public string ProfilePictureIcon { get; internal set; } = string.Empty;
        public int NameCardId { get; internal set; }
        public string NameCardIcon { get; internal set; } = string.Empty;

        public HSRRecordInfo RecordInfo { get; internal set; } = new HSRRecordInfo();

        public List<HSRCharacter> DisplayedCharacters { get; internal set; } = new List<HSRCharacter>();
    }

    public class HSRRecordInfo
    {
        public int AchievementCount { get; internal set; }
        public int AvatarCount { get; internal set; }
        public int LightConeCount { get; internal set; }
        public int RelicCount { get; internal set; }
        public int ChestCount { get; internal set; }
        public int SpiralAbyssProgress { get; internal set; }
        public int MemoryOfChaosScore { get; internal set; }
    }
}
