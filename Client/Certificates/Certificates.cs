using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Client.Errors;

namespace Client.Certificates
{
    public class CertificatesProcess
    {
        public static List<UserCertificate> GetCertificates()
        {
            var certificates = new List<UserCertificate>();

            var certificatesStore = new X509Store(StoreLocation.CurrentUser);
            certificatesStore.Open(OpenFlags.ReadOnly);

            foreach (var certificate in certificatesStore.Certificates)
            {
                try
                {
                    certificates.Add(new UserCertificate(certificate));
                }
                catch (UnknownCertificateError)
                {
                    ;
                }
            }
            certificatesStore.Close();

            certificatesStore = new X509Store(StoreLocation.LocalMachine);
            certificatesStore.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificatesStore.Certificates)
            {
                try
                {
                    certificates.Add(new UserCertificate(certificate));
                }
                catch (UnknownCertificateError)
                {
                    ;
                }
            }
            certificatesStore.Close();


            return certificates;
        }
    }
}
