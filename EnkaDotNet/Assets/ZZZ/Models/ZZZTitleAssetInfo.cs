using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZTitleAssetInfo
    {
        [JsonPropertyName("TitleText")]
        public string TitleText { get; set; }

        [JsonPropertyName("ColorA")]
        public string ColorA { get; set; }

        [JsonPropertyName("ColorB")]
        public string ColorB { get; set; }
    }
}