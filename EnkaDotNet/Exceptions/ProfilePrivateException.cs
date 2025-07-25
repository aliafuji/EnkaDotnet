﻿using System;

namespace EnkaDotNet.Exceptions
{
    public class ProfilePrivateException : EnkaNetworkException
    {
        public int Uid { get; }

        public ProfilePrivateException(int uid) : base($"Profile for UID {uid} is private or character details are hidden.")
        {
            Uid = uid;
        }

        public ProfilePrivateException(int uid, string message) : base(message)
        {
            Uid = uid;
        }

        public ProfilePrivateException(int uid, string message, Exception innerException) : base(message, innerException)
        {
            Uid = uid;
        }
    }
}
