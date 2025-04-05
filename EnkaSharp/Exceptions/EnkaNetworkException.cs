namespace EnkaSharp.Exceptions
{
    public class EnkaNetworkException : Exception
    {
        public EnkaNetworkException() { }
        public EnkaNetworkException(string message) : base(message) { }
        public EnkaNetworkException(string message, Exception innerException) : base(message, innerException) { }
    }
}
