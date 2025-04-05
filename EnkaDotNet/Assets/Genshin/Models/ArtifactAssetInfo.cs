using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models;

public class ArtifactAssetInfo
{
    [JsonPropertyName("NameTextMapHash")]
    public string? NameTextMapHash { get; set; }

    [JsonPropertyName("setIcon")]
    public int? SetIcon { get; set; }

    [JsonPropertyName("EquipType")]
    public string? EquipType { get; set; }

    [JsonPropertyName("Icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("RankLevel")]
    public int RankLevel { get; set; }
}