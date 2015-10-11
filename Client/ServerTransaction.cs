using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Client.Errors;
using System.IO;

namespace Client
{
    public class ServerTransaction
    {
        private static string ip = "127.0.0.1";
        private static int bufferSize = 128;

        public static string CreateNewUser(string name, string mail, string data, string keysPassword, string rescuePassword)
        {
            string response, id;

            using (SslClient stream = new SslClient(ip, 12345))
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
                stream.ReceiveFile("certificate.pfx");
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
            else if (response == "premission-denied")
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

            using (SslClient stream = new SslClient(ip, 12345))
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
            RSACryptoServiceProvider myRSAProvide = (RSACryptoServiceProvider)certificate.PublicKey.Key;
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

        public static void DecryptFile(string inFile, string outFile, string rsaPrivateKeyPath)
        {
            RSACryptoServiceProvider rsaPrivateKey = (RSACryptoServiceProvider)new X509Certificate2(rsaPrivateKeyPath).PrivateKey;
            // Create instance of AesManaged for
            // symetric decryption of the data.
            using (AesManaged aesManaged = new AesManaged())
            {
                aesManaged.KeySize = 256;
                aesManaged.BlockSize = 128;
                aesManaged.Mode = CipherMode.CBC;

                // Create byte arrays to get the length of
                // the encrypted key and IV.
                // These values were stored as 4 bytes each
                // at the beginning of the encrypted package.
                byte[] LenK = new byte[4];
                byte[] LenIV = new byte[4];

                // Consruct the file name for the decrypted file.

                // Use FileStream objects to read the encrypted
                // file (inFs) and save the decrypted file (outFs).
                using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                {

                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Read(LenK, 0, 3);
                    inFs.Seek(4, SeekOrigin.Begin);
                    inFs.Read(LenIV, 0, 3);

                    // Convert the lengths to integer values.
                    int lenK = BitConverter.ToInt32(LenK, 0);
                    int lenIV = BitConverter.ToInt32(LenIV, 0);

                    // Determine the start postition of
                    // the ciphter text (startC)
                    // and its length(lenC).
                    int startC = lenK + lenIV + 8;
                    int lenC = (int)inFs.Length - startC;

                    // Create the byte arrays for
                    // the encrypted AesManaged key,
                    // the IV, and the cipher text.
                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    // Extract the key and IV
                    // starting from index 8
                    // after the length values.
                    inFs.Seek(8, SeekOrigin.Begin);
                    inFs.Read(KeyEncrypted, 0, lenK);
                    inFs.Seek(8 + lenK, SeekOrigin.Begin);
                    inFs.Read(IV, 0, lenIV);
                    // Use RSACryptoServiceProvider
                    // to decrypt the AesManaged key.
                    byte[] KeyDecrypted = rsaPrivateKey.Decrypt(KeyEncrypted, false);

                    // Decrypt the key.
                    using (ICryptoTransform transform = aesManaged.CreateDecryptor(KeyDecrypted, IV))
                    {

                        // Decrypt the cipher text from
                        // from the FileSteam of the encrypted
                        // file (inFs) into the FileStream
                        // for the decrypted file (outFs).
                        using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                        {

                            int count = 0;
                            int offset = 0;

                            int blockSizeBytes = aesManaged.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];

                            // By decrypting a chunk a time,
                            // you can save memory and
                            // accommodate large files.

                            // Start at the beginning
                            // of the cipher text.
                            inFs.Seek(startC, SeekOrigin.Begin);
                            using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    offset += count;
                                    outStreamDecrypted.Write(data, 0, count);

                                }
                                while (count > 0);

                                outStreamDecrypted.FlushFinalBlock();
                                outStreamDecrypted.Close();
                            }
                            outFs.Close();
                        }
                        inFs.Close();
                    }

                }

            }
        }

        public static void EncryptFile(string inFile, string outFile, string rsaPublicKeyPath)
        {
            RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)new X509Certificate2(rsaPublicKeyPath).PublicKey.Key;
            using (AesManaged aesManaged = new AesManaged())
            {
                // Create instance of AesManaged for
                // symetric encryption of the data.
                aesManaged.KeySize = 256;
                aesManaged.BlockSize = 128;
                aesManaged.Mode = CipherMode.CBC;
                using (ICryptoTransform transform = aesManaged.CreateEncryptor())
                {
                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(rsaPublicKey);
                    byte[] keyEncrypted = keyFormatter.CreateKeyExchange(aesManaged.Key, aesManaged.GetType());

                    // Create byte arrays to contain
                    // the length values of the key and IV.
                    byte[] LenK = new byte[4];
                    byte[] LenIV = new byte[4];

                    int lKey = keyEncrypted.Length;
                    LenK = BitConverter.GetBytes(lKey);
                    int lIV = aesManaged.IV.Length;
                    LenIV = BitConverter.GetBytes(lIV);

                    // Write the following to the FileStream
                    // for the encrypted file (outFs):
                    // - length of the key
                    // - length of the IV
                    // - ecrypted key
                    // - the IV
                    // - the encrypted cipher content
                    using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                    {

                        outFs.Write(LenK, 0, 4);
                        outFs.Write(LenIV, 0, 4);
                        outFs.Write(keyEncrypted, 0, lKey);
                        outFs.Write(aesManaged.IV, 0, lIV);

                        // Now write the cipher text using
                        // a CryptoStream for encrypting.
                        using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                        {

                            // By encrypting a chunk at
                            // a time, you can save memory
                            // and accommodate large files.
                            int count = 0;
                            int offset = 0;

                            // blockSizeBytes can be any arbitrary size.
                            int blockSizeBytes = aesManaged.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];
                            int bytesRead = 0;

                            using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    offset += count;
                                    outStreamEncrypted.Write(data, 0, count);
                                    bytesRead += blockSizeBytes;
                                }
                                while (count > 0);
                                inFs.Close();
                            }
                            outStreamEncrypted.FlushFinalBlock();
                            outStreamEncrypted.Close();
                        }
                        outFs.Close();
                    }
                }
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
