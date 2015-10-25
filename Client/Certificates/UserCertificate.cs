using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Client.Errors;
using Client.Clients;

namespace Client.Certificates
{
    public class UserCertificate : X509Certificate2
    {
        public UserCertificate(string fileName) : base(fileName)
        {
            Initialize();
        }

        public UserCertificate(X509Certificate certificate) : base(certificate)
        {
            Initialize();
        }

        public UserCertificate(string fileName, SecureString password) : base(fileName, password)
        {
            Initialize();
        }

        public UserCertificate(string fileName, string password) : base(fileName, password)
        {
            Initialize();
        }

        private void Initialize()
        {
            CheckIssuer();
            CheckSubject();
        }

        private void CheckIssuer()
        {
            var issuer = GetNameInfo(X509NameType.SimpleName, true);
            if (issuer != "PKI Cloud")
            {
                throw new UnknownCertificateError("Unsupported certificate issuer");
            }
        }

        private void CheckSubject()
        {
            try
            {
                new User(this);
            }
            catch (IndexOutOfRangeException)
            {
                throw new UnknownCertificateError("Certificate is corrupted");
            }
        }

        public override string ToString()
        {
            return GetNameInfo(X509NameType.SimpleName, false).Split(';')[0];
        }

        public string CommonName
        {
            get
            {
                return ToString();
            }
        }

        public string ID
        {
            get
            {
                return GetNameInfo(X509NameType.SimpleName, false).Split(';')[1];
            }
        }

        public string Email
        {
            get
            {
                return GetNameInfo(X509NameType.EmailName, false);
            }
        }
    }
}
