﻿using System.Text.Json.Serialization;

namespace EnkaSharp.Models.Genshin
{
    public class PlayerInfoModel
    {
        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("worldLevel")]
        public int WorldLevel { get; set; }

        [JsonPropertyName("nameCardId")]
        public int NameCardId { get; set; }

        [JsonPropertyName("finishAchievementNum")]
        public int FinishAchievementNum { get; set; }

        [JsonPropertyName("towerFloorIndex")]
        public int TowerFloorIndex { get; set; }

        [JsonPropertyName("towerLevelIndex")]
        public int TowerLevelIndex { get; set; }

        [JsonPropertyName("showAvatarInfoList")]
        public List<ShowAvatarInfoModel>? ShowAvatarInfoList { get; set; }

        [JsonPropertyName("showNameCardIdList")]
        public List<int>? ShowNameCardIdList { get; set; }

        [JsonPropertyName("profilePicture")]
        public ProfilePictureModel? ProfilePicture { get; set; }
    }

    public class ShowAvatarInfoModel
    {
        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("costumeId")]
        public int? CostumeId { get; set; }
    }

    public class ProfilePictureModel
    {
        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("costumeId")]
        public int? CostumeId { get; set; }
    }
}
