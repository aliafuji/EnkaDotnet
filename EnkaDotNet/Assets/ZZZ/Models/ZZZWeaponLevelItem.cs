﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZWeaponLevelData
    {
        [JsonPropertyName("Items")]
        public List<ZZZWeaponLevelItem> Items { get; set; }
    }

    public class ZZZWeaponLevelItem
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("EnhanceRate")]
        public double EnhanceRate { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }

        [JsonPropertyName("ExpRecycleRate")]
        public int ExpRecycleRate { get; set; }
    }
}