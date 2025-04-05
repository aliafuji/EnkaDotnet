using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnkaSharp.Exceptions
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
