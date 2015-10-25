using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Cryptography
{
    public class Hasher
    {
        public static string HashMD5String(string message)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(message));
            StringBuilder hashBuilder = new StringBuilder();
            foreach (var _byte in hash)
            {
                hashBuilder.Append(_byte.ToString("x2"));
            }
            return hashBuilder.ToString();
        }
    }
}
