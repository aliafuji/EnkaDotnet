using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class ArtifactSetAssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonPropertyName("SetNeedNum")]
        public List<int> SetNeedNum { get; set; }
    }
}