using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFPfpAssetInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }
    }
}
