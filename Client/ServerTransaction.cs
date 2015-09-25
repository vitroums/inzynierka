using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Client.Errors;

namespace Client
{
    public class ServerTransaction
    {
        public static string CreateNewUser(string name, string mail, string data, string keysPassword, string rescuePassword)
        {
            string response, id;

            using (SslClient stream = new SslClient("192.168.0.30", 12345))
            {
                stream.SendString("new-user");
                response = stream.ReceiveString();
                if (response != "provide-user-name")
                {
                    UnknownCommandError("provide-user-name", response);
                }
                stream.SendString("user-name");
                stream.SendString(name);
                response = stream.ReceiveString();
                if (response != "provide-user-mail")
                {
                    UnknownCommandError("provide-user-mail", response);
                }
                stream.SendString("user-mail");
                stream.SendString(mail);
                response = stream.ReceiveString();
                if (response == "user-exists")
                {
                    ServerResponseError(response);
                }
                else if (response != "provide-user-data")
                {
                    UnknownCommandError("provide-user-data", response);
                }
                stream.SendString("user-data");
                stream.SendString(data);
                response = stream.ReceiveString();
                if (response != "provide-keys-password")
                {
                    UnknownCommandError("provide-keys-password", response);
                }
                stream.SendString("keys-password");
                stream.SendString(keysPassword);
                response = stream.ReceiveString();
                if (response != "provide-rescue-password")
                {
                    UnknownCommandError("provide-rescue-password", response);
                }
                stream.SendString("rescue-password");
                stream.SendString(rescuePassword);
                response = stream.ReceiveString();
                if (response == "problem-while-adding-user")
                {
                    ServerResponseError(response);
                }
                else if (response != "user-added")
                {
                    UnknownCommandError("user-added", response);
                }
                response = stream.ReceiveString();
                if (response != "certificate-file")
                {
                    UnknownCommandError("certificate-file", response);
                }
                stream.ReceiveFile("certificate.crt");
                response = stream.ReceiveString();
                if (response != "keys-file")
                {
                    UnknownCommandError("keys-file", response);
                }
                stream.ReceiveFile("keys.crt");
                response = stream.ReceiveString();
                if (response != "user-id")
                {
                    UnknownCommandError("user-id", response);
                }
                id = stream.ReceiveString();
                stream.SendString("everything-ok");
            }

            return id;
        }

        public static bool Authenticate(SslClient stream, string id, string login, string mail)
        {
            string response;
            stream.SendString("login");
            response = stream.ReceiveString();
            if (response != "provide-user-id")
            {
                UnknownCommandError("provide-user-id", response);
            }
            stream.SendString("user-id");
            stream.SendString(id);
            response = stream.ReceiveString();
            if (response != "provide-user-name")
            {
                UnknownCommandError("provide-user-name", response);
            }
            stream.SendString("user-name");
            stream.SendString(login);
            response = stream.ReceiveString();
            if (response != "provide-user-mail")
            {
                UnknownCommandError("provide-user-mail", response);
            }
            stream.SendString("user-mail");
            stream.SendString(mail);
            response = stream.ReceiveString();
            if (response == "user-doesnt-exist")
            {
                AuthenticationError(response);
            }
            else if (response != "decrypt-message")
            {
                UnknownCommandError("decrypt-message", response);
            }
            string chiper = stream.ReceiveString();
            //TODO: Dodać deszyfrowanie po implementacji na serwerze
            string message = chiper;
            stream.SendString("decrypted-message");
            stream.SendString(message);
            response = stream.ReceiveString();            
            if (response == "premission-denied")
            {
                AuthenticationError(response);
            }
            else if (response != "permission-granted")
            {
                UnknownCommandError("permission-granted", response);
            }
            return true;
        }

        public static bool CreateNewGroup(string name, string password, string id, string login, string mail)
        {
            string response;

            using (SslClient stream = new SslClient("127.0.0.1", 12345))
            {
                Authenticate(stream, id, login, mail);
                stream.SendString("new-group");
                response = stream.ReceiveString();
                if (response != "provide-group-name")
                {
                    UnknownCommandError("provide-group-name", response);
                }
                stream.SendString("group-name");
                stream.SendString(name);
                response = stream.ReceiveString();
                if (response != "provide-group-password")
                {
                    UnknownCommandError("provide-group-password", response);
                }
                stream.SendString("group-password");
                stream.SendString(password);
                response = stream.ReceiveString();
                if (response == "group-exists")
                {
                    ServerResponseError(response);
                }
                else if (response != "group-added")
                {
                    UnknownCommandError("group-added", response);
                }
            }

            return true;

        }

        public static string DecryptString(string encryptedData, string pathToPrivateKey)
        {
            X509Certificate2 certificate;
            try
            {
                certificate = new X509Certificate2(pathToPrivateKey);
            }
            catch
            {
                throw new CryptographicException("Unable to open key file.");
            }

            RSACryptoServiceProvider privateKey;
            if (certificate.HasPrivateKey)
            {
                privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
            }
            else
            {
                throw new CryptographicException("Private key not contained within certificate.");
            }


            if (privateKey == null)
            {
                return string.Empty;
            }

            byte[] decryptedBytes;
            try
            {
                decryptedBytes = privateKey.Decrypt(Convert.FromBase64String(encryptedData), false);
            }
            catch
            {
                throw new CryptographicException("Unable to decrypt data.");
            }

            if (decryptedBytes.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        public static string EncryptString(string messageToEncrypt, string pathToPublicKey)
        {
            X509Certificate2 certificate;
            try
            {
                certificate = new X509Certificate2(pathToPublicKey);
            }
            catch
            {
                throw new CryptographicException("Unable to open key file.");
            }

            RSACryptoServiceProvider myRSAProvide = new RSACryptoServiceProvider();
            byte[] resultBytes = null;
            try
            {
               var decryptedDateBytes = Encoding.UTF8.GetBytes(messageToEncrypt);
               resultBytes = myRSAProvide.Encrypt(decryptedDateBytes, false);
            }  
            catch
            {
                throw new CryptographicException("Unable to encypt data.");
            }

            if (resultBytes.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return Encoding.UTF8.GetString(resultBytes);
            }
        }

        private static void UnknownCommandError(string expected, string response)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append("Expected message: ");
            messageBuilder.Append(expected);
            messageBuilder.Append(" Received message: ");
            messageBuilder.Append(response);
            throw new UnknownCommadError(messageBuilder.ToString());
        }

        private static void ServerResponseError(string response)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append("Server response: ");
            switch (response)
            {
                case "user-exists":
                    messageBuilder.Append("User already exists");
                    break;
                case "user-doesnt-exist":
                    messageBuilder.Append("User doesn't exist");
                    break;
                case "problem-while-adding-user":
                    messageBuilder.Append("Problem while adding user");
                    break;
                case "group-exists":
                    messageBuilder.Append("Group already exists");
                    break;
            }
            throw new ServerResponseError(messageBuilder.ToString());
        }

        private static void AuthenticationError(string response)
        {
            StringBuilder messageBuilder = new StringBuilder();
            switch (response)
            {
                case "user-doesnt-exist":
                    messageBuilder.Append("User doesn't exists");
                    break;
                case "premission-denied":
                    messageBuilder.Append("Wrong credentials");
                    break;
            }
            throw new AuthenticationError(messageBuilder.ToString());
        }
    }
}
