using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZApiResponse
    {
        [JsonPropertyName("PlayerInfo")]
        public ZZZPlayerInfoModel PlayerInfo { get; set; }

        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }
    }

    public class ZZZPlayerInfoModel
    {
        [JsonPropertyName("ShowcaseDetail")]
        public ZZZShowcaseDetailModel ShowcaseDetail { get; set; }

        [JsonPropertyName("SocialDetail")]
        public ZZZSocialDetailModel SocialDetail { get; set; }

        [JsonPropertyName("Desc")]
        public string Desc { get; set; }
    }

    public class ZZZShowcaseDetailModel
    {
        [JsonPropertyName("AvatarList")]
        public List<ZZZAvatarModel> AvatarList { get; set; }
    }

    public class ZZZTitleInfoModel
    {
        [JsonPropertyName("Title")]
        public int Title { get; set; }
    }

    public class ZZZSocialDetailModel
    {
        [JsonPropertyName("MedalList")]
        public List<ZZZMedalModel> MedalList { get; set; }

        [JsonPropertyName("ProfileDetail")]
        public ZZZProfileDetailModel ProfileDetail { get; set; }

        [JsonPropertyName("Desc")]
        public string Desc { get; set; }

        [JsonPropertyName("TitleInfo")]
        public ZZZTitleInfoModel TitleInfo { get; set; }

        [JsonPropertyName("Title")]
        [Obsolete("Use SocialDetail.ProfileDetail.TitleInfo or SocialDetail.ProfileDetail.Title instead. This direct Title property might be removed.")]
        public int Title { get; set; }
    }

    public class ZZZMedalModel
    {
        [JsonPropertyName("Value")]
        public int Value { get; set; }

        [JsonPropertyName("MedalType")]
        public int MedalType { get; set; }

        [JsonPropertyName("MedalIcon")]
        public int MedalIcon { get; set; }
    }
}