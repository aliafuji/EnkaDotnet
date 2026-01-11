using System.Text.Json;
using EnkaDotNet;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Assets.Genshin;
using Moq;
using Xunit;

namespace EnkaDotNet.Tests.Genshin
{
    public class GenshinPlayerInfoTests
    {
        [Fact]
        public void PlayerInfoModel_Deserialize_WithNewFields_ParsesCorrectly()
        {
            var json = @"{
                ""nickname"": ""TestPlayer"",
                ""level"": 60,
                ""signature"": ""Test signature"",
                ""worldLevel"": 8,
                ""nameCardId"": 210001,
                ""finishAchievementNum"": 500,
                ""towerFloorIndex"": 12,
                ""towerLevelIndex"": 3,
                ""towerStarIndex"": 36,
                ""theaterActIndex"": 10,
                ""theaterStarIndex"": 48,
                ""stygianIndex"": 5,
                ""stygianSeconds"": 180,
                ""fetterCount"": 25
            }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal("TestPlayer", model.Nickname);
            Assert.Equal(60, model.Level);
            Assert.Equal(5, model.StygianIndex);
            Assert.Equal(180, model.StygianSeconds);
            Assert.Equal(25, model.FetterCount);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_StygianIndex_ParsesCorrectly()
        {
            var json = @"{ ""stygianIndex"": 7 }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(7, model.StygianIndex);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_StygianSeconds_ParsesCorrectly()
        {
            var json = @"{ ""stygianSeconds"": 300 }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(300, model.StygianSeconds);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_FetterCount_ParsesCorrectly()
        {
            var json = @"{ ""fetterCount"": 42 }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal(42, model.FetterCount);
        }
    }

    public class GenshinPlayerInfoBackwardCompatibilityTests
    {
        [Fact]
        public void PlayerInfoModel_Deserialize_WithoutNewFields_DefaultsToNull()
        {
            var json = @"{
                ""nickname"": ""TestPlayer"",
                ""level"": 60,
                ""signature"": ""Test signature"",
                ""worldLevel"": 8,
                ""nameCardId"": 210001,
                ""finishAchievementNum"": 500,
                ""towerFloorIndex"": 12,
                ""towerLevelIndex"": 3,
                ""towerStarIndex"": 36,
                ""theaterActIndex"": 10,
                ""theaterStarIndex"": 48
            }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Equal("TestPlayer", model.Nickname);
            Assert.Equal(60, model.Level);
            Assert.Null(model.StygianIndex);
            Assert.Null(model.StygianSeconds);
            Assert.Null(model.FetterCount);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_WithoutStygianIndex_DefaultsToNull()
        {
            var json = @"{ ""nickname"": ""Test"" }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Null(model.StygianIndex);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_WithoutStygianSeconds_DefaultsToNull()
        {
            var json = @"{ ""nickname"": ""Test"" }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Null(model.StygianSeconds);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_WithoutFetterCount_DefaultsToNull()
        {
            var json = @"{ ""nickname"": ""Test"" }";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Null(model.FetterCount);
        }

        [Fact]
        public void PlayerInfoModel_Deserialize_EmptyJson_AllNewFieldsNull()
        {
            var json = @"{}";

            var model = JsonSerializer.Deserialize<PlayerInfoModel>(json);

            Assert.NotNull(model);
            Assert.Null(model.StygianIndex);
            Assert.Null(model.StygianSeconds);
            Assert.Null(model.FetterCount);
        }
    }


    public class GenshinDataMapperTests
    {
        [Fact]
        public void MapPlayerInfo_MapsStygianIndexToDifficulty()
        {
            var mockAssets = new Mock<IGenshinAssets>();
            var options = new EnkaClientOptions();
            var mapper = new DataMapper(mockAssets.Object, options);

            var model = new PlayerInfoModel
            {
                Nickname = "TestPlayer",
                Level = 60,
                StygianIndex = 5,
                StygianSeconds = 180,
                FetterCount = 25
            };

            var playerInfo = mapper.MapPlayerInfo(model);

            Assert.NotNull(playerInfo);
            Assert.NotNull(playerInfo.Challenge);
            Assert.NotNull(playerInfo.Challenge.Stygian);
            Assert.Equal(5, playerInfo.Challenge.Stygian.Difficulty);
        }

        [Fact]
        public void MapPlayerInfo_MapsStygianSecondsToClearTime()
        {
            var mockAssets = new Mock<IGenshinAssets>();
            var options = new EnkaClientOptions();
            var mapper = new DataMapper(mockAssets.Object, options);

            var model = new PlayerInfoModel
            {
                Nickname = "TestPlayer",
                Level = 60,
                StygianIndex = 7,
                StygianSeconds = 300,
                FetterCount = 10
            };

            var playerInfo = mapper.MapPlayerInfo(model);

            Assert.NotNull(playerInfo);
            Assert.NotNull(playerInfo.Challenge);
            Assert.NotNull(playerInfo.Challenge.Stygian);
            Assert.Equal(300, playerInfo.Challenge.Stygian.ClearTime);
        }

        [Fact]
        public void MapPlayerInfo_MapsFetterCountToMaxFriendshipCharacterCount()
        {
            var mockAssets = new Mock<IGenshinAssets>();
            var options = new EnkaClientOptions();
            var mapper = new DataMapper(mockAssets.Object, options);

            var model = new PlayerInfoModel
            {
                Nickname = "TestPlayer",
                Level = 60,
                StygianIndex = 3,
                StygianSeconds = 120,
                FetterCount = 42
            };

            var playerInfo = mapper.MapPlayerInfo(model);

            Assert.NotNull(playerInfo);
            Assert.Equal(42, playerInfo.MaxFriendshipCharacterCount);
        }

        [Fact]
        public void MapPlayerInfo_WithNullNewFields_MapsToNull()
        {
            var mockAssets = new Mock<IGenshinAssets>();
            var options = new EnkaClientOptions();
            var mapper = new DataMapper(mockAssets.Object, options);

            var model = new PlayerInfoModel
            {
                Nickname = "TestPlayer",
                Level = 60,
                StygianIndex = null,
                StygianSeconds = null,
                FetterCount = null
            };

            var playerInfo = mapper.MapPlayerInfo(model);

            Assert.NotNull(playerInfo);
            Assert.NotNull(playerInfo.Challenge);
            Assert.NotNull(playerInfo.Challenge.Stygian);
            Assert.Null(playerInfo.Challenge.Stygian.Difficulty);
            Assert.Null(playerInfo.Challenge.Stygian.ClearTime);
            Assert.Null(playerInfo.MaxFriendshipCharacterCount);
        }

        [Fact]
        public void MapPlayerInfo_MapsAllNewFieldsCorrectly()
        {
            var mockAssets = new Mock<IGenshinAssets>();
            var options = new EnkaClientOptions();
            var mapper = new DataMapper(mockAssets.Object, options);

            var model = new PlayerInfoModel
            {
                Nickname = "TestPlayer",
                Level = 60,
                Signature = "Test signature",
                WorldLevel = 8,
                NameCardId = 210001,
                FinishAchievementNum = 500,
                TowerFloorIndex = 12,
                TowerLevelIndex = 3,
                TowerStarIndex = 36,
                TheaterActIndex = 10,
                TheaterStarIndex = 48,
                StygianIndex = 6,
                StygianSeconds = 240,
                FetterCount = 30
            };

            var playerInfo = mapper.MapPlayerInfo(model);

            Assert.NotNull(playerInfo);
            Assert.Equal("TestPlayer", playerInfo.Nickname);
            Assert.Equal(60, playerInfo.Level);
            Assert.NotNull(playerInfo.Challenge.Stygian);
            Assert.Equal(6, playerInfo.Challenge.Stygian.Difficulty);
            Assert.Equal(240, playerInfo.Challenge.Stygian.ClearTime);
            Assert.Equal(30, playerInfo.MaxFriendshipCharacterCount);
            Assert.Equal(12, playerInfo.Challenge.SpiralAbyss.Floor);
            Assert.Equal(3, playerInfo.Challenge.SpiralAbyss.Chamber);
            Assert.Equal(36, playerInfo.Challenge.SpiralAbyss.Star);
            Assert.Equal(10, playerInfo.Challenge.Theater.Act);
            Assert.Equal(48, playerInfo.Challenge.Theater.Star);
        }
    }
}
