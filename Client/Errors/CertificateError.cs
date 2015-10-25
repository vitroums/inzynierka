using System;

namespace Client.Errors
{
    class CertificateError : Exception
    {
        public CertificateError(string message) : base(message) { }
    }
}
