using System.ComponentModel;
namespace EnkaDotNet.Enums
{
    public enum GameType
    {
        [Description("Genshin Impact")]
        Genshin = 0,

        [Description("Zenless Zone Zero")]
        ZZZ = 1,

        [Description("Honkai Star Rail")]
        HSR = 2,
    }
}