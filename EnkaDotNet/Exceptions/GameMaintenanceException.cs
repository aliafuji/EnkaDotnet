using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Exceptions
{
    public class GameMaintenanceException : Exception
    {
        public GameMaintenanceException() { }
        public GameMaintenanceException(string message) : base(message) { }
        public GameMaintenanceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
