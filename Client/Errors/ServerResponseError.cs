using System;

namespace Client.Errors
{
    public class ServerResponseError : Exception
    {
        public ServerResponseError(string message) : base(message)
        {

        }
    }
}
