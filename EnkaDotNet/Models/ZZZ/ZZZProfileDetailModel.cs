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
    public class ZZZProfileDetailModel
    {
        [JsonProperty("Nickname")]
        public string Nickname { get; set; }

        [JsonProperty("AvatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("Uid")]
        public long Uid { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("Title")]
        [Obsolete("Use TitleInfo field if available.")]
        public int Title { get; set; }

        [JsonProperty("TitleInfo")]
        public ZZZTitleInfoModel TitleInfo { get; set; }


        [JsonProperty("ProfileId")]
        public int ProfileId { get; set; }

        [JsonProperty("PlatformType")]
        public int PlatformType { get; set; }

        [JsonProperty("CallingCardId")]
        public int CallingCardId { get; set; }
    }
}