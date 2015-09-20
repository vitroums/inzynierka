using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace Client
{
    public class ServerTransaction
    {
        public static string CreateNewUser(string name, string mail, string data, string keysPassword, string rescuePassword)
        {
            string response, id;

            using (SslClient stream = new SslClient("127.0.0.1", 12345))
            {
                stream.SendString("new-user");
                response = stream.ReceiveString();
                if (response != "provide-user-name")
                {
                    CommandError();
                }
                stream.SendString(name);
                response = stream.ReceiveString();
                if (response != "provide-user-mail")
                {
                    CommandError();
                }
                stream.SendString(mail);
                response = stream.ReceiveString();
                if (response == "user-exists")
                {
                    CommandError();
                }
                else if (response != "provide-user-data")
                {
                    CommandError();
                }
                stream.SendString(data);
                response = stream.ReceiveString();
                if (response != "provide-keys-password")
                {
                    CommandError();
                }
                stream.SendString(keysPassword);
                response = stream.ReceiveString();
                if (response != "provide-rescue-password")
                {
                    CommandError();
                }
                stream.SendString(rescuePassword);
                response = stream.ReceiveString();
                if (response == "problem-while-adding-user")
                {
                    CommandError();
                }
                else if (response != "user-added")
                {
                    CommandError();
                }
                response = stream.ReceiveString();
                if (response != "certificate-file")
                {
                    CommandError();
                }
                stream.ReceiveFile("certificate.crt");
                response = stream.ReceiveString();
                if (response != "keys-file")
                {
                    CommandError();
                }
                stream.ReceiveFile("keys.crt");
                response = stream.ReceiveString();
                if (response != "user-id")
                {
                    CommandError();
                }
                id = stream.ReceiveString();
            }

            return id;
        }

        public static bool AuthenticateMyself(SslClient stream, string id, string login, string mail)
        {
            string response;
            bool result = false;

            stream.SendString("login");
            response = stream.ReceiveString();
            if (response != "provide-user-id")
            {
                CommandError();
            }
            stream.SendString(id);
            response = stream.ReceiveString();
            if (response != "provide-user-name")
            {
                CommandError();
            }
            stream.SendString(login);
            response = stream.ReceiveString();
            if (response != "provide-user-mail")
            {
                CommandError();
            }
            stream.SendString(mail);
            response = stream.ReceiveString();
            if (response == "user-doesnt-exist")
            {
                CommandError();
            }
            else if (response != "decrypt-message")
            {
                CommandError();
            }
            string chiper = stream.ReceiveString();
            string message = DecryptEncryptedData(chiper, "certificate.pem");
            stream.SendString(message);
            response = stream.ReceiveString();
            if (response == "permission-granted")
            {
                result = true;
            }
            else if (response == "premission-denied")
            {
                result = false;
            }
            else
            {
                CommandError();
            }

            return result;
        }

        public static bool CreateNewGroup(string name, string password, string id, string login, string mail)
        {
            string response;
            bool result;

            using (SslClient stream = new SslClient("127.0.0.1", 12345))
            {
                if (AuthenticateMyself(stream, id, login, mail))
                {
                    stream.SendString("new-group");
                    response = stream.ReceiveString();
                    if (response != "provide-group-name")
                    {
                        CommandError();
                    }
                    stream.SendString(name);
                    response = stream.ReceiveString();
                    if (response != "provide-group-password")
                    {
                        CommandError();
                    }
                    stream.SendString(password);
                    response = stream.ReceiveString();
                    if (response == "group-exists")
                    {
                        CommandError();
                    }
                    else if (response != "group-added")
                    {
                        CommandError();
                    }
                    result = true;
                }
                else
                {
                    // TODO Dodać rzucanie wyjątku
                    return false;
                }
            }

            return result;

        }

        private static void CommandError()
        {
            // TODO Dodać argument (nierozpoznane polecenie) i obsługę rzucania wyjątku
        }

        public static string DecryptEncryptedData(string Base64EncryptedData, string PathToPrivateKeyFile) 
        {
        X509Certificate2 myCertificate;
        try{
            myCertificate = new X509Certificate2(PathToPrivateKeyFile);
        } catch{
            throw new CryptographicException("Unable to open key file.");
        }

        RSACryptoServiceProvider rsaObj;
        if(myCertificate.HasPrivateKey) {
             rsaObj = (RSACryptoServiceProvider)myCertificate.PrivateKey;
        } else
            throw new CryptographicException("Private key not contained within certificate.");

        if(rsaObj == null)
            return string.Empty;

        byte[] decryptedBytes;
        try{
            decryptedBytes = rsaObj.Decrypt(Convert.FromBase64String(Base64EncryptedData), false);
            
        } catch {
            throw new CryptographicException("Unable to decrypt data.");
        }

    //    Check to make sure we decrpyted the string
        if(decryptedBytes.Length == 0)
            return string.Empty;
        else
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string EncryptDecryptedData(string DecryptedData, string PathToPublicKeyFile)
        {
            X509Certificate2 myCertificate;
            try
            {
                myCertificate = new X509Certificate2(PathToPublicKeyFile);
            }
            catch
            {
                throw new CryptographicException("Unable to open key file.");
            }
             RSACryptoServiceProvider myRSAProvide = new RSACryptoServiceProvider();
             byte[] bteCrypt = null;
             byte[] bteResult = null;
            try
            {
               bteCrypt = Encoding.UTF8.GetBytes(DecryptedData);
               bteResult = myRSAProvide.Encrypt(bteCrypt, false);
             }  
            catch
            {
                throw new CryptographicException("Unable to encypt data.");
            }


            //    Check to make sure we decrpyted the string
            if (bteResult.Length == 0)
                return string.Empty;
            else
                return System.Text.Encoding.UTF8.GetString(bteResult);
        } 
    }
}
