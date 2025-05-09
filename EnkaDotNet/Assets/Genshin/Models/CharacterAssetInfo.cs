using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class CharacterAssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonPropertyName("Element")]
        public string Element { get; set; }

        [JsonPropertyName("WeaponType")]
        public string WeaponType { get; set; }

        [JsonPropertyName("SideIconName")]
        public string SideIconName { get; set; }

        [JsonPropertyName("NamecardIcon")]
        public string NamecardIcon { get; set; }

        [JsonPropertyName("Consts")]
        public List<string> Constellations { get; set; }
    }
}