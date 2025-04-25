using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZAvatarAssetInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("ProfessionType")]
        public string ProfessionType { get; set; }

        [JsonProperty("ElementTypes")]
        public List<string> ElementTypes { get; set; }

        [JsonProperty("Image")]
        public string Image { get; set; }

        [JsonProperty("CircleIcon")]
        public string CircleIcon { get; set; }

        [JsonProperty("WeaponId")]
        public int WeaponId { get; set; }

        [JsonProperty("Colors")]
        public ZZZAvatarColors Colors { get; set; }

        [JsonProperty("HighlightProps")]
        public List<int> HighlightProps { get; set; }

        [JsonProperty("BaseProps")]
        public Dictionary<string, int> BaseProps { get; set; }

        [JsonProperty("GrowthProps")]
        public Dictionary<string, int> GrowthProps { get; set; }

        [JsonProperty("PromotionProps")]
        public List<Dictionary<string, int>> PromotionProps { get; set; }

        [JsonProperty("CoreEnhancementProps")]
        public List<Dictionary<string, int>> CoreEnhancementProps { get; set; }
    }

    public class ZZZAvatarColors
    {
        [JsonProperty("Accent")]
        public string Accent { get; set; }

        [JsonProperty("Mindscape")]
        public string Mindscape { get; set; }
    }
}