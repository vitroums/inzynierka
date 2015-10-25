using System;

namespace Client.Errors
{
    class CloudException : Exception
    {
        public CloudException(string message) : base(message) { }
    }
}
