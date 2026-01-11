using System.Text.Json;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Utils.HSR;
using EnkaDotNet.Enums.HSR;
using Xunit;

namespace EnkaDotNet.Tests.HSR
{
    public class HSRRecordInfoModelTests
    {
        [Fact]
        public void HSRRecordInfoModel_Deserialize_WithNewFields_ParsesCorrectly()
        {
            var json = @"{
                ""achievementCount"": 500,
                ""avatarCount"": 30,
                ""equipmentCount"": 50,
                ""relicCount"": 200,
                ""maxRogueChallengeScore"": 10,
                ""bookCount"": 100,
                ""musicCount"": 50
            }";

            var model = JsonSerializer.Deserialize<HSRRecordInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(500, model.AchievementCount);
            Assert.Equal(30, model.AvatarCount);
            Assert.Equal(50, model.EquipmentCount);
            Assert.Equal(200, model.RelicCount);
            Assert.Equal(10, model.MaxRogueChallengeScore);
            Assert.Equal(100, model.BookCount);
            Assert.Equal(50, model.MusicCount);
        }

        [Fact]
        public void HSRRecordInfoModel_Deserialize_WithoutNewFields_DefaultsToNull()
        {
            var json = @"{
                ""achievementCount"": 500,
                ""avatarCount"": 30,
                ""equipmentCount"": 50,
                ""relicCount"": 200
            }";

            var model = JsonSerializer.Deserialize<HSRRecordInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(500, model.AchievementCount);
            Assert.Null(model.MaxRogueChallengeScore);
            Assert.Null(model.BookCount);
            Assert.Null(model.MusicCount);
        }

        [Fact]
        public void HSRRecordInfoModel_Deserialize_BookCount_ParsesCorrectly()
        {
            var json = @"{ ""bookCount"": 75 }";

            var model = JsonSerializer.Deserialize<HSRRecordInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(75, model.BookCount);
        }

        [Fact]
        public void HSRRecordInfoModel_Deserialize_MusicCount_ParsesCorrectly()
        {
            var json = @"{ ""musicCount"": 42 }";

            var model = JsonSerializer.Deserialize<HSRRecordInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(42, model.MusicCount);
        }

        [Fact]
        public void HSRRecordInfoModel_Deserialize_MaxRogueChallengeScore_ParsesCorrectly()
        {
            var json = @"{ ""maxRogueChallengeScore"": 8 }";

            var model = JsonSerializer.Deserialize<HSRRecordInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(8, model.MaxRogueChallengeScore);
        }
    }

    public class HSRAvatarDetailTests
    {
        [Fact]
        public void HSRAvatarDetail_Deserialize_WithEnhanceId_ParsesCorrectly()
        {
            var json = @"{
                ""avatarId"": 1001,
                ""level"": 80,
                ""promotion"": 6,
                ""rank"": 2,
                ""enhanceId"": true
            }";

            var model = JsonSerializer.Deserialize<HSRAvatarDetail>(json);

            Assert.NotNull(model);
            Assert.Equal(1001, model.AvatarId);
            Assert.Equal(80, model.Level);
            Assert.True(model.Enhanced);
        }

        [Fact]
        public void HSRAvatarDetail_Deserialize_WithoutEnhanceId_DefaultsToFalse()
        {
            var json = @"{
                ""avatarId"": 1001,
                ""level"": 80,
                ""promotion"": 6,
                ""rank"": 2
            }";

            var model = JsonSerializer.Deserialize<HSRAvatarDetail>(json);

            Assert.NotNull(model);
            Assert.False(model.Enhanced);
        }

        [Fact]
        public void HSRAvatarDetail_Deserialize_EnhanceIdFalse_ParsesCorrectly()
        {
            var json = @"{
                ""avatarId"": 1002,
                ""level"": 60,
                ""enhanceId"": false
            }";

            var model = JsonSerializer.Deserialize<HSRAvatarDetail>(json);

            Assert.NotNull(model);
            Assert.False(model.Enhanced);
        }
    }

    public class HSRStatPropertyUtilsTests
    {
        [Theory]
        [InlineData("MaxHP", StatPropertyType.MaxHP)]
        [InlineData("Attack", StatPropertyType.Attack)]
        [InlineData("Defence", StatPropertyType.Defence)]
        [InlineData("Speed", StatPropertyType.Speed)]
        [InlineData("BaseHP", StatPropertyType.BaseHP)]
        [InlineData("BaseAttack", StatPropertyType.BaseAttack)]
        [InlineData("BaseDefence", StatPropertyType.BaseDefence)]
        [InlineData("BaseSpeed", StatPropertyType.BaseSpeed)]
        public void MapToStatPropertyType_BasicStats_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HPDelta", StatPropertyType.HPDelta)]
        [InlineData("AttackDelta", StatPropertyType.AttackDelta)]
        [InlineData("DefenceDelta", StatPropertyType.DefenceDelta)]
        [InlineData("SpeedDelta", StatPropertyType.SpeedDelta)]
        public void MapToStatPropertyType_DeltaStats_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HPAddedRatio", StatPropertyType.HPAddedRatio)]
        [InlineData("AttackAddedRatio", StatPropertyType.AttackAddedRatio)]
        [InlineData("DefenceAddedRatio", StatPropertyType.DefenceAddedRatio)]
        [InlineData("SpeedAddedRatio", StatPropertyType.SpeedAddedRatio)]
        public void MapToStatPropertyType_RatioStats_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("CriticalChance", StatPropertyType.CriticalChance)]
        [InlineData("CriticalDamage", StatPropertyType.CriticalDamage)]
        [InlineData("CriticalChanceBase", StatPropertyType.CriticalChanceBase)]
        [InlineData("CriticalDamageBase", StatPropertyType.CriticalDamageBase)]
        public void MapToStatPropertyType_CriticalStats_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("PhysicalAddedRatio", StatPropertyType.PhysicalAddedRatio)]
        [InlineData("FireAddedRatio", StatPropertyType.FireAddedRatio)]
        [InlineData("IceAddedRatio", StatPropertyType.IceAddedRatio)]
        [InlineData("ThunderAddedRatio", StatPropertyType.LightningAddedRatio)]
        [InlineData("WindAddedRatio", StatPropertyType.WindAddedRatio)]
        [InlineData("QuantumAddedRatio", StatPropertyType.QuantumAddedRatio)]
        [InlineData("ImaginaryAddedRatio", StatPropertyType.ImaginaryAddedRatio)]
        public void MapToStatPropertyType_ElementalDMGBoost_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("PhysicalResistance", StatPropertyType.PhysicalResistance)]
        [InlineData("FireResistance", StatPropertyType.FireResistance)]
        [InlineData("IceResistance", StatPropertyType.IceResistance)]
        [InlineData("ThunderResistance", StatPropertyType.LightningResistance)]
        [InlineData("WindResistance", StatPropertyType.WindResistance)]
        [InlineData("QuantumResistance", StatPropertyType.QuantumResistance)]
        [InlineData("ImaginaryResistance", StatPropertyType.ImaginaryResistance)]
        public void MapToStatPropertyType_ElementalResistance_ReturnsCorrectType(string propertyType, StatPropertyType expected)
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HPAddedRatio", true)]
        [InlineData("AttackAddedRatio", true)]
        [InlineData("CriticalChance", true)]
        [InlineData("CriticalDamage", true)]
        [InlineData("BreakDamageAddedRatio", true)]
        [InlineData("PhysicalAddedRatio", true)]
        [InlineData("FireResistance", true)]
        public void IsPercentageType_PercentageStats_ReturnsTrue(string propertyType, bool expected)
        {
            var result = HSRStatPropertyUtils.IsPercentageType(propertyType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HPDelta", false)]
        [InlineData("AttackDelta", false)]
        [InlineData("DefenceDelta", false)]
        [InlineData("SpeedDelta", false)]
        public void IsPercentageType_FlatStats_ReturnsFalse(string propertyType, bool expected)
        {
            var result = HSRStatPropertyUtils.IsPercentageType(propertyType);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MapToStatPropertyType_UnknownType_ReturnsNone()
        {
            var result = HSRStatPropertyUtils.MapToStatPropertyType("UnknownStat");
            Assert.Equal(StatPropertyType.None, result);
        }

        [Theory]
        [InlineData("HPDelta", "HP")]
        [InlineData("HPAddedRatio", "HP%")]
        [InlineData("AttackDelta", "ATK")]
        [InlineData("AttackAddedRatio", "ATK%")]
        [InlineData("CriticalChance", "CRIT Rate")]
        [InlineData("CriticalDamage", "CRIT DMG")]
        [InlineData("PhysicalAddedRatio", "Physical DMG")]
        [InlineData("FireAddedRatio", "Fire DMG")]
        public void GetDisplayName_KnownTypes_ReturnsCorrectName(string propertyType, string expected)
        {
            var result = HSRStatPropertyUtils.GetDisplayName(propertyType);
            Assert.Equal(expected, result);
        }
    }

    public class HSREnumExtensionsTests
    {
        [Theory]
        [InlineData(PathType.Warrior, "Destruction")]
        [InlineData(PathType.Rogue, "Hunt")]
        [InlineData(PathType.Mage, "Erudition")]
        [InlineData(PathType.Shaman, "Harmony")]
        [InlineData(PathType.Warlock, "Nihility")]
        [InlineData(PathType.Knight, "Preservation")]
        [InlineData(PathType.Priest, "Abundance")]
        [InlineData(PathType.Memory, "Remembrance")]
        public void GetPathName_AllPaths_ReturnsCorrectName(PathType pathType, string expected)
        {
            var result = pathType.GetPathName();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(ElementType.Physical, "Physical")]
        [InlineData(ElementType.Fire, "Fire")]
        [InlineData(ElementType.Ice, "Ice")]
        [InlineData(ElementType.Lightning, "Lightning")]
        [InlineData(ElementType.Wind, "Wind")]
        [InlineData(ElementType.Quantum, "Quantum")]
        [InlineData(ElementType.Imaginary, "Imaginary")]
        public void GetElementName_AllElements_ReturnsCorrectName(ElementType elementType, string expected)
        {
            var result = elementType.GetElementName();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(RelicType.HEAD, "Head")]
        [InlineData(RelicType.HAND, "Hands")]
        [InlineData(RelicType.BODY, "Body")]
        [InlineData(RelicType.FOOT, "Feet")]
        [InlineData(RelicType.NECK, "Planar Sphere")]
        [InlineData(RelicType.OBJECT, "Link Rope")]
        public void GetRelicName_AllRelicTypes_ReturnsCorrectName(RelicType relicType, string expected)
        {
            var result = relicType.GetRelicName();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(StatPropertyType.MaxHP, "HP")]
        [InlineData(StatPropertyType.Attack, "ATK")]
        [InlineData(StatPropertyType.Defence, "DEF")]
        [InlineData(StatPropertyType.Speed, "SPD")]
        [InlineData(StatPropertyType.CriticalChance, "CRIT Rate")]
        [InlineData(StatPropertyType.CriticalDamage, "CRIT DMG")]
        [InlineData(StatPropertyType.HPAddedRatio, "HP%")]
        [InlineData(StatPropertyType.AttackAddedRatio, "ATK%")]
        public void GetStatPropertyName_CommonStats_ReturnsCorrectName(StatPropertyType statType, string expected)
        {
            var result = statType.GetStatPropertyName();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(StatPropertyType.HPAddedRatio, true)]
        [InlineData(StatPropertyType.CriticalChance, true)]
        [InlineData(StatPropertyType.CriticalDamage, true)]
        [InlineData(StatPropertyType.HPDelta, false)]
        [InlineData(StatPropertyType.AttackDelta, false)]
        [InlineData(StatPropertyType.SpeedDelta, false)]
        public void IsPercentageStat_VariousStats_ReturnsCorrectValue(StatPropertyType statType, bool expected)
        {
            var result = statType.IsPercentageStat();
            Assert.Equal(expected, result);
        }
    }
}
