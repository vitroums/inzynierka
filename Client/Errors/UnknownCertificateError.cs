using System;

namespace Client.Errors
{
    public class UnknownCertificateError : Exception
    {
        public UnknownCertificateError(string message) : base(message)
        {

        }
    }
}
