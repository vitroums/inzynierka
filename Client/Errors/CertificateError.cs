using System;

namespace Client.Errors
{
    public class CertificateError : Exception
    {
        public CertificateError(string message) : base(message) { }
    }
}
