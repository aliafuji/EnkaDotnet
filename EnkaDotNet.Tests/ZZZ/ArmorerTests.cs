using System.Net;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Utils.ZZZ;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EnkaDotNet.Tests.ZZZ;

public class ArmorerTests
{
    [Fact]
    public async Task MapAgent_LoadsArmorerAssetsAndNewStatProperties()
    {
        using var client = new HttpClient(new ArmorerAssetHandler());
        using var assets = new ZZZAssets("en", client, NullLogger<ZZZAssets>.Instance);
        await assets.EnsureInitializedAsync();
        var mapper = new ZZZDataMapper(assets, new EnkaClientOptions());
        var agent = mapper.MapAgent(new ZZZAvatarModel
        {
            Id = 1001, Level = 2, PromotionLevel = 1, CoreSkillEnhancement = 1
        });

        Assert.Equal(ProfessionType.Armorer, agent.ProfessionType);
        Assert.Equal(ProfessionType.Armorer, assets.GetWeaponType(2001));
        Assert.Equal("Shiyu Defense", assets.GetMedalName(1001));
        Assert.Equal("https://enka.network/ui/zzz/Shiyu01.png", assets.GetMedalIconUrl(1001));
        // Base + one level of growth + promotion + core enhancement.
        Assert.Equal(5500, agent.Stats[StatType.LacerationDMGBase]);
        Assert.Equal(500, agent.Stats[StatType.LacerationDMGFlat]);
        Assert.Equal(210, agent.Stats[StatType.AutomaticSharpnessAccumulationBase]);
        Assert.Equal(2500, agent.Stats[StatType.AutomaticSharpnessAccumulationPercent]);
        Assert.Equal(20, agent.Stats[StatType.AutomaticSharpnessAccumulationFlat]);
        Assert.Equal(60, agent.GetFinalStats()["Laceration DMG"]);
        Assert.Equal(2.825, agent.GetFinalStats()["Automatic Sharpness Accumulation"], 6);
        Assert.Equal("60.0%", agent.GetAllStats()["Laceration DMG"].Final);
        Assert.Equal("2.83", agent.GetAllStats()["Automatic Sharpness Accumulation"].Final);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CalculateStats_IncludesAgentWeaponDiscsAndSetBonuses(bool raw)
    {
        var assets = new Mock<IZZZAssets>();
        assets.Setup(a => a.GetLocalizedText("SharpCriticalDamage")).Returns("Localized Laceration");
        assets.Setup(a => a.GetLocalizedText("EpRecover")).Returns("Localized ASA");
        assets.Setup(a => a.GetDiscSetInfo("1")).Returns(new ZZZEquipmentSuitInfo
        {
            SetBonusProps = new Dictionary<string, int>
            {
                ["21303"] = 600, ["32402"] = 1000, ["32403"] = 20
            }
        });
        var agent = new ZZZAgent
        {
            Assets = assets.Object,
            Options = new EnkaClientOptions { UseRawStatValues = raw },
            ProfessionType = ProfessionType.Armorer,
            Stats = new()
            {
                [StatType.LacerationDMGBase] = 5000,
                [StatType.LacerationDMGFlat] = 300,
                [StatType.AutomaticSharpnessAccumulationBase] = 200,
                [StatType.AutomaticSharpnessAccumulationPercent] = 1000,
                [StatType.AutomaticSharpnessAccumulationFlat] = 10
            },
            Weapon = new ZZZWEngine
            {
                SecondaryStat = new ZZZStat { Type = StatType.LacerationDMGFlat, Value = 700 }
            },
            EquippedDiscs = new List<ZZZDriveDisc>
            {
                new()
                {
                    SuitId = 1,
                    MainStat = new ZZZStat { Type = StatType.LacerationDMGFlat, Value = 1000 },
                    SubStatsRaw = new List<ZZZStat>
                    {
                        new() { Type = StatType.LacerationDMGFlat, Value = 200, Level = 2 },
                        new() { Type = StatType.AutomaticSharpnessAccumulationFlat, Value = 5, Level = 2 }
                    }
                },
                new()
                {
                    SuitId = 1,
                    MainStat = new ZZZStat { Type = StatType.AutomaticSharpnessAccumulationPercent, Value = 500 }
                }
            }
        };

        var totals = ZZZStatsHelpers.CalculateAllTotalStats(agent, assets.Object);
        Assert.Equal(80, totals["Laceration DMG"].FinalValue);
        Assert.Equal(50, totals["Laceration DMG"].BaseValue);
        Assert.Equal(30, totals["Laceration DMG"].AddedValue);
        // (200 * (1 + 2500/10000) + 40) / 100 = 2.9.
        Assert.Equal(2.9, totals["Automatic Sharpness Accumulation"].FinalValue, 6);
        Assert.Equal(2, totals["Automatic Sharpness Accumulation"].BaseValue);
        Assert.Equal(0.9, totals["Automatic Sharpness Accumulation"].AddedValue, 6);
        var formatted = agent.GetAllStats();
        var laceration = formatted[raw ? "LacerationDMGFlat" : "Localized Laceration"];
        Assert.Equal(raw ? "80.0" : "80.0%", laceration.Final);
        var asa = formatted[raw ? "AutomaticSharpnessAccumulationBase" : "Localized ASA"];
        Assert.Equal("2.9", asa.Final);
        Assert.Equal("2", asa.Base);
        Assert.Equal("0.9", asa.Added);
    }

    [Theory]
    [InlineData(21301, true)]
    [InlineData(21303, true)]
    [InlineData(32401, false)]
    [InlineData(32402, true)]
    [InlineData(32403, false)]
    public void CalculateWeaponStats_RecognizesNewProperties(int propertyId, bool percentage)
    {
        var assets = new Mock<IZZZAssets>();
        assets.Setup(a => a.GetWeaponInfo("1")).Returns(new ZZZWeaponAssetInfo
        {
            Rarity = 4,
            MainStat = new ZZZStatProperty { PropertyId = 12101, PropertyValue = 100 },
            SecondaryStat = new ZZZStatProperty { PropertyId = propertyId, PropertyValue = 250 }
        });
        assets.Setup(a => a.GetWeaponLevelData()).Returns(new List<ZZZWeaponLevelItem>
        {
            new() { Rarity = 4, Level = 1 }
        });
        assets.Setup(a => a.GetWeaponStarData()).Returns(new List<ZZZWeaponStarItem>
        {
            new() { Rarity = 4, BreakLevel = 0 }
        });
        var calculator = new ZZZStatsCalculator(assets.Object);
        var (_, secondary) = calculator.CalculateWeaponStats(1, 1, 0);
        Assert.Equal((StatType)propertyId, secondary.Type);
        Assert.Equal(250, secondary.Value);
        Assert.Equal(percentage, secondary.IsPercentage);
    }

    [Theory]
    [InlineData(32401, 10, 2.3, "0.1")]
    [InlineData(32402, 500, 2.3, "5.0%")]
    [InlineData(32403, 10, 2.3, "0.1")]
    public void SharpnessRecovery_WeaponContributionsAndEquipmentFormatting(int propertyId, double value, double expected, string display)
    {
        var assets = new Mock<IZZZAssets>();
        var stat = new ZZZStatsCalculator(assets.Object).CreateStatWithProperScaling(propertyId, value);
        var weapon = new ZZZWEngine { SecondaryStat = stat };
        var disc = new ZZZDriveDisc
        {
            MainStat = stat,
            SubStatsRaw = new List<ZZZStat>
            {
                new() { Type = stat.Type, Value = stat.Value, Level = 2, IsPercentage = stat.IsPercentage }
            }
        };
        var agent = new ZZZAgent
        {
            Assets = assets.Object,
            Weapon = weapon,
            Stats = new()
            {
                [StatType.AutomaticSharpnessAccumulationBase] = 200,
                [StatType.AutomaticSharpnessAccumulationPercent] = 1000
            }
        };

        Assert.Equal(expected, agent.GetFinalStats()["Automatic Sharpness Accumulation"], 6);
        Assert.Equal(display, weapon.FormattedSecondaryStat.Value);
        Assert.Equal(display, disc.FormattedMainStat.Value);
        Assert.Equal(propertyId == 32402 ? "10.0%" : "0.2", disc.SubStats[0].Value);
    }

    private sealed class ArmorerAssetHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string json = Path.GetFileName(request.RequestUri!.AbsolutePath) switch
            {
                "locs.json" => """{"en":{"SharpCriticalDamage":"Laceration DMG","EpRecover":"Automatic Sharpness Accumulation","MedalName1001":"Shiyu Defense"}}""",
                "avatars.json" => """
                    {"1001":{"ProfessionType":"Armorer",
                    "BaseProps":{"21301":5000,"32401":100},
                    "GrowthProps":{"21301":1000000,"32401":100000},
                    "PromotionProps":[{"21301":200,"32401":50}],
                    "CoreEnhancementProps":[{}, {"21301":200,"21303":500,"32401":50,"32402":2500,"32403":20}]}}
                    """,
                "weapons.json" => """{"2001":{"ProfessionType":"Armorer"}}""",
                "medals.json" => """
                    {"1001":{"Name":"MedalName1001","Icon":"/ui/zzz/Shiyu01.png","TipNum":[],"PrefixIcon":""},
                     "2004":{"Name":"MedalName2004","Icon":"/ui/zzz/Tower04.png","TipNum":[0,2],"PrefixIcon":""}}
                    """,
                "equipments.json" => """{"Items":{},"Suits":{}}""",
                "equipment_level.json" or "weapon_level.json" or "weapon_star.json" => """{"Items":[{}]}""",
                _ => "{}"
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
    }
}
