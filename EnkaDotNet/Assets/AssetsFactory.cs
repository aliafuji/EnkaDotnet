using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;

namespace EnkaDotNet.Assets
{
    public static class AssetsFactory
    {
        public static IAssets Create(string language = "en", GameType gameType = GameType.Genshin)
        {
            return gameType switch
            {
                GameType.Genshin => new GenshinAssets(language),
                GameType.ZZZ => new ZZZAssets(language),
                GameType.HSR => new HSRAssets(language),
                _ => throw new UnsupportedGameTypeException(gameType, $"Game type {gameType} is not supported.")
            };
        }

        public static IGenshinAssets CreateGenshin(string language = "en")
        {
            return new GenshinAssets(language);
        }

        public static IZZZAssets CreateZZZ(string language = "en")
        {
            return new ZZZAssets(language);
        }

        public static IHSRAssets CreateHSR(string language = "en")
        {
            return new HSRAssets(language);
        }
    }
}