using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZPropertyAssetInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Format")]
        public string Format { get; set; }
    }
}