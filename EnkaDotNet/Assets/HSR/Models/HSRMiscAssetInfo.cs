using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRNameCardAssetInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }
    }

    public class HSRPfpAssetInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }
    }

    public class HSRPropertyAssetInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Format")]
        public string Format { get; set; }
    }

    public class HSRSkillAssetInfo
    {
        [JsonPropertyName("IconPath")]
        public string IconPath { get; set; }
    }

    public class HSREidolonAssetInfo
    {
        [JsonPropertyName("IconPath")]
        public string IconPath { get; set; }

        [JsonPropertyName("SkillAddLevelList")]
        public Dictionary<string, int> SkillAddLevelList { get; set; }
    }
}