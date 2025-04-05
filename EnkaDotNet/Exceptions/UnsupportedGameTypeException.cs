using EnkaDotNet.Enums;
using System;

namespace EnkaDotNet.Exceptions
{
    public class UnsupportedGameTypeException : EnkaNetworkException
    {
        public GameType GameType { get; }

        public UnsupportedGameTypeException(GameType gameType)
            : base($"Game type {gameType} is not supported.")
        {
            GameType = gameType;
        }

        public UnsupportedGameTypeException(GameType gameType, string message)
            : base(message)
        {
            GameType = gameType;
        }

        public UnsupportedGameTypeException(GameType gameType, string message, Exception innerException)
            : base(message, innerException)
        {
            GameType = gameType;
        }
    }
}