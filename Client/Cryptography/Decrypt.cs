using Client.Certificates;
using Client.Errors;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Client.Cryptography
{
    public class Decrypt
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
            RSACryptoServiceProvider rsaPrivateKey = key;
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
                    await inFs.ReadAsync(LenK, 0, 3);
                    inFs.Seek(4, SeekOrigin.Begin);
                    await inFs.ReadAsync(LenIV, 0, 3);

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
                    await inFs.ReadAsync(KeyEncrypted, 0, lenK);
                    inFs.Seek(8 + lenK, SeekOrigin.Begin);
                    await inFs.ReadAsync(IV, 0, lenIV);
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
                                    count = await inFs.ReadAsync(data, 0, blockSizeBytes);
                                    offset += count;
                                    await outStreamDecrypted.WriteAsync(data, 0, count);

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
