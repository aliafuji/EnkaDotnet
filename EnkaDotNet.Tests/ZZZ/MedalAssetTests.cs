using System.Text.Json;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Serialization;
using Xunit;

namespace EnkaDotNet.Tests.ZZZ;

public class MedalAssetTests
{
    [Theory]
    [InlineData("[]", new int[0])]
    [InlineData("[1]", new[] { 1 })]
    [InlineData("[0,2]", new[] { 0, 2 })]
    public void DeserializeMedals_PreservesTipNumbersAcrossBothSerializers(string tipNumbers, int[] expected)
    {
        string json = """
            {"1001":{"Name":"MedalName1001","Icon":"/ui/zzz/Shiyu01.png","TipNum":TIP_NUMBERS,"PrefixIcon":""}}
            """.Replace("TIP_NUMBERS", tipNumbers, StringComparison.Ordinal);

        var generated = JsonSerializer.Deserialize<Dictionary<string, ZZZMedalAssetInfo>>(
            json, EnkaJsonContext.Default.Options)!;
        var reflection = JsonSerializer.Deserialize<Dictionary<string, ZZZMedalAssetInfo>>(json)!;

        foreach (var medals in new[] { generated, reflection })
        {
            Assert.Equal(expected, medals["1001"].TipNum);
            Assert.Equal("MedalName1001", medals["1001"].Name);
            Assert.Equal("/ui/zzz/Shiyu01.png", medals["1001"].Icon);
            Assert.Equal("", medals["1001"].PrefixIcon);
        }
    }
}
