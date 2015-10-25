using System;

namespace Client.Errors
{
    class UnknownCertificateError : Exception
    {
        public UnknownCertificateError(string message) : base(message)
        {

        }
    }
}
