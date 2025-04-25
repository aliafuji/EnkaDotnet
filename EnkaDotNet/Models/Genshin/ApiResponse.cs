using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.Genshin
{
    public class ApiResponse
    {
        [JsonProperty("playerInfo")]
        public PlayerInfoModel PlayerInfo { get; set; }

        [JsonProperty("avatarInfoList")]
        public List<AvatarInfoModel> AvatarInfoList { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }
    }
}