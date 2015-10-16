using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class Certificates
    {
        public static List<Certificate> GetCertificates()
        {
            var certificates = new List<Certificate>();

            var certificatesStore = new X509Store(StoreLocation.LocalMachine);
            certificatesStore.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificatesStore.Certificates)
            {
                certificates.Add(new Certificate(certificate));
            }
            certificatesStore.Close();


            return certificates;
        }
    }
}
