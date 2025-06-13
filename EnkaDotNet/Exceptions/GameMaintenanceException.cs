using System;

namespace EnkaDotNet.Exceptions
{
    public class GameMaintenanceException : Exception
    {
        public GameMaintenanceException() { }
        public GameMaintenanceException(string message) : base(message) { }
        public GameMaintenanceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
