using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.ZZZ;
using Moq;
using Xunit;

namespace EnkaDotNet.Tests.ZZZ
{
    public class ZZZStatsHelpersTests
    {
        #region ElementType Enum Tests

        [Theory]
        [InlineData(ElementType.Unknown, 0)]
        [InlineData(ElementType.Physical, 1)]
        [InlineData(ElementType.Fire, 2)]
        [InlineData(ElementType.Ice, 3)]
        [InlineData(ElementType.FireFrost, 4)]
        [InlineData(ElementType.Electric, 5)]
        [InlineData(ElementType.Ether, 6)]
        [InlineData(ElementType.AuricEther, 7)]
        [InlineData(ElementType.HonedEdge, 8)]
        public void ElementType_HasCorrectValue(ElementType elementType, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)elementType);
        }

        [Fact]
        public void ElementType_AllValuesAreDefined()
        {
            var allValues = Enum.GetValues(typeof(ElementType));
            Assert.Equal(9, allValues.Length);
        }

        #endregion

        #region StatType Enum Value Tests

        [Theory]
        [InlineData(StatType.None, 0)]
        [InlineData(StatType.HPBase, 11101)]
        [InlineData(StatType.HPPercent, 11102)]
        [InlineData(StatType.HPFlat, 11103)]
        [InlineData(StatType.ATKBase, 12101)]
        [InlineData(StatType.ATKPercent, 12102)]
        [InlineData(StatType.ATKFlat, 12103)]
        [InlineData(StatType.SheerForceBase, 12301)]
        [InlineData(StatType.SheerForceFlat, 12302)]
        [InlineData(StatType.ImpactBase, 12201)]
        [InlineData(StatType.ImpactPercent, 12202)]
        [InlineData(StatType.DefBase, 13101)]
        [InlineData(StatType.DefPercent, 13102)]
        [InlineData(StatType.DefFlat, 13103)]
        [InlineData(StatType.CritRateBase, 20101)]
        [InlineData(StatType.CritRateFlat, 20103)]
        [InlineData(StatType.CritDMGBase, 21101)]
        [InlineData(StatType.CritDMGFlat, 21103)]
        [InlineData(StatType.PenRatioBase, 23101)]
        [InlineData(StatType.PenRatioFlat, 23103)]
        [InlineData(StatType.PENBase, 23201)]
        [InlineData(StatType.PENFlat, 23203)]
        [InlineData(StatType.EnergyRegenBase, 30501)]
        [InlineData(StatType.EnergyRegenPercent, 30502)]
        [InlineData(StatType.EnergyRegenFlat, 30503)]
        [InlineData(StatType.AnomalyProficiencyBase, 31201)]
        [InlineData(StatType.AnomalyProficiencyFlat, 31203)]
        [InlineData(StatType.AnomalyMasteryBase, 31401)]
        [InlineData(StatType.AnomalyMasteryPercent, 31402)]
        [InlineData(StatType.AnomalyMasteryFlat, 31403)]
        [InlineData(StatType.PhysicalDMGBonusBase, 31501)]
        [InlineData(StatType.PhysicalDMGBonusFlat, 31503)]
        [InlineData(StatType.FireDMGBonusBase, 31601)]
        [InlineData(StatType.FireDMGBonusFlat, 31603)]
        [InlineData(StatType.IceDMGBonusBase, 31701)]
        [InlineData(StatType.IceDMGBonusFlat, 31703)]
        [InlineData(StatType.ElectricDMGBonusBase, 31801)]
        [InlineData(StatType.ElectricDMGBonusFlat, 31803)]
        [InlineData(StatType.EtherDMGBonusBase, 31901)]
        [InlineData(StatType.EtherDMGBonusFlat, 31903)]
        [InlineData(StatType.AutomaticAdrenalineAccumulationBase, 32001)]
        [InlineData(StatType.AutomaticAdrenalineAccumulationPercent, 32002)]
        [InlineData(StatType.AutomaticAdrenalineAccumulationFlat, 32003)]
        [InlineData(StatType.SheerDMGBonusBase, 32201)]
        [InlineData(StatType.SheerDMGBonusFlat, 32203)]
        public void StatType_HasCorrectValue(StatType statType, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)statType);
        }

        #endregion

        #region EnumHelper Tests

        [Theory]
        [InlineData(11101)]
        [InlineData(11102)]
        [InlineData(11103)]
        [InlineData(12101)]
        [InlineData(12102)]
        [InlineData(12103)]
        [InlineData(12301)]
        [InlineData(12302)]
        [InlineData(32201)]
        [InlineData(32203)]
        public void EnumHelper_IsDefinedZZZStatType_RecognizesValidStatIds(int statId)
        {
            Assert.True(EnumHelper.IsDefinedZZZStatType(statId));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(99999)]
        [InlineData(12300)]
        public void EnumHelper_IsDefinedZZZStatType_RejectsInvalidStatIds(int statId)
        {
            Assert.False(EnumHelper.IsDefinedZZZStatType(statId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void EnumHelper_IsDefinedZZZSkillType_RecognizesValidSkillIds(int skillId)
        {
            Assert.True(EnumHelper.IsDefinedZZZSkillType(skillId));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(99)]
        public void EnumHelper_IsDefinedZZZSkillType_RejectsInvalidSkillIds(int skillId)
        {
            Assert.False(EnumHelper.IsDefinedZZZSkillType(skillId));
        }

        #endregion

        #region GetStatCategory Tests

        [Theory]
        [InlineData(StatType.HPBase, "HP")]
        [InlineData(StatType.HPPercent, "HP")]
        [InlineData(StatType.HPFlat, "HP")]
        [InlineData(StatType.ATKBase, "ATK")]
        [InlineData(StatType.ATKPercent, "ATK")]
        [InlineData(StatType.ATKFlat, "ATK")]
        [InlineData(StatType.DefBase, "DEF")]
        [InlineData(StatType.DefPercent, "DEF")]
        [InlineData(StatType.DefFlat, "DEF")]
        [InlineData(StatType.ImpactBase, "Impact")]
        [InlineData(StatType.ImpactPercent, "Impact")]
        [InlineData(StatType.CritRateBase, "CRIT Rate")]
        [InlineData(StatType.CritRateFlat, "CRIT Rate")]
        [InlineData(StatType.CritDMGBase, "CRIT DMG")]
        [InlineData(StatType.CritDMGFlat, "CRIT DMG")]
        [InlineData(StatType.EnergyRegenBase, "Energy Regen")]
        [InlineData(StatType.EnergyRegenPercent, "Energy Regen")]
        [InlineData(StatType.EnergyRegenFlat, "Energy Regen")]
        [InlineData(StatType.AnomalyProficiencyBase, "Anomaly Proficiency")]
        [InlineData(StatType.AnomalyProficiencyFlat, "Anomaly Proficiency")]
        [InlineData(StatType.AnomalyMasteryBase, "Anomaly Mastery")]
        [InlineData(StatType.AnomalyMasteryPercent, "Anomaly Mastery")]
        [InlineData(StatType.AnomalyMasteryFlat, "Anomaly Mastery")]
        [InlineData(StatType.PenRatioBase, "Pen Ratio")]
        [InlineData(StatType.PenRatioFlat, "Pen Ratio")]
        [InlineData(StatType.PENBase, "PEN")]
        [InlineData(StatType.PENFlat, "PEN")]
        [InlineData(StatType.PhysicalDMGBonusBase, "Physical DMG")]
        [InlineData(StatType.PhysicalDMGBonusFlat, "Physical DMG")]
        [InlineData(StatType.FireDMGBonusBase, "Fire DMG")]
        [InlineData(StatType.FireDMGBonusFlat, "Fire DMG")]
        [InlineData(StatType.IceDMGBonusBase, "Ice DMG")]
        [InlineData(StatType.IceDMGBonusFlat, "Ice DMG")]
        [InlineData(StatType.ElectricDMGBonusBase, "Electric DMG")]
        [InlineData(StatType.ElectricDMGBonusFlat, "Electric DMG")]
        [InlineData(StatType.EtherDMGBonusBase, "Ether DMG")]
        [InlineData(StatType.EtherDMGBonusFlat, "Ether DMG")]
        [InlineData(StatType.SheerForceBase, "Sheer Force")]
        [InlineData(StatType.SheerForceFlat, "Sheer Force")]
        [InlineData(StatType.SheerDMGBonusBase, "Sheer DMG")]
        [InlineData(StatType.SheerDMGBonusFlat, "Sheer DMG")]
        [InlineData(StatType.AutomaticAdrenalineAccumulationBase, "Automatic Adrenaline Accumulation")]
        [InlineData(StatType.AutomaticAdrenalineAccumulationPercent, "Automatic Adrenaline Accumulation")]
        [InlineData(StatType.AutomaticAdrenalineAccumulationFlat, "Automatic Adrenaline Accumulation")]
        public void GetStatCategory_ReturnsCorrectCategory(StatType statType, string expectedCategory)
        {
            var result = ZZZStatsHelpers.GetStatCategory(statType);
            Assert.Equal(expectedCategory, result);
        }

        [Fact]
        public void GetStatCategory_None_ReturnsEmptyString()
        {
            var result = ZZZStatsHelpers.GetStatCategory(StatType.None);
            Assert.Equal("", result);
        }

        #endregion

        #region GetStatCategoryDisplay Tests

        [Theory]
        [InlineData(StatType.HPBase, "HP")]
        [InlineData(StatType.HPFlat, "HP")]
        [InlineData(StatType.HPPercent, "HP%")]
        [InlineData(StatType.ATKBase, "ATK")]
        [InlineData(StatType.ATKFlat, "ATK")]
        [InlineData(StatType.ATKPercent, "ATK%")]
        [InlineData(StatType.DefBase, "DEF")]
        [InlineData(StatType.DefFlat, "DEF")]
        [InlineData(StatType.DefPercent, "DEF%")]
        [InlineData(StatType.ImpactBase, "Impact")]
        [InlineData(StatType.ImpactPercent, "Impact")]
        [InlineData(StatType.SheerForceBase, "Sheer Force")]
        [InlineData(StatType.SheerForceFlat, "Sheer Force")]
        [InlineData(StatType.SheerDMGBonusBase, "Sheer DMG")]
        [InlineData(StatType.SheerDMGBonusFlat, "Sheer DMG")]
        public void GetStatCategoryDisplay_ReturnsCorrectDisplay(StatType statType, string expectedDisplay)
        {
            var result = ZZZStatsHelpers.GetStatCategoryDisplay(statType);
            Assert.Equal(expectedDisplay, result);
        }

        #endregion

        #region IsCalculationPercentageStat Tests

        [Theory]
        [InlineData(StatType.HPPercent, true)]
        [InlineData(StatType.ATKPercent, true)]
        [InlineData(StatType.DefPercent, true)]
        [InlineData(StatType.ImpactPercent, true)]
        [InlineData(StatType.EnergyRegenPercent, true)]
        [InlineData(StatType.AnomalyMasteryPercent, true)]
        [InlineData(StatType.CritRateBase, true)]
        [InlineData(StatType.CritRateFlat, true)]
        [InlineData(StatType.CritDMGBase, true)]
        [InlineData(StatType.CritDMGFlat, true)]
        [InlineData(StatType.PenRatioBase, true)]
        [InlineData(StatType.PenRatioFlat, true)]
        [InlineData(StatType.PhysicalDMGBonusBase, true)]
        [InlineData(StatType.PhysicalDMGBonusFlat, true)]
        [InlineData(StatType.FireDMGBonusBase, true)]
        [InlineData(StatType.FireDMGBonusFlat, true)]
        [InlineData(StatType.IceDMGBonusBase, true)]
        [InlineData(StatType.IceDMGBonusFlat, true)]
        [InlineData(StatType.ElectricDMGBonusBase, true)]
        [InlineData(StatType.ElectricDMGBonusFlat, true)]
        [InlineData(StatType.EtherDMGBonusBase, true)]
        [InlineData(StatType.EtherDMGBonusFlat, true)]
        public void IsCalculationPercentageStat_PercentageStats_ReturnsTrue(StatType statType, bool expected)
        {
            var result = ZZZStatsHelpers.IsCalculationPercentageStat(statType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(StatType.HPBase)]
        [InlineData(StatType.HPFlat)]
        [InlineData(StatType.ATKBase)]
        [InlineData(StatType.ATKFlat)]
        [InlineData(StatType.DefBase)]
        [InlineData(StatType.DefFlat)]
        [InlineData(StatType.ImpactBase)]
        [InlineData(StatType.PENBase)]
        [InlineData(StatType.PENFlat)]
        [InlineData(StatType.AnomalyProficiencyBase)]
        [InlineData(StatType.AnomalyProficiencyFlat)]
        [InlineData(StatType.SheerForceBase)]
        [InlineData(StatType.SheerForceFlat)]
        public void IsCalculationPercentageStat_NonPercentageStats_ReturnsFalse(StatType statType)
        {
            var result = ZZZStatsHelpers.IsCalculationPercentageStat(statType);
            Assert.False(result);
        }

        #endregion

        #region GetStatTypeFromFriendlyName Tests

        [Theory]
        [InlineData("HP", false, StatType.HPFlat)]
        [InlineData("HP", true, StatType.HPPercent)]
        [InlineData("ATK", false, StatType.ATKFlat)]
        [InlineData("ATK", true, StatType.ATKPercent)]
        [InlineData("DEF", false, StatType.DefFlat)]
        [InlineData("DEF", true, StatType.DefPercent)]
        [InlineData("Impact", false, StatType.ImpactBase)]
        [InlineData("Impact", true, StatType.ImpactPercent)]
        [InlineData("CRIT Rate", false, StatType.CritRateFlat)]
        [InlineData("CRIT DMG", false, StatType.CritDMGFlat)]
        [InlineData("Anomaly Mastery", false, StatType.AnomalyMasteryFlat)]
        [InlineData("Anomaly Proficiency", false, StatType.AnomalyProficiencyFlat)]
        [InlineData("Pen Ratio", false, StatType.PenRatioFlat)]
        [InlineData("PEN", false, StatType.PENFlat)]
        [InlineData("Physical DMG", false, StatType.PhysicalDMGBonusFlat)]
        [InlineData("Fire DMG", false, StatType.FireDMGBonusFlat)]
        [InlineData("Ice DMG", false, StatType.IceDMGBonusFlat)]
        [InlineData("Electric DMG", false, StatType.ElectricDMGBonusFlat)]
        [InlineData("Ether DMG", false, StatType.EtherDMGBonusFlat)]
        public void GetStatTypeFromFriendlyName_ReturnsCorrectStatType(string friendlyName, bool isPercentage, StatType expected)
        {
            var result = ZZZStatsHelpers.GetStatTypeFromFriendlyName(friendlyName, isPercentage);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetStatTypeFromFriendlyName_EnergyRegen_ReturnsEnergyRegenPercent()
        {
            var result = ZZZStatsHelpers.GetStatTypeFromFriendlyName("Energy Regen", false, isEnergyRegen: true);
            Assert.Equal(StatType.EnergyRegenPercent, result);
        }

        [Fact]
        public void GetStatTypeFromFriendlyName_UnknownName_ReturnsNone()
        {
            var result = ZZZStatsHelpers.GetStatTypeFromFriendlyName("Unknown Stat", false);
            Assert.Equal(StatType.None, result);
        }

        #endregion

        #region IsDisplayPercentageStat Tests

        [Theory]
        [InlineData(StatType.CritRateBase, true)]
        [InlineData(StatType.CritRateFlat, true)]
        [InlineData(StatType.CritDMGBase, true)]
        [InlineData(StatType.CritDMGFlat, true)]
        [InlineData(StatType.PenRatioBase, true)]
        [InlineData(StatType.PenRatioFlat, true)]
        [InlineData(StatType.PhysicalDMGBonusBase, true)]
        [InlineData(StatType.FireDMGBonusBase, true)]
        [InlineData(StatType.IceDMGBonusBase, true)]
        [InlineData(StatType.ElectricDMGBonusBase, true)]
        [InlineData(StatType.EtherDMGBonusBase, true)]
        [InlineData(StatType.HPPercent, true)]
        [InlineData(StatType.ATKPercent, true)]
        [InlineData(StatType.DefPercent, true)]
        [InlineData(StatType.ImpactBase, true)]
        [InlineData(StatType.ImpactPercent, true)]
        [InlineData(StatType.EnergyRegenBase, true)]
        [InlineData(StatType.SheerDMGBonusBase, true)]
        public void IsDisplayPercentageStat_PercentageDisplayStats_ReturnsTrue(StatType statType, bool expected)
        {
            var result = ZZZStatsHelpers.IsDisplayPercentageStat(statType);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(StatType.HPBase)]
        [InlineData(StatType.HPFlat)]
        [InlineData(StatType.ATKBase)]
        [InlineData(StatType.ATKFlat)]
        [InlineData(StatType.DefBase)]
        [InlineData(StatType.DefFlat)]
        [InlineData(StatType.PENBase)]
        [InlineData(StatType.PENFlat)]
        [InlineData(StatType.AnomalyProficiencyBase)]
        [InlineData(StatType.SheerForceBase)]
        public void IsDisplayPercentageStat_NonPercentageDisplayStats_ReturnsFalse(StatType statType)
        {
            var result = ZZZStatsHelpers.IsDisplayPercentageStat(statType);
            Assert.False(result);
        }

        #endregion

        #region IsDisplayPercentageStatForGroup Tests

        [Theory]
        [InlineData("CRIT Rate", true)]
        [InlineData("CRIT DMG", true)]
        [InlineData("Pen Ratio", true)]
        [InlineData("Physical DMG", true)]
        [InlineData("Fire DMG", true)]
        [InlineData("Ice DMG", true)]
        [InlineData("Electric DMG", true)]
        [InlineData("Ether DMG", true)]
        [InlineData("Energy Regen", true)]
        [InlineData("Sheer DMG", true)]
        public void IsDisplayPercentageStatForGroup_PercentageGroups_ReturnsTrue(string statGroup, bool expected)
        {
            var result = ZZZStatsHelpers.IsDisplayPercentageStatForGroup(statGroup);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HP")]
        [InlineData("ATK")]
        [InlineData("DEF")]
        [InlineData("PEN")]
        [InlineData("Anomaly Proficiency")]
        [InlineData("Sheer Force")]
        public void IsDisplayPercentageStatForGroup_NonPercentageGroups_ReturnsFalse(string statGroup)
        {
            var result = ZZZStatsHelpers.IsDisplayPercentageStatForGroup(statGroup);
            Assert.False(result);
        }

        #endregion

        #region HP Calculation Tests

        [Fact]
        public void CalculateAllTotalStats_ZhaoHP_ReturnsCorrectValue()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var agent = CreateTestAgent(
                hpBase: 15000,
                hpPercent: 5000,
                hpFlat: 4566
            );

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("HP"), "HP stat should be present in results");
            double expectedHP = 27066;
            double actualHP = result["HP"].FinalValue;
            Assert.True(
                Math.Abs(actualHP - expectedHP) <= 1,
                $"HP should be approximately {expectedHP}, but was {actualHP}. Difference: {Math.Abs(actualHP - expectedHP)}"
            );
        }

        [Fact]
        public void CalculateAllTotalStats_HPWithDiscContributions_SumsCorrectly()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var agent = CreateTestAgentWithDiscs(
                hpBase: 10000,
                discHpFlat: 2000,
                discHpPercent: 2000
            );

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("HP"), "HP stat should be present in results");
            double expectedHP = 14000;
            double actualHP = result["HP"].FinalValue;
            Assert.True(
                Math.Abs(actualHP - expectedHP) <= 1,
                $"HP should be approximately {expectedHP}, but was {actualHP}"
            );
        }

        [Fact]
        public void CalculateAllTotalStats_HPCeilingFunction_AppliedCorrectly()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var agent = CreateTestAgent(
                hpBase: 10001,
                hpPercent: 3333,
                hpFlat: 0
            );

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            double expectedHP = 13335;
            double actualHP = result["HP"].FinalValue;
            Assert.True(
                Math.Abs(actualHP - expectedHP) <= 1,
                $"HP should be approximately {expectedHP}, but was {actualHP}. Ceiling function may not be applied correctly."
            );
        }

        #endregion

        #region ATK Calculation Tests

        [Fact]
        public void CalculateAllTotalStats_ATK_BasicCalculation()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var agent = CreateTestAgentWithATK(
                atkBase: 1000,
                atkPercent: 2000,
                atkFlat: 500,
                weaponAtkBase: 500
            );

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("ATK"));
            double expectedATK = (1000 + 500) * (1 + 2000 / 10000.0) + 500;
            Assert.True(Math.Abs(result["ATK"].FinalValue - expectedATK) <= 1);
        }

        [Fact]
        public void CalculateAllTotalStats_ATK_WithWeaponAndDiscs()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.ATKBase] = 800;
            stats[StatType.ATKPercent] = 1000;

            var weapon = new ZZZWEngine
            {
                MainStat = new ZZZStat { Type = StatType.ATKBase, Value = 400 },
                SecondaryStat = new ZZZStat { Type = StatType.ATKPercent, Value = 500 }
            };

            var discs = new List<ZZZDriveDisc>
            {
                new ZZZDriveDisc
                {
                    Id = 1,
                    SuitId = 1,
                    MainStat = new ZZZStat { Type = StatType.ATKPercent, Value = 300 },
                    SubStatsRaw = new List<ZZZStat>
                    {
                        new ZZZStat { Type = StatType.ATKFlat, Value = 50, Level = 2 }
                    }
                }
            };

            var agent = new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = discs,
                Weapon = weapon,
                ProfessionType = ProfessionType.Attack
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("ATK"));
            double combinedBase = 800 + 400;
            double totalRatio = 1000 + 500 + 300;
            double totalFlat = 50 * 2;
            double expectedATK = combinedBase * (1 + totalRatio / 10000.0) + totalFlat;
            Assert.True(Math.Abs(result["ATK"].FinalValue - expectedATK) <= 1);
        }

        #endregion

        #region DEF Calculation Tests

        [Fact]
        public void CalculateAllTotalStats_DEF_BasicCalculation()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.DefBase] = 800;
            stats[StatType.DefPercent] = 2000;
            stats[StatType.DefFlat] = 200;

            var agent = new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Defense
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("DEF"));
            double expectedDEF = 800 * (1 + 2000 / 10000.0) + 200;
            Assert.True(Math.Abs(result["DEF"].FinalValue - expectedDEF) <= 1);
        }

        #endregion

        #region Ben DEF-to-ATK Conversion Tests

        [Theory]
        [InlineData(0, 0.4)]
        [InlineData(1, 0.46)]
        [InlineData(2, 0.52)]
        [InlineData(3, 0.6)]
        [InlineData(4, 0.66)]
        [InlineData(5, 0.72)]
        [InlineData(6, 0.8)]
        public void CalculateAllTotalStats_Ben_DEFToATKConversion(int enhancementLevel, double expectedMultiplier)
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.ATKBase] = 500;
            stats[StatType.DefBase] = 1000;

            var agent = new ZZZAgent
            {
                Id = 1121,
                Name = "Ben",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Defense,
                CoreSkillEnhancement = enhancementLevel
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("ATK"));
            double baseDEF = 1000;
            double baseATK = 500;
            double bonusATK = Math.Floor(baseDEF * expectedMultiplier);
            double expectedATK = baseATK + bonusATK;
            Assert.True(Math.Abs(result["ATK"].FinalValue - expectedATK) <= 1,
                $"Expected ATK: {expectedATK}, Actual: {result["ATK"].FinalValue}");
        }

        [Fact]
        public void CalculateAllTotalStats_Ben_EnhancementLevelClamped()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.ATKBase] = 500;
            stats[StatType.DefBase] = 1000;

            var agent = new ZZZAgent
            {
                Id = 1121,
                Name = "Ben",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Defense,
                CoreSkillEnhancement = 10
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("ATK"));
            double bonusATK = Math.Floor(1000 * 0.8);
            double expectedATK = 500 + bonusATK;
            Assert.True(Math.Abs(result["ATK"].FinalValue - expectedATK) <= 1);
        }

        #endregion

        #region Rupture Agent Sheer Force Calculation Tests

        [Fact]
        public void CalculateAllTotalStats_RuptureAgent_SheerForceCalculation()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.HPBase] = 10000;
            stats[StatType.ATKBase] = 1000;

            var agent = new ZZZAgent
            {
                Id = 1371,
                Name = "Zhao",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Rupture
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("Sheer Force"));
            double expectedSheerForce = Math.Floor(Math.Floor(1000.0) * 0.3) + Math.Floor(Math.Floor(10000.0) * 0.1);
            Assert.True(Math.Abs(result["Sheer Force"].FinalValue - expectedSheerForce) <= 1,
                $"Expected Sheer Force: {expectedSheerForce}, Actual: {result["Sheer Force"].FinalValue}");
        }

        [Fact]
        public void CalculateAllTotalStats_NonRuptureAgent_NoSheerForceBonus()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.HPBase] = 10000;
            stats[StatType.ATKBase] = 1000;

            var agent = new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Attack
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("Sheer Force"));
            Assert.Equal(0, result["Sheer Force"].FinalValue);
        }

        [Fact]
        public void CalculateAllTotalStats_RuptureAgent_SheerForceWithFlatBonus()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.HPBase] = 10000;
            stats[StatType.ATKBase] = 1000;
            stats[StatType.SheerForceFlat] = 100;

            var agent = new ZZZAgent
            {
                Id = 1371,
                Name = "Zhao",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Rupture
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("Sheer Force"));
            double baseSheerForce = Math.Floor(Math.Floor(1000.0) * 0.3) + Math.Floor(Math.Floor(10000.0) * 0.1);
            double expectedSheerForce = baseSheerForce + 100;
            Assert.True(Math.Abs(result["Sheer Force"].FinalValue - expectedSheerForce) <= 1);
        }

        #endregion

        #region CRIT Stats Tests

        [Fact]
        public void CalculateAllTotalStats_CritRate_SumsCorrectly()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.CritRateBase] = 500;

            var discs = new List<ZZZDriveDisc>
            {
                new ZZZDriveDisc
                {
                    Id = 1,
                    SuitId = 1,
                    MainStat = new ZZZStat { Type = StatType.CritRateFlat, Value = 200 },
                    SubStatsRaw = new List<ZZZStat>()
                }
            };

            var agent = new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = discs,
                Weapon = null,
                ProfessionType = ProfessionType.Attack
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("CRIT Rate"));
            // Raw value is 500 + 200 = 700, but CalculateAllTotalStats now returns scaled value (divided by 100)
            double expectedCritRate = (500 + 200) / 100.0; // 7.0%
            Assert.True(Math.Abs(result["CRIT Rate"].FinalValue - expectedCritRate) <= 0.01);
        }

        [Fact]
        public void CalculateAllTotalStats_CritDMG_SumsCorrectly()
        {
            var mockAssets = new Mock<IZZZAssets>();
            mockAssets.Setup(a => a.GetDiscSetInfo(It.IsAny<string>()))
                .Returns((ZZZEquipmentSuitInfo?)null);

            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.CritDMGBase] = 5000;

            var weapon = new ZZZWEngine
            {
                MainStat = new ZZZStat { Type = StatType.ATKBase, Value = 100 },
                SecondaryStat = new ZZZStat { Type = StatType.CritDMGFlat, Value = 1000 }
            };

            var agent = new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = weapon,
                ProfessionType = ProfessionType.Attack
            };

            var result = ZZZStatsHelpers.CalculateAllTotalStats(agent, mockAssets.Object);

            Assert.True(result.ContainsKey("CRIT DMG"));
            // Raw value is 5000 + 1000 = 6000, but CalculateAllTotalStats now returns scaled value (divided by 100)
            double expectedCritDMG = (5000 + 1000) / 100.0; // 60.0%
            Assert.True(Math.Abs(result["CRIT DMG"].FinalValue - expectedCritDMG) <= 0.01);
        }

        #endregion

        #region Helper Methods

        private ZZZAgent CreateTestAgent(double hpBase, double hpPercent, double hpFlat)
        {
            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.HPBase] = hpBase;
            if (hpPercent > 0) stats[StatType.HPPercent] = hpPercent;
            if (hpFlat > 0) stats[StatType.HPFlat] = hpFlat;

            return new ZZZAgent
            {
                Id = 1371,
                Name = "Zhao",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = null,
                ProfessionType = ProfessionType.Rupture
            };
        }

        private ZZZAgent CreateTestAgentWithDiscs(double hpBase, double discHpFlat, double discHpPercent)
        {
            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.HPBase] = hpBase;

            var discs = new List<ZZZDriveDisc>();
            
            if (discHpPercent > 0)
            {
                discs.Add(new ZZZDriveDisc
                {
                    Id = 1,
                    SuitId = 1,
                    MainStat = new ZZZStat { Type = StatType.HPPercent, Value = discHpPercent },
                    SubStatsRaw = new List<ZZZStat>()
                });
            }

            if (discHpFlat > 0)
            {
                discs.Add(new ZZZDriveDisc
                {
                    Id = 2,
                    SuitId = 2,
                    MainStat = new ZZZStat { Type = StatType.HPFlat, Value = discHpFlat },
                    SubStatsRaw = new List<ZZZStat>()
                });
            }

            return new ZZZAgent
            {
                Id = 1371,
                Name = "Zhao",
                Level = 60,
                Stats = stats,
                EquippedDiscs = discs,
                Weapon = null,
                ProfessionType = ProfessionType.Rupture
            };
        }

        private ZZZAgent CreateTestAgentWithATK(double atkBase, double atkPercent, double atkFlat, double weaponAtkBase)
        {
            var stats = new ConcurrentDictionary<StatType, double>();
            stats[StatType.ATKBase] = atkBase;
            if (atkPercent > 0) stats[StatType.ATKPercent] = atkPercent;
            if (atkFlat > 0) stats[StatType.ATKFlat] = atkFlat;

            ZZZWEngine weapon = null;
            if (weaponAtkBase > 0)
            {
                weapon = new ZZZWEngine
                {
                    MainStat = new ZZZStat { Type = StatType.ATKBase, Value = weaponAtkBase },
                    SecondaryStat = new ZZZStat { Type = StatType.None, Value = 0 }
                };
            }

            return new ZZZAgent
            {
                Id = 1001,
                Name = "TestAgent",
                Level = 60,
                Stats = stats,
                EquippedDiscs = new List<ZZZDriveDisc>(),
                Weapon = weapon,
                ProfessionType = ProfessionType.Attack
            };
        }

        #endregion
    }
}
