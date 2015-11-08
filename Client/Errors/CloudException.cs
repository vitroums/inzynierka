using System;

namespace Client.Errors
{
    public class CloudException : Exception
    {
        public CloudException(string message) : base(message) { }
    }
}
