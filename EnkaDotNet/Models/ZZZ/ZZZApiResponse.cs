using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZApiResponse
    {
        [JsonProperty("PlayerInfo")]
        public ZZZPlayerInfoModel PlayerInfo { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }
    }

    public class ZZZPlayerInfoModel
    {
        [JsonProperty("ShowcaseDetail")]
        public ZZZShowcaseDetailModel ShowcaseDetail { get; set; }

        [JsonProperty("SocialDetail")]
        public ZZZSocialDetailModel SocialDetail { get; set; }

        [JsonProperty("Desc")]
        public string Desc { get; set; }
    }

    public class ZZZShowcaseDetailModel
    {
        [JsonProperty("AvatarList")]
        public List<ZZZAvatarModel> AvatarList { get; set; }
    }

    public class ZZZTitleInfoModel
    {
        [JsonProperty("Title")]
        public int Title { get; set; }
    }

    public class ZZZSocialDetailModel
    {
        [JsonProperty("MedalList")]
        public List<ZZZMedalModel> MedalList { get; set; }

        [JsonProperty("ProfileDetail")]
        public ZZZProfileDetailModel ProfileDetail { get; set; }

        [JsonProperty("Desc")]
        public string Desc { get; set; }

        [JsonProperty("TitleInfo")]
        public ZZZTitleInfoModel TitleInfo { get; set; }

        [JsonProperty("Title")]
        [Obsolete("Use SocialDetail.ProfileDetail.TitleInfo or SocialDetail.ProfileDetail.Title instead. This direct Title property might be removed.")]
        public int Title { get; set; }
    }

    public class ZZZMedalModel
    {
        [JsonProperty("Value")]
        public int Value { get; set; }

        [JsonProperty("MedalType")]
        public int MedalType { get; set; }

        [JsonProperty("MedalIcon")]
        public int MedalIcon { get; set; }
    }
}