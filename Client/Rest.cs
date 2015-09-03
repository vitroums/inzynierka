using System;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class Rest
    {
        public X509Store getCertificates()
        {
            Console.WriteLine("Getting certs..");
            var store = new X509Store(StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates;
            foreach (var certificate in certificates)
            {
                var friendlyName = certificate.FriendlyName;
                Console.WriteLine(friendlyName);
            }

            store.Close();

            return store;
        }
    }
}
