using EnkaDotNet.Utils.Common;
using Xunit;

namespace EnkaDotNet.Tests.Http
{
    public class EnkaTelemetryTests
    {
        [Theory]
        [InlineData("uid/123456789", "genshin")]
        [InlineData("zzz/uid/123456789", "zzz")]
        [InlineData("hsr/uid/123456789", "hsr")]
        [InlineData("ef/uid/1234567890123", "endfield")]
        [InlineData("profile/someone/?format=json", "profile")]
        [InlineData("/ZZZ/uid/1", "zzz")]
        [InlineData("assets/foo", "unknown")]
        [InlineData(null, "unknown")]
        [InlineData("", "unknown")]
        public void ResolveGame_MapsRelativeUrl(string? relativeUrl, string expected)
        {
            Assert.Equal(expected, EnkaTelemetry.ResolveGame(relativeUrl));
        }

        [Fact]
        public void HashUid_IsStableAndNonEmpty()
        {
            var a = EnkaTelemetry.HashUid("800000001");
            var b = EnkaTelemetry.HashUid("800000001");
            var c = EnkaTelemetry.HashUid("800000002");

            Assert.Equal(8, a.Length);
            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }
    }
}
