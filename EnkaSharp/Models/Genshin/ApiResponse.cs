using System.Text.Json.Serialization;

namespace EnkaSharp.Models.Genshin
{
    public class ApiResponse
    {
        [JsonPropertyName("playerInfo")]
        public PlayerInfoModel? PlayerInfo { get; set; }

        [JsonPropertyName("avatarInfoList")]
        public List<AvatarInfoModel>? AvatarInfoList { get; set; }

        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }
    }
}