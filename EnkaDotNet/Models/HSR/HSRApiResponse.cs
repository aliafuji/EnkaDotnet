using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.HSR
{
    public class HSRApiResponse
    {
        [JsonPropertyName("detailInfo")]
        public HSRDetailInfo? DetailInfo { get; set; }

        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }
    }

    public class HSRDetailInfo
    {
        [JsonPropertyName("worldLevel")]
        public int WorldLevel { get; set; }

        [JsonPropertyName("headIcon")]
        public int HeadIcon { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("platform")]
        public string? Platform { get; set; }

        [JsonPropertyName("isDisplayAvatar")]
        public bool IsDisplayAvatar { get; set; }

        [JsonPropertyName("friendCount")]
        public int FriendCount { get; set; }

        [JsonPropertyName("avatarDetailList")]
        public List<HSRAvatarDetail>? AvatarDetailList { get; set; }

        [JsonPropertyName("uid")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public long Uid { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("recordInfo")]
        public HSRRecordInfoModel? RecordInfo { get; set; }
    }

    public class HSRRecordInfoModel
    {
        [JsonPropertyName("achievementCount")]
        public int AchievementCount { get; set; }

        [JsonPropertyName("avatarCount")]
        public int AvatarCount { get; set; }

        [JsonPropertyName("equipmentCount")]
        public int EquipmentCount { get; set; }

        [JsonPropertyName("relicCount")]
        public int RelicCount { get; set; }

        [JsonPropertyName("challengeInfo")]
        public Dictionary<string, object>? ChallengeInfo { get; set; }

        [JsonPropertyName("maxRogueChallengeScore")]
        public int MaxRogueChallengeScore { get; set; }
    }
}