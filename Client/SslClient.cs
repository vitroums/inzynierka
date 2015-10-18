using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Client.Errors;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Client
{
    public class SslClient : TcpClient
    {

        private readonly int _bufferSize;
        private readonly X509Certificate2 _certificate;
        private readonly SslStream _stream;

        public SslClient(string ip, int port, Certificate certificate = null, int bufferSize = 1024) : base(ip, port)
        {
            try
            {
                _bufferSize = bufferSize;
                if (certificate != null)
                {
                    _certificate = certificate;
                }
                else
                {
                    _certificate = new X509Certificate2();
                }
                _stream = new SslStream(GetStream(), false, ServerCertificateValidation, UserCertificateSelection);
                _stream.AuthenticateAsClient(ip);
            }
            catch (AuthenticationException)
            {
                throw new AuthenticationError("Problem with certificate");
            }
        }
 
        #region Certificates Proccess
        private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //TODO: Trzeba będzie dodać porównywanie certificate z certyfikatem CA
            return true;
        }

        private X509Certificate UserCertificateSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _certificate;
        }
        #endregion

        public void SendString(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            SendStreamSize(message.Length);
             _stream.Write(buffer, 0, buffer.Length);
        }

        public void SendFile(string path)
        {
            var buffer = new byte[_bufferSize];           
            using (FileStream file = File.OpenRead(path))
            using (BinaryReader reader = new BinaryReader(file))
            {
                SendStreamSize(file.Length);
                var bytesRead = reader.Read(buffer, 0, _bufferSize);
                while (bytesRead != 0)
                {
                     _stream.Write(buffer, 0, bytesRead);
                    bytesRead = reader.Read(buffer, 0, _bufferSize);
                }
            }
        }

        public string ReceiveString()
        {
            var buffer = new byte[_bufferSize];
            long streamSize =  ReceiveStreamSize();
            long allBytes = 0;
            StringBuilder message = new StringBuilder();
            while (allBytes < streamSize)
            {
                var bytesRead =  _stream.Read(buffer, 0, _bufferSize);
                message.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
                allBytes += bytesRead;
            }
            return message.ToString();
        }

        public void ReceiveFile(string path)
        {
            var buffer = new byte[_bufferSize];
            long streamSize = ReceiveStreamSize();
            long allBytes = 0;
            using (FileStream file = File.OpenWrite(path))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                while (allBytes < streamSize)
                {
                    var bytesRead = _stream.Read(buffer, 0, _bufferSize);
                    writer.Write(buffer, 0, bytesRead);
                    allBytes += bytesRead;
                }
            }
        }


        #region Steram Size Proccess
        private void SendStreamSize(long size)
        {
            var buffer = Encoding.UTF8.GetBytes(Convert.ToString(size, 2).PadLeft(64, '0'));
             _stream.Write(buffer, 0, 64);
        }

        private long ReceiveStreamSize()
        {
            var buffer = new byte[64];
             _stream.Read(buffer, 0, 64);
            return Convert.ToInt64(System.Text.Encoding.UTF8.GetString(buffer, 0, 64), 2);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
        }
    }
}
