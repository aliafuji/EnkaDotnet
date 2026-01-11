using EnkaDotNet.Utils.Common;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Enums.HSR;
using GenshinStatType = EnkaDotNet.Enums.Genshin.StatType;
using Xunit;

namespace EnkaDotNet.Tests.Common
{
    public class EnumHelperZZZTests
    {
        [Theory]
        [InlineData(11101)]
        [InlineData(11102)]
        [InlineData(11103)]
        [InlineData(12101)]
        [InlineData(12102)]
        [InlineData(12103)]
        [InlineData(20101)]
        [InlineData(21101)]
        [InlineData(12301)]
        [InlineData(12302)]
        [InlineData(32201)]
        [InlineData(32203)]
        public void IsDefinedZZZStatType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedZZZStatType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(99999)]
        [InlineData(11100)]
        [InlineData(12300)]
        public void IsDefinedZZZStatType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedZZZStatType(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void IsDefinedZZZSkillType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedZZZSkillType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(7)]
        [InlineData(100)]
        public void IsDefinedZZZSkillType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedZZZSkillType(value));
        }
    }

    public class EnumHelperHSRTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        [InlineData(40)]
        [InlineData(50)]
        public void IsDefinedHSRStatPropertyType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedHSRStatPropertyType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(999)]
        public void IsDefinedHSRStatPropertyType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedHSRStatPropertyType(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void IsDefinedHSRTraceType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedHSRTraceType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        [InlineData(100)]
        public void IsDefinedHSRTraceType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedHSRTraceType(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void IsDefinedHSRRelicType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedHSRRelicType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        [InlineData(100)]
        public void IsDefinedHSRRelicType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedHSRRelicType(value));
        }
    }

    public class EnumHelperGenshinTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(20)]
        [InlineData(22)]
        [InlineData(23)]
        [InlineData(28)]
        [InlineData(40)]
        [InlineData(46)]
        [InlineData(2000)]
        [InlineData(2001)]
        [InlineData(3000)]
        [InlineData(3046)]
        public void IsDefinedGenshinStatType_ValidValues_ReturnsTrue(int value)
        {
            Assert.True(EnumHelper.IsDefinedGenshinStatType(value));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(19)]
        [InlineData(33)]
        [InlineData(39)]
        [InlineData(48)]
        [InlineData(49)]
        [InlineData(57)]
        [InlineData(63)]
        [InlineData(66)]
        [InlineData(77)]
        [InlineData(82)]
        [InlineData(999)]
        [InlineData(1007)]
        [InlineData(2006)]
        [InlineData(3047)]
        [InlineData(99999)]
        public void IsDefinedGenshinStatType_InvalidValues_ReturnsFalse(int value)
        {
            Assert.False(EnumHelper.IsDefinedGenshinStatType(value));
        }

        [Fact]
        public void IsDefinedGenshinStatType_AllEnumValues_ReturnsTrue()
        {
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.None));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.BaseHP));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.CriticalRate));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.CriticalDamage));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.EnergyRecharge));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.ElementalMastery));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.PyroDamageBonus));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.HP));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.Attack));
            Assert.True(EnumHelper.IsDefinedGenshinStatType((int)GenshinStatType.Defense));
        }
    }

    public class EnumHelperConsistencyTests
    {
        [Fact]
        public void AllZZZStatTypeEnumValues_AreRecognized()
        {
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.None));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.HPBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.HPPercent));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.HPFlat));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.ATKBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.CritRateBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.CritDMGBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.SheerForceBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.SheerForceFlat));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.SheerDMGBonusBase));
            Assert.True(EnumHelper.IsDefinedZZZStatType((int)StatType.SheerDMGBonusFlat));
        }

        [Fact]
        public void AllHSRTraceTypeEnumValues_AreRecognized()
        {
            Assert.True(EnumHelper.IsDefinedHSRTraceType((int)TraceType.Unknown));
            Assert.True(EnumHelper.IsDefinedHSRTraceType((int)TraceType.Stat));
            Assert.True(EnumHelper.IsDefinedHSRTraceType((int)TraceType.Skill));
            Assert.True(EnumHelper.IsDefinedHSRTraceType((int)TraceType.Talent));
            Assert.True(EnumHelper.IsDefinedHSRTraceType((int)TraceType.Memosprite));
        }

        [Fact]
        public void AllHSRRelicTypeEnumValues_AreRecognized()
        {
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.Unknown));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.HEAD));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.HAND));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.BODY));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.FOOT));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.NECK));
            Assert.True(EnumHelper.IsDefinedHSRRelicType((int)RelicType.OBJECT));
        }
    }
}
