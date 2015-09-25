using System;

namespace Client.Errors
{
    public class AuthenticationError : Exception
    {
        public AuthenticationError(string message) : base(message)
        {

        }
    }
}
