using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.HSR
{
    public class HSRApiResponse
    {
        [JsonProperty("detailInfo")]
        public HSRDetailInfo DetailInfo { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }
    }

    public class HSRDetailInfo
    {
        [JsonProperty("worldLevel")]
        public int WorldLevel { get; set; }

        [JsonProperty("headIcon")]
        public int HeadIcon { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("isDisplayAvatar")]
        public bool IsDisplayAvatar { get; set; }

        [JsonProperty("friendCount")]
        public int FriendCount { get; set; }

        [JsonProperty("avatarDetailList")]
        public List<HSRAvatarDetail> AvatarDetailList { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("recordInfo")]
        public HSRRecordInfoModel RecordInfo { get; set; }
    }

    public class HSRRecordInfoModel
    {
        [JsonProperty("achievementCount")]
        public int AchievementCount { get; set; }

        [JsonProperty("avatarCount")]
        public int AvatarCount { get; set; }

        [JsonProperty("equipmentCount")]
        public int EquipmentCount { get; set; }

        [JsonProperty("relicCount")]
        public int RelicCount { get; set; }

        [JsonProperty("challengeInfo")]
        public Dictionary<string, object> ChallengeInfo { get; set; }

        [JsonProperty("maxRogueChallengeScore")]
        public int MaxRogueChallengeScore { get; set; }
    }
}
