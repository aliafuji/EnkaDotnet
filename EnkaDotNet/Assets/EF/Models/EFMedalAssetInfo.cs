using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFMedalAssetInfo
    {
        [JsonPropertyName("NameHash")]
        public string NameHash { get; set; }

        [JsonPropertyName("IconByLevel")]
        public Dictionary<string, string> IconByLevel { get; set; }
    }
}
