using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
            switch (gameType)
            {
                case GameType.Genshin: return new GenshinAssets(language);
                case GameType.ZZZ: return new ZZZAssets(language);
                case GameType.HSR: return new HSRAssets(language);
                default: throw new UnsupportedGameTypeException(gameType, $"Game type {gameType} is not supported.");
            }
            ;
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