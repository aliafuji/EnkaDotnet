using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFNameCardAssetInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }
    }
}
