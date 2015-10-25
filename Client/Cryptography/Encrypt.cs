using Client.Certificates;
using Client.Errors;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Client.Cryptography
{
    public class Encrypt
    {

        public async static Task FileWithPublicKey(string inputFile, string outputFile, UserCertificate certificate)
        {
            await File(inputFile, outputFile, certificate.PublicKey.Key as RSACryptoServiceProvider);
        }

        public async static Task FileWithPrivateKey(string inputFile, string outputFile, UserCertificate certificate)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new CertificateError("Certificate doesn't have private key.");
            }
            await File(inputFile, outputFile, certificate.PrivateKey as RSACryptoServiceProvider);
        }

        private async static Task File(string inFile, string outFile, RSACryptoServiceProvider key)
        {
            using (AesManaged aesManaged = new AesManaged())
            {
                // Create instance of AesManaged for
                // symetric encryption of the data.
                aesManaged.KeySize = 256;
                aesManaged.BlockSize = 128;
                aesManaged.Mode = CipherMode.CBC;
                using (ICryptoTransform transform = aesManaged.CreateEncryptor())
                {
                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(key);
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

                        await outFs.WriteAsync(LenK, 0, 4);
                        await outFs.WriteAsync(LenIV, 0, 4);
                        await outFs.WriteAsync(keyEncrypted, 0, lKey);
                        await outFs.WriteAsync(aesManaged.IV, 0, lIV);

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
                                    count = await inFs.ReadAsync(data, 0, blockSizeBytes);
                                    offset += count;
                                    await outStreamEncrypted.WriteAsync(data, 0, count);
                                    bytesRead += blockSizeBytes;
                                }
                                while (count > 0);
                            }
                        }
                    }
                }
            }
        }
    }
}
