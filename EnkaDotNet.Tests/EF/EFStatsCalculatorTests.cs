using System.Collections.Generic;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Assets.EF.Models;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Models.EF;
using EnkaDotNet.Utils.EF;
using Moq;
using Xunit;

namespace EnkaDotNet.Tests.EF
{
    public class EFStatsCalculatorTests
    {
        [Fact]
        public void CalculateOperatorStats_MatchesYvonneWebsiteNumbers()
        {
            var assets = CreateYvonneAssetsMock();
            var model = CreateYvonneCharModel();

            var result = EFStatsCalculator.CalculateFromModel(model, assets.Object);

            Assert.Equal(5304, System.Math.Floor(result.Hp));
            Assert.Equal(2983, System.Math.Floor(result.Atk));
            Assert.Equal(454, result.WeaponAtk);
            Assert.Equal(74, System.Math.Floor(result.Attrs[(int)EFAttrType.Strength]));
            Assert.Equal(258, System.Math.Floor(result.Attrs[(int)EFAttrType.Agility]));
            Assert.Equal(501, System.Math.Floor(result.Attrs[(int)EFAttrType.Intellect]));
            Assert.Equal(94, System.Math.Floor(result.Attrs[(int)EFAttrType.Will]));
            Assert.Equal(140, result.Attrs[(int)EFAttrType.Defense]);
            Assert.InRange(result.Attrs[(int)EFAttrType.CritRate] * 100.0, 42.3, 42.5);
            Assert.Equal(0.16, result.Attrs[53], 3);
        }

        [Fact]
        public void CalculateOperatorStats_EnhanceUsesAttrIndexNotAttrType()
        {
            var assets = new Mock<IEFAssets>();
            var avatar = new EFAvatarAssetInfo
            {
                MainAttrId = 41,
                SubAttrId = 40,
                BaseHpByLevel = Fill(80, 1000),
                BaseAtkByLevel = Fill(80, 100),
                BaseAttributes = new Dictionary<string, EFBaseAttributeValue>
                {
                    ["39"] = new EFBaseAttributeValue { BaseValue = 10, AddValue = 0 },
                    ["40"] = new EFBaseAttributeValue { BaseValue = 10, AddValue = 0 },
                    ["41"] = new EFBaseAttributeValue { BaseValue = 10, AddValue = 0 },
                    ["42"] = new EFBaseAttributeValue { BaseValue = 10, AddValue = 0 },
                    ["9"] = new EFBaseAttributeValue { BaseValue = 0.05, AddValue = 0 }
                }
            };
            assets.Setup(a => a.GetAvatarInfo(1)).Returns(avatar);
            assets.Setup(a => a.GetEquipItem(100)).Returns(new EFEquipItemInfo
            {
                SuitId = "suit_x",
                AttrModifiers = new List<EFAttrModifierInfo>
                {
                    new() { AttrType = 3, Values = new List<double> { 10, 10, 10, 10 } },
                    new() { AttrType = 41, Values = new List<double> { 1, 2, 3, 4 } },
                    new() { AttrType = 40, Values = new List<double> { 1, 2, 3, 4 } },
                    new() { AttrType = 9, Values = new List<double> { 0.10, 0.11, 0.12, 0.13 } }
                }
            });
            assets.Setup(a => a.GetSuitInfo(It.IsAny<string>())).Returns((EFEquipSuitInfo?)null);

            var model = new EFCharDataModel
            {
                TemplateId = 1,
                Level = 1,
                Equip = new List<EFEquipEntryModel>
                {
                    new()
                    {
                        Key = 0,
                        Value = new EFEquipValueModel
                        {
                            TemplateId = 100,
                            Enhance = new List<EFIntKeyValueModel> { new() { Key = 3, Value = 2 } }
                        }
                    }
                }
            };

            var result = EFStatsCalculator.CalculateFromModel(model, assets.Object);

            Assert.Equal(0.05 + 0.12, result.Attrs[9], 5);
            Assert.Equal(11, result.Attrs[41]);
            Assert.Equal(11, result.Attrs[40]);
        }

        [Fact]
        public void MapAttrType_KnownIds_MapCorrectly()
        {
            Assert.Equal(EFAttrType.Strength, EFStatsCalculator.MapAttrType(39));
            Assert.Equal(EFAttrType.Agility, EFStatsCalculator.MapAttrType(40));
            Assert.Equal(EFAttrType.Intellect, EFStatsCalculator.MapAttrType(41));
            Assert.Equal(EFAttrType.Will, EFStatsCalculator.MapAttrType(42));
            Assert.Equal(EFAttrType.CritRate, EFStatsCalculator.MapAttrType(9));
            Assert.Equal(EFAttrType.Defense, EFStatsCalculator.MapAttrType(3));
            Assert.Equal(EFAttrType.ArtsIntensity, EFStatsCalculator.MapAttrType(17));
            Assert.Equal(EFAttrType.CryoDMG, EFStatsCalculator.MapAttrType(53));
            Assert.Equal(EFAttrType.Unknown, EFStatsCalculator.MapAttrType(999));
        }

        [Fact]
        public void EFDataMapper_MapPlayerInfo_MapsCardAndOperators()
        {
            var assets = CreateYvonneAssetsMock();
            assets.Setup(a => a.ResolveStrIdToTemplateId("chr_0017_yvonne")).Returns(27);
            assets.Setup(a => a.GetOperatorName(27)).Returns("Yvonne");
            assets.Setup(a => a.GetOperatorIconUrl(27)).Returns("https://enka.network/ui/ef/icon.png");
            assets.Setup(a => a.GetOperatorIconUrl("chr_0017_yvonne")).Returns("https://enka.network/ui/ef/icon.png");
            assets.Setup(a => a.GetProfilePictureIconUrl(It.IsAny<int>())).Returns(string.Empty);
            assets.Setup(a => a.GetNameCardIconUrl(It.IsAny<int>())).Returns(string.Empty);
            assets.Setup(a => a.GetWeaponName(3265)).Returns("Artzy Tyrannical");
            assets.Setup(a => a.GetWeaponIconUrl(3265)).Returns("https://enka.network/ui/ef/itemicon/wpn_pistol_0010.png");
            assets.Setup(a => a.GetEquipIconUrl(It.IsAny<int>())).Returns("https://enka.network/ui/ef/itemicon/equip.png");
            assets.Setup(a => a.GetSuitName("suit_criti01")).Returns("MI Security");
            assets.Setup(a => a.GetSuitIconUrl("suit_criti01")).Returns("https://enka.network/ui/ef/equipmentlogobigwhite/icon_pack_wuling_suit_criti01.png");
            assets.Setup(a => a.GetOperatorRoundIconUrl(27)).Returns("https://enka.network/ui/ef/charroundicon/icon_round_chr_0017_yvonne.png");
            assets.Setup(a => a.GetOperatorSplashArtUrl(27)).Returns("https://enka.network/ui/ef/splash/chr_0017_yvonne.webp");
            assets.Setup(a => a.GetOperatorSilhouetteUrl(27)).Returns("https://enka.network/ui/ef/charinfo/bg_charinfo_chr_0017_yvonne.png");
            assets.Setup(a => a.GetProfessionIconUrl("ASSAULT")).Returns("https://enka.network/ui/ef/profession/ASSAULT.png");
            assets.Setup(a => a.GetGemIconUrl(It.IsAny<int>())).Returns("https://enka.network/ui/ef/item_gem_rarity_5.png");
            assets.Setup(a => a.GetSkillIconUrl(It.IsAny<int>(), It.IsAny<string>()))
                .Returns("https://enka.network/ui/ef/skillicon/icon.png");
            assets.Setup(a => a.GetLocalizedText(It.IsAny<string>())).Returns((string key) => key);
            assets.Setup(a => a.GetWeaponMeta()).Returns(new EFWeaponMetaData
            {
                LevelCurves = new Dictionary<string, List<double>>
                {
                    ["curve"] = Fill(90, 454)
                }
            });
            var mapper = new EFDataMapper(assets.Object, new EnkaClientOptions());
            var response = new EFApiResponse
            {
                Uid = "4228833345",
                Ttl = 60,
                Region = "ASIA",
                PlayerInfo = new EFPlayerInfoModel
                {
                    BusinessCard = new EFBusinessCardModel
                    {
                        Name = "Lawrence",
                        Signature = "Audentes fortuna iuvat",
                        AdventureLevel = 57,
                        WorldLevel = 7,
                        PlatformRoleId = "4228833345",
                        Statistic = new EFStatisticModel { CharNum = 24, WeaponNum = 49, DocNum = 138 },
                        CharList = new List<EFCharListEntryModel>
                        {
                            new() { TemplateId = "chr_0017_yvonne", Level = 80, PotentialLevel = 1 }
                        }
                    },
                    CharData = new List<EFCharDataModel> { CreateYvonneCharModel() }
                }
            };

            var player = mapper.MapPlayerInfo(response);

            Assert.Equal("Lawrence", player.Nickname);
            Assert.Equal(57, player.AdminLevel);
            Assert.Equal(7, player.EndfieldLevel);
            Assert.Equal(4228833345L, player.Uid);
            Assert.Equal("ASIA", player.Region);
            Assert.Equal(24, player.CharCount);
            Assert.Single(player.CharList);
            Assert.Equal(27, player.CharList[0].ResolvedTemplateId);
            Assert.Equal("Yvonne", player.CharList[0].Name);

            var op = Assert.Single(player.ShowcaseOperators);
            Assert.Equal(27, op.Id);
            Assert.Equal("Yvonne", op.Name);
            Assert.Equal(80, op.Level);
            Assert.Equal(5304, System.Math.Floor(op.Hp));
            Assert.Equal(2983, System.Math.Floor(op.Atk));
            Assert.Equal("Artzy Tyrannical", op.Weapon.Name);
            Assert.Equal(454, op.Weapon.BaseAtk);
            Assert.Equal(4, op.Equips.Count);

            var stats = op.GetAllStats();
            Assert.Equal("5304", stats["HP"]);
            Assert.Equal("2983", stats["ATK"]);
            Assert.Equal("42.4%", stats["Critical Rate"]);
            Assert.Equal("50.0%", stats["Critical DMG"]);
            Assert.Equal("9.4%", stats["Treatment Received Bonus"]);
            Assert.Equal("16.0%", stats["Cryo DMG Bonus"]);
            Assert.Equal(80, op.Weapon.MaxLevel);
            Assert.Equal("Intellect", op.Weapon.SubStats[0].Name);
            Assert.Equal("116", op.Weapon.SubStats[0].FormattedValue);
            Assert.Equal("6.5%", op.Weapon.SubStats[1].FormattedValue);
            Assert.Equal("https://enka.network/ui/ef/itemicon/wpn_pistol_0010.png", op.Weapon.IconUrl);
            Assert.All(op.Equips, e => Assert.False(string.IsNullOrEmpty(e.IconUrl)));
            Assert.All(op.Equips, e => Assert.False(string.IsNullOrEmpty(e.SuitIconUrl)));
            Assert.Equal("https://enka.network/ui/ef/charroundicon/icon_round_chr_0017_yvonne.png", op.RoundIconUrl);
            Assert.Equal("https://enka.network/ui/ef/profession/ASSAULT.png", op.ProfessionIconUrl);
            Assert.Contains(op.Skills, s => s.IsCombatSkill && s.Name == "Battle Skill" && s.Level == 6);
            Assert.Equal(2, op.Talents.Count);
            Assert.Equal(2, op.Talents[0].Rank);
            Assert.Equal("Talent 1", op.Talents[0].Name);
            Assert.Contains("icon_talent_yvonne_01.png", op.Talents[0].IconUrl);
            Assert.Equal(2, op.Talents[1].Rank);
        }

        private static Mock<IEFAssets> CreateYvonneAssetsMock()
        {
            var assets = new Mock<IEFAssets>();

            var avatar = new EFAvatarAssetInfo
            {
                StrId = "chr_0017_yvonne",
                NameHash = "-8634541419450148796",
                Rarity = 6,
                Element = "Cryst",
                Profession = "ASSAULT",
                WeaponType = "Pistol",
                MainAttrId = 41,
                SubAttrId = 40,
                BaseHpByLevel = Fill(80, 4934),
                BaseAtkByLevel = Fill(80, 288),
                AttributeNodes = new Dictionary<string, Dictionary<string, double>>
                {
                    ["chr_0017_yvonne_1"] = new Dictionary<string, double> { ["41"] = 10 }
                },
                BaseAttributes = new Dictionary<string, EFBaseAttributeValue>
                {
                    ["39"] = new EFBaseAttributeValue { BaseValue = 8.350515463917557, AddValue = 0.8350515463917522 },
                    ["40"] = new EFBaseAttributeValue { BaseValue = 14.7422680412371, AddValue = 1.2742688828108597 },
                    ["41"] = new EFBaseAttributeValue { BaseValue = 24.58762886597939, AddValue = 1.708920681674737 },
                    ["42"] = new EFBaseAttributeValue { BaseValue = 10.618556701030965, AddValue = 1.0618556701030923 },
                    ["9"] = new EFBaseAttributeValue { BaseValue = 0.05, AddValue = 0 },
                    ["49"] = new EFBaseAttributeValue { BaseValue = 1, AddValue = 0.00510204081632653 },
                    ["25"] = new EFBaseAttributeValue { BaseValue = 1, AddValue = 0.002551020408163265 }
                },
                SkillInfoMap = new Dictionary<string, EFAvatarSkillInfo>(),
                NodeSkillMap = new Dictionary<string, EFAvatarNodeSkillInfo>
                {
                    ["chr_0017_yvonne_passive_skill_0_2"] = new EFAvatarNodeSkillInfo
                    {
                        Icon = "/ui/ef/icon_talent_yvonne_01.png",
                        Level = 2,
                        Index = 0,
                        Type = 4
                    },
                    ["chr_0017_yvonne_passive_skill_1_2"] = new EFAvatarNodeSkillInfo
                    {
                        Icon = "/ui/ef/icon_talent_yvonne_02.png",
                        Level = 2,
                        Index = 1,
                        Type = 4
                    }
                },
                PotAttributes = new List<EFPotAttributeEntry>()
            };
            assets.Setup(a => a.GetAvatarInfo(27)).Returns(avatar);
            assets.Setup(a => a.GetAvatarInfo("27")).Returns(avatar);
            assets.Setup(a => a.GetNodeSkillInfo(27, It.IsAny<string>()))
                .Returns((int id, string nodeId) =>
                    avatar.NodeSkillMap != null && avatar.NodeSkillMap.TryGetValue(nodeId, out var node) ? node : null);
            assets.Setup(a => a.GetNodeSkillIconUrl(27, It.IsAny<string>()))
                .Returns((int id, string nodeId) =>
                {
                    if (avatar.NodeSkillMap != null && avatar.NodeSkillMap.TryGetValue(nodeId, out var node) && !string.IsNullOrEmpty(node.Icon))
                    {
                        string file = node.Icon.Substring(node.Icon.LastIndexOf('/') + 1);
                        return "https://enka.network/ui/ef/skillicon/" + file;
                    }
                    return string.Empty;
                });

            assets.Setup(a => a.GetEquipItem(2943)).Returns(new EFEquipItemInfo
            {
                Rarity = 5,
                SuitId = "suit_criti01",
                AttrModifiers = new List<EFAttrModifierInfo>
                {
                    new() { AttrType = 3, Values = new List<double> { 21, 21, 21, 21 } },
                    new() { AttrType = 41, Values = new List<double> { 32, 35, 38, 41 } },
                    new() { AttrType = 40, Values = new List<double> { 21, 23, 25, 27 } },
                    new() { AttrType = 9, Values = new List<double> { 0.1035, 0.11385, 0.1242, 0.13455 } }
                }
            });
            assets.Setup(a => a.GetEquipItem(3109)).Returns(new EFEquipItemInfo
            {
                Rarity = 5,
                SuitId = "suit_criti01",
                AttrModifiers = new List<EFAttrModifierInfo>
                {
                    new() { AttrType = 3, Values = new List<double> { 56, 56, 56, 56 } },
                    new() { AttrType = 41, Values = new List<double> { 87, 95, 104, 113 } },
                    new() { AttrType = 40, Values = new List<double> { 58, 63, 69, 75 } },
                    new() { AttrType = 17, Values = new List<double> { 0.138, 0.1518, 0.1656, 0.1794 } }
                }
            });
            assets.Setup(a => a.GetEquipItem(2814)).Returns(new EFEquipItemInfo
            {
                Rarity = 5,
                SuitId = "suit_criti01",
                AttrModifiers = new List<EFAttrModifierInfo>
                {
                    new() { AttrType = 3, Values = new List<double> { 42, 42, 42, 42 } },
                    new() { AttrType = 41, Values = new List<double> { 65, 71, 78, 84 } },
                    new() { AttrType = 40, Values = new List<double> { 43, 47, 51, 55 } },
                    new() { AttrType = 17, Values = new List<double> { 0.23, 0.253, 0.276, 0.299 } }
                }
            });

            assets.Setup(a => a.GetWeaponInfo(3265)).Returns(new EFWeaponAssetInfo
            {
                Rarity = 6,
                NameHash = "8266882546363894424",
                WeaponType = "Pistol",
                LevelTemplateId = "curve",
                BreakthroughTemplateId = "break",
                TalentTemplateId = "talent",
                SkillList = new List<int> { 1247, 1261, 2252 }
            });
            assets.Setup(a => a.GetWeaponAtk("curve", 80)).Returns(454);
            assets.Setup(a => a.GetBreakBounds("break", 3)).Returns(new List<EFSkillLevelBound>
            {
                new() { LowerBound = 3, UpperBound = 8 },
                new() { LowerBound = 2, UpperBound = 7 },
                new() { LowerBound = 1, UpperBound = 4 }
            });

            assets.Setup(a => a.GetGemTermTag(68)).Returns(new EFGemTermInfo { TagId = "attr_wisd", TermType = 1 });
            assets.Setup(a => a.GetGemTermTag(50)).Returns(new EFGemTermInfo { TagId = "attr_crirate", TermType = 2 });
            assets.Setup(a => a.GetGemTermTag(74)).Returns(new EFGemTermInfo { TagId = "phyabn", TermType = 3 });

            assets.Setup(a => a.GetSkillProp(1247)).Returns(new EFSkillAssetInfo
            {
                TagId = "attr_wisd",
                PropMap = new Dictionary<string, EFSkillPropInfo>
                {
                    ["41"] = new EFSkillPropInfo { Values = new List<double> { 20, 36, 52, 68, 84, 100, 116, 132, 156 } }
                }
            });
            assets.Setup(a => a.GetSkillProp(1261)).Returns(new EFSkillAssetInfo
            {
                TagId = "attr_crirate",
                PropMap = new Dictionary<string, EFSkillPropInfo>
                {
                    ["9"] = new EFSkillPropInfo { Values = new List<double> { 0.025, 0.045, 0.065, 0.085, 0.105, 0.125, 0.145, 0.165, 0.195 } }
                }
            });
            assets.Setup(a => a.GetSkillProp(2252)).Returns(new EFSkillAssetInfo
            {
                TagId = "crit",
                PropMap = new Dictionary<string, EFSkillPropInfo>
                {
                    ["53"] = new EFSkillPropInfo { Values = new List<double> { 0.16, 0.192, 0.224, 0.256, 0.288, 0.32, 0.352, 0.384, 0.448 } }
                }
            });
            assets.Setup(a => a.GetSkillProp(1068)).Returns(new EFSkillAssetInfo
            {
                TagId = "",
                PropMap = new Dictionary<string, EFSkillPropInfo>
                {
                    ["9"] = new EFSkillPropInfo { Values = new List<double> { 0.05 } }
                }
            });
            assets.Setup(a => a.GetSuitInfo("suit_criti01")).Returns(new EFEquipSuitInfo
            {
                NameHash = "1298184612084436356",
                SkillId = 1068
            });

            return assets;
        }

        private static EFCharDataModel CreateYvonneCharModel()
        {
            return new EFCharDataModel
            {
                TemplateId = 27,
                Level = 80,
                PotentialLevel = 1,
                EquipMedicineId = 955,
                Talent = new EFTalentModel
                {
                    LatestBreakNode = "equipBreakT4",
                    AttrNodes = new List<string> { "chr_0017_yvonne_1" },
                    LatestPassiveSkillNodes = new List<string>
                    {
                        "chr_0017_yvonne_passive_skill_0_2",
                        "chr_0017_yvonne_passive_skill_1_2"
                    }
                },
                SkillInfo = new EFSkillInfoModel
                {
                    LevelInfo = new List<EFSkillLevelModel>
                    {
                        new() { SkillId = "chr_0017_yvonne_NormalSkill", SkillLevel = 6, SkillMaxLevel = 9, SkillEnhancedLevel = 6 },
                        new() { SkillId = "chr_0017_yvonne_NormalAttack", SkillLevel = 9, SkillMaxLevel = 9, SkillEnhancedLevel = 9 },
                        new() { SkillId = "chr_0017_yvonne_UltimateSkill", SkillLevel = 9, SkillMaxLevel = 9, SkillEnhancedLevel = 9 },
                        new() { SkillId = "chr_0017_yvonne_ComboSkill", SkillLevel = 9, SkillMaxLevel = 9, SkillEnhancedLevel = 9 }
                    }
                },
                Equip = new List<EFEquipEntryModel>
                {
                    new()
                    {
                        Key = 3,
                        Value = new EFEquipValueModel
                        {
                            TemplateId = 2943,
                            Enhance = new List<EFIntKeyValueModel> { new() { Key = 3, Value = 2 } }
                        }
                    },
                    new()
                    {
                        Key = 2,
                        Value = new EFEquipValueModel
                        {
                            TemplateId = 2943,
                            Enhance = new List<EFIntKeyValueModel> { new() { Key = 3, Value = 3 } }
                        }
                    },
                    new()
                    {
                        Key = 1,
                        Value = new EFEquipValueModel { TemplateId = 3109, Enhance = new List<EFIntKeyValueModel>() }
                    },
                    new()
                    {
                        Key = 0,
                        Value = new EFEquipValueModel { TemplateId = 2814, Enhance = new List<EFIntKeyValueModel>() }
                    }
                },
                Weapon = new EFWeaponModel
                {
                    TemplateId = 3265,
                    WeaponLv = 80,
                    RefineLv = 0,
                    BreakthroughLv = 3,
                    AttachedGem = new EFGemModel
                    {
                        TemplateId = 1070,
                        DomainId = 1,
                        Terms = new List<EFGemTermModel>
                        {
                            new() { TermNumId = 68, Cost = 4 },
                            new() { TermNumId = 50, Cost = 1 },
                            new() { TermNumId = 74, Cost = 1 }
                        }
                    }
                }
            };
        }

        [Fact]
        public void CalculateAllTotalStats_UsesStableEnglishKeysAndLocalizedNames()
        {
            var assets = CreateYvonneAssetsMock();
            assets.Setup(a => a.GetLocalizedText(It.IsAny<string>())).Returns((string key) => key switch
            {
                "MaxHp" => "ＨＰ",
                "Atk" => "攻撃力",
                "CriticalRate" => "会心率",
                "CrystDamageIncrease" => "寒冷ダメージUP",
                "CriticalDamageIncrease" => "会心ダメージ",
                "HealTakenIncrease" => "被治療効果",
                "Str" => "筋力",
                "Agi" => "敏捷",
                "Wisd" => "知性",
                "Will" => "意志",
                "Def" => "防御力",
                _ => key
            });
            assets.Setup(a => a.GetOperatorSplashArtUrl(27)).Returns(string.Empty);
            assets.Setup(a => a.GetOperatorSilhouetteUrl(27)).Returns(string.Empty);
            assets.Setup(a => a.GetOperatorRoundIconUrl(27)).Returns(string.Empty);
            assets.Setup(a => a.GetProfessionIconUrl(It.IsAny<string>())).Returns(string.Empty);
            assets.Setup(a => a.GetWeaponName(3265)).Returns("Artzy Tyrannical");
            assets.Setup(a => a.GetWeaponIconUrl(3265)).Returns(string.Empty);
            assets.Setup(a => a.GetEquipIconUrl(It.IsAny<int>())).Returns(string.Empty);
            assets.Setup(a => a.GetSuitName(It.IsAny<string>())).Returns("MI Security");
            assets.Setup(a => a.GetSuitIconUrl(It.IsAny<string>())).Returns(string.Empty);
            assets.Setup(a => a.GetGemIconUrl(It.IsAny<int>())).Returns(string.Empty);
            assets.Setup(a => a.GetSkillIconUrl(It.IsAny<int>(), It.IsAny<string>())).Returns(string.Empty);
            assets.Setup(a => a.GetWeaponMeta()).Returns(new EFWeaponMetaData
            {
                LevelCurves = new Dictionary<string, List<double>> { ["curve"] = Fill(90, 454) }
            });

            var mapper = new EFDataMapper(assets.Object, new EnkaClientOptions());
            var op = mapper.MapOperator(CreateYvonneCharModel());
            var totals = op.CalculateAllTotalStats();

            Assert.True(totals.ContainsKey("HP"));
            Assert.True(totals.ContainsKey("ATK"));
            Assert.True(totals.ContainsKey("Critical Rate"));
            Assert.True(totals.ContainsKey("Cryo DMG Bonus"));
            Assert.Equal("ＨＰ", totals["HP"].LocalizedName);
            Assert.Equal("会心率", totals["Critical Rate"].LocalizedName);
            Assert.Equal("寒冷ダメージUP", totals["Cryo DMG Bonus"].LocalizedName);
            Assert.Equal(5304, System.Math.Floor(totals["HP"].Value));
            Assert.Equal(2983, System.Math.Floor(totals["ATK"].Value));
            Assert.InRange(totals["Critical Rate"].Value, 42.3, 42.5);
            Assert.True(totals["Critical Rate"].IsPercentage);

            var display = op.GetAllStats();
            Assert.Equal("5304", display["ＨＰ"]);
            Assert.Equal("42.4%", display["会心率"]);

            var finalStats = op.GetFinalStats();
            Assert.Equal(5304, finalStats["HP"]);
            Assert.Equal(2983, finalStats["ATK"]);
            Assert.True(finalStats.ContainsKey("Critical Rate"));
            Assert.Equal(42.4, finalStats["Critical Rate"]);

            var rawStats = op.GetFinalStats(raw: true);
            Assert.InRange(rawStats["ATK"], 2983, 2984);
            Assert.InRange(rawStats["Critical Rate"], 0.423, 0.425);
        }

        private static List<double> Fill(int count, double value)
        {
            var list = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(value);
            }
            return list;
        }
    }
}
