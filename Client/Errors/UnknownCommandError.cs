using System;

namespace Client.Errors
{
    public class UnknownCommadError : Exception
    {
        public UnknownCommadError(string message) : base(message)
        {

        }
    }
}
