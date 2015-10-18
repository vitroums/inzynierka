using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class Certificate : X509Certificate2
    {
        public Certificate(string fileName) : base(fileName) { }
        public Certificate(X509Certificate certificate) : base(certificate) { }
        public Certificate(string fileName, SecureString password) : base(fileName, password) { }
        public Certificate(string fileName, string password) : base(fileName, password) { }

        public override string ToString()
        {
            return GetNameInfo(X509NameType.SimpleName, false).Split(';')[0];
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
