using EnkaDotNet.Assets.Genshin;
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
                _ => throw new UnsupportedGameTypeException(gameType, $"Game type {gameType} is not supported.")
            };
        }

        public static IGenshinAssets CreateGenshin(string language = "en")
        {
            return new GenshinAssets(language);
        }
    }
}