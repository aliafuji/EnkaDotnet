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
    public class PlayerInfoModel
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("worldLevel")]
        public int WorldLevel { get; set; }

        [JsonProperty("nameCardId")]
        public int NameCardId { get; set; }

        [JsonProperty("finishAchievementNum")]
        public int FinishAchievementNum { get; set; }

        [JsonProperty("towerFloorIndex")]
        public int TowerFloorIndex { get; set; }

        [JsonProperty("towerLevelIndex")]
        public int TowerLevelIndex { get; set; }

        [JsonProperty("towerStarIndex")]
        public int TowerStarIndex { get; set; }

        [JsonProperty("theaterActIndex")]
        public int TheaterActIndex { get; set; }

        [JsonProperty("theaterStarIndex")]
        public int TheaterStarIndex { get; set; }


        [JsonProperty("showAvatarInfoList")]
        public List<ShowAvatarInfoModel> ShowAvatarInfoList { get; set; }

        [JsonProperty("showNameCardIdList")]
        public List<int> ShowNameCardIdList { get; set; }

        [JsonProperty("profilePicture")]
        public ProfilePictureModel ProfilePicture { get; set; }
    }

    public class ShowAvatarInfoModel
    {
        [JsonProperty("avatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("costumeId")]
        public int CostumeId { get; set; }
    }

    public class ProfilePictureModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("avatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("costumeId")]
        public int CostumeId { get; set; }
    }
}