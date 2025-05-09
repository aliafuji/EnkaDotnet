using System;

namespace EnkaDotNet.Exceptions
{
    public class EnkaClientConfigurationException : EnkaNetworkException
    {
        public EnkaClientConfigurationException()
            : base("Enka.Net client configuration error occurred.")
        {
        }

        public EnkaClientConfigurationException(string message)
            : base(message)
        {
        }

        public EnkaClientConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
