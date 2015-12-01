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
                if (certificate.HasPrivateKey)
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
            }
            certificatesStore.Close();

            certificatesStore = new X509Store(StoreLocation.LocalMachine);
            certificatesStore.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificatesStore.Certificates)
            {
                if (certificate.HasPrivateKey)
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
            }
            certificatesStore.Close();


            return certificates;
        }

        public static bool CheckCaCertificate()
        {
            var certificatesStore = new X509Store(StoreName.Root ,StoreLocation.CurrentUser);
            certificatesStore.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificatesStore.Certificates)
            {
                if (certificate.GetNameInfo(X509NameType.SimpleName, false).Equals("PKI Cloud"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
