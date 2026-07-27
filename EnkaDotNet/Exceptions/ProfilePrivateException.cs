using System;

namespace EnkaDotNet.Exceptions
{
    public class ProfilePrivateException : EnkaNetworkException
    {
        public long Uid { get; }

        public ProfilePrivateException(long uid) : base($"Profile for UID {uid} is private or character details are hidden.")
        {
            Uid = uid;
        }

        public ProfilePrivateException(long uid, string message) : base(message)
        {
            Uid = uid;
        }

        public ProfilePrivateException(long uid, string message, Exception innerException) : base(message, innerException)
        {
            Uid = uid;
        }
    }
}
