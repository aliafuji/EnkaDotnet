using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Exceptions
{
    public class EnkaNetworkException : Exception
    {
        public EnkaNetworkException() { }
        public EnkaNetworkException(string message) : base(message) { }
        public EnkaNetworkException(string message, Exception innerException) : base(message, innerException) { }
    }
}
