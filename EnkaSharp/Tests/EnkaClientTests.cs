using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using EnkaSharp.Assets.Genshin;
using EnkaSharp.Exceptions;
using EnkaSharp.Utils.Common;

namespace EnkaSharp.Tests
{
    [TestClass]
    public class EnkaClientTests
    {
        private EnkaClient _client;
        private const int ValidUid = 829344442;
        private const int InvalidUid = 901211015;
        private const string AssetPath = "enka_assets";

        [TestInitialize]
        public void Initialize()
        {
            _client = new EnkaClient(AssetPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
        }

        [TestMethod]
        public void Constructor_WithAssetsPath_InitializesCorrectly()
        {
            var client = new EnkaClient(AssetPath);
            Assert.IsNotNull(client);
            client.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullAssetsPath_ThrowsException()
        {
            new EnkaClient(null);
        }

        [TestMethod]
        public async Task GetRawUserResponseAsync_WithValidUid_ReturnsApiResponse()
        {
            var result = await _client.GetRawUserResponseAsync(ValidUid);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PlayerInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetRawUserResponseAsync_WithZeroUid_ThrowsArgumentException()
        {
            await _client.GetRawUserResponseAsync(0);
        }

        [TestMethod]
        public async Task GetPlayerInfoAsync_WithValidUid_ReturnsPlayerInfo()
        {
            var result = await _client.GetPlayerInfoAsync(ValidUid);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Nickname);
            Assert.IsTrue(result.Level > 0);
        }

        [TestMethod]
        public async Task GetCharactersAsync_WithValidUid_ReturnsCharacters()
        {
            var result = await _client.GetCharactersAsync(ValidUid);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task GetUserProfile_WithValidUid_ReturnsPlayerInfoAndCharacters()
        {
            var result = await _client.GetUserProfile(ValidUid);

            Assert.IsNotNull(result.PlayerInfo);
            Assert.IsNotNull(result.Characters);
            Assert.IsTrue(result.PlayerInfo.Nickname.Length > 0);
            Assert.IsTrue(result.Characters.Count > 0);
        }

        [TestMethod]
        public async Task ProfileTests_WithInvalidUid_HandlesErrorsGracefully()
        {
            try
            {
                await _client.GetUserProfile(InvalidUid);
                Assert.Fail("Should have thrown an exception for invalid UID");
            }
            catch (PlayerNotFoundException)
            {
                Assert.IsTrue(true);
            }
            catch (ProfilePrivateException)
            {
                Assert.IsTrue(true);
            }
            catch (EnkaNetworkException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void CacheStats_ReturnsValidData()
        {
            var stats = _client.GetCacheStats();

            Assert.IsNotNull(stats);
            var (count, expiredCount) = stats;
        }

        [TestMethod]
        public void ClearCache_DoesNotThrowException()
        {
            // Act & Assert - just verify no exception
            _client.ClearCache();
        }
    }
}