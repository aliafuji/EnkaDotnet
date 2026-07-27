using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFSkillAssetInfo
    {
        [JsonPropertyName("TagId")]
        public string TagId { get; set; }

        [JsonPropertyName("PropMap")]
        public Dictionary<string, EFSkillPropInfo> PropMap { get; set; }
    }

    public class EFSkillPropInfo
    {
        [JsonPropertyName("Values")]
        public List<double> Values { get; set; }

        [JsonPropertyName("Formula")]
        public string Formula { get; set; }
    }
}
