using System.Text.Json;
using EnkaDotNet.Enums;
using EnkaDotNet.Enums.EF;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.EF;
using Xunit;

namespace EnkaDotNet.Tests.EF
{
    public class EFApiResponseTests
    {
        [Fact]
        public void EFApiResponse_Deserialize_ParsesPlayerAndLongUid()
        {
            var json = """
                {
                  "playerInfo": {
                    "businessCard": {
                      "name": "Lawrence",
                      "signature": "Audentes fortuna iuvat",
                      "adventureLevel": 57,
                      "worldLevel": 7,
                      "platformRoleId": "4228833345",
                      "userAvatarId": 3,
                      "businessCardTopicId": 15,
                      "statistic": { "charNum": 24, "weaponNum": 49, "docNum": 138 },
                      "charList": [
                        { "templateId": "chr_0017_yvonne", "level": 80, "potentialLevel": 1 }
                      ]
                    },
                    "charData": [
                      {
                        "templateId": 27,
                        "level": 80,
                        "potentialLevel": 1,
                        "equip": [
                          {
                            "key": 3,
                            "value": {
                              "templateid": 2943,
                              "enhance": [{ "key": 3, "value": 2 }]
                            }
                          }
                        ],
                        "weapon": {
                          "templateId": 3265,
                          "weaponLv": 80,
                          "refineLv": 0,
                          "breakthroughLv": 3,
                          "attachedGem": {
                            "templateId": 1070,
                            "terms": [
                              { "termNumId": 68, "cost": 4 },
                              { "termNumId": 50, "cost": 1 }
                            ],
                            "domainId": 1
                          }
                        },
                        "skillInfo": {
                          "levelInfo": [
                            { "skillId": "chr_0017_yvonne_NormalSkill", "skillLevel": 6, "skillMaxLevel": 9, "skillEnhancedLevel": 6 }
                          ],
                          "normalSkill": "chr_0017_yvonne_NormalSkill"
                        },
                        "talent": {
                          "latestBreakNode": "equipBreakT4",
                          "attrNodes": ["chr_0017_yvonne_1"]
                        },
                        "equipMedicineId": 955
                      }
                    ]
                  },
                  "uid": "4228833345",
                  "ttl": 60,
                  "region": "ASIA"
                }
                """;

            var model = JsonSerializer.Deserialize<EFApiResponse>(json);

            Assert.NotNull(model);
            Assert.Equal("4228833345", model.Uid);
            Assert.Equal(60, model.Ttl);
            Assert.Equal("ASIA", model.Region);
            Assert.NotNull(model.PlayerInfo);
            Assert.NotNull(model.PlayerInfo.BusinessCard);
            Assert.Equal("Lawrence", model.PlayerInfo.BusinessCard.Name);
            Assert.Equal(57, model.PlayerInfo.BusinessCard.AdventureLevel);
            Assert.Equal(7, model.PlayerInfo.BusinessCard.WorldLevel);
            Assert.Equal("4228833345", model.PlayerInfo.BusinessCard.PlatformRoleId);
            Assert.Equal(24, model.PlayerInfo.BusinessCard.Statistic.CharNum);
            Assert.Single(model.PlayerInfo.BusinessCard.CharList);
            Assert.Equal("chr_0017_yvonne", model.PlayerInfo.BusinessCard.CharList[0].TemplateId);

            var op = Assert.Single(model.PlayerInfo.CharData);
            Assert.Equal(27, op.TemplateId);
            Assert.Equal(80, op.Level);
            Assert.Equal(1, op.PotentialLevel);
            Assert.Equal(955, op.EquipMedicineId);

            var equip = Assert.Single(op.Equip);
            Assert.Equal(3, equip.Key);
            Assert.Equal(2943, equip.Value.TemplateId);
            Assert.Equal(3, equip.Value.Enhance[0].Key);
            Assert.Equal(2, equip.Value.Enhance[0].Value);

            Assert.NotNull(op.Weapon);
            Assert.Equal(3265, op.Weapon.TemplateId);
            Assert.Equal(80, op.Weapon.WeaponLv);
            Assert.Equal(3, op.Weapon.BreakthroughLv);
            Assert.Equal(1070, op.Weapon.AttachedGem.TemplateId);
            Assert.Equal(68, op.Weapon.AttachedGem.Terms[0].TermNumId);
            Assert.Equal(4, op.Weapon.AttachedGem.Terms[0].Cost);

            Assert.Equal("chr_0017_yvonne_1", op.Talent.AttrNodes[0]);
            Assert.Equal(6, op.SkillInfo.LevelInfo[0].SkillLevel);
        }

        [Fact]
        public void GameType_Endfield_HasExpectedValue()
        {
            Assert.Equal(3, (int)GameType.Endfield);
        }

        [Theory]
        [InlineData(EFAttrType.Unknown, 0)]
        [InlineData(EFAttrType.Defense, 3)]
        [InlineData(EFAttrType.CritRate, 9)]
        [InlineData(EFAttrType.ArtsIntensity, 17)]
        [InlineData(EFAttrType.Strength, 39)]
        [InlineData(EFAttrType.Agility, 40)]
        [InlineData(EFAttrType.Intellect, 41)]
        [InlineData(EFAttrType.Will, 42)]
        [InlineData(EFAttrType.CryoDMG, 53)]
        public void EFAttrType_HasCorrectValue(EFAttrType attrType, int expected)
        {
            Assert.Equal(expected, (int)attrType);
        }

        [Fact]
        public void PlayerNotFoundException_AcceptsLongUid()
        {
            long uid = 4228833345L;
            var ex = new PlayerNotFoundException(uid);
            Assert.Equal(uid, ex.Uid);
        }

        [Fact]
        public void ProfilePrivateException_AcceptsLongUid()
        {
            long uid = 4228833345L;
            var ex = new ProfilePrivateException(uid);
            Assert.Equal(uid, ex.Uid);
        }
    }
}
