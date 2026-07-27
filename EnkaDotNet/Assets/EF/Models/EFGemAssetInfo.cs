using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFGemData
    {
        [JsonPropertyName("TermNums")]
        public Dictionary<string, EFGemTermInfo> TermNums { get; set; }

        [JsonPropertyName("TemplateItems")]
        public Dictionary<string, EFGemTemplateInfo> TemplateItems { get; set; }
    }

    public class EFGemTermInfo
    {
        [JsonPropertyName("TermType")]
        public int TermType { get; set; }

        [JsonPropertyName("TagId")]
        public string TagId { get; set; }

        [JsonPropertyName("TagIcon")]
        public string TagIcon { get; set; }

        [JsonPropertyName("TagNameHash")]
        public string TagNameHash { get; set; }
    }

    public class EFGemTemplateInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }
    }
}
