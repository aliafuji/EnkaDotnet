using System;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZProfileDetailModel
    {
        [JsonPropertyName("Nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("AvatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("Uid")]
        public long Uid { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("Title")]
        [Obsolete("Use TitleInfo field if available.")]
        public int Title { get; set; }

        [JsonPropertyName("TitleInfo")]
        public ZZZTitleInfoModel TitleInfo { get; set; }


        [JsonPropertyName("ProfileId")]
        public int ProfileId { get; set; }

        [JsonPropertyName("PlatformType")]
        public int PlatformType { get; set; }

        [JsonPropertyName("CallingCardId")]
        public int CallingCardId { get; set; }
    }
}