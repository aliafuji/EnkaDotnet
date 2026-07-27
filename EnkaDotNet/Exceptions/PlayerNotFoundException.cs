using System;

namespace EnkaDotNet.Exceptions
{
    public class PlayerNotFoundException : EnkaNetworkException
    {
        public long Uid { get; }

        public PlayerNotFoundException(long uid) : base($"Player profile for UID {uid} not found.")
        {
            Uid = uid;
        }

        public PlayerNotFoundException(long uid, string message) : base(message)
        {
            Uid = uid;
        }

        public PlayerNotFoundException(long uid, string message, Exception innerException) : base(message, innerException)
        {
            Uid = uid;
        }
    }
}
