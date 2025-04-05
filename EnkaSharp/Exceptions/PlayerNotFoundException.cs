﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnkaSharp.Exceptions
{
    public class PlayerNotFoundException : EnkaNetworkException
    {
        public int Uid { get; }

        public PlayerNotFoundException(int uid) : base($"Player profile for UID {uid} not found.")
        {
            Uid = uid;
        }

        public PlayerNotFoundException(int uid, string message) : base(message)
        {
            Uid = uid;
        }

        public PlayerNotFoundException(int uid, string message, Exception innerException) : base(message, innerException)
        {
            Uid = uid;
        }
    }
}
