using Client.Errors;
using Client.Clients;
using Client.Groups;
using Client.Certificates;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    public class ServerTransaction
    {
        private static Configuration _configuration = Configuration.Instance;

        public async static Task<bool> CreateNewUser(CertificateInfo certificateInformation, string outputPath)
        {
            string response;

            using (SslClient stream = new SslClient(_configuration.IP, _configuration.Port))
            {
                await stream.SendStringAsync("new-user");
                response = await stream.ReceiveStringAsync();
                if (response != "provide-user-name")
                {
                    UnknownCommandError("provide-user-name", response);
                }
                await stream.SendStringAsync("user-name");
                await stream.SendStringAsync(certificateInformation.CommonName);
                response = await stream.ReceiveStringAsync();
                if (response != "provide-user-mail")
                {
                    UnknownCommandError("provide-user-mail", response);
                }
                await stream.SendStringAsync("user-mail");
                await stream.SendStringAsync(certificateInformation.Email);
                response = await stream.ReceiveStringAsync();
                if (response == "user-exists")
                {
                    ServerResponseError(response);
                }
                else if (response != "provide-user-data")
                {
                    UnknownCommandError("provide-user-data", response);
                }
                await stream.SendStringAsync("user-data");
                await stream.SendStringAsync(certificateInformation.ToString());
                response = await stream.ReceiveStringAsync();
                if (response == "problem-while-adding-user")
                {
                    ServerResponseError(response);
                }
                else if (response != "user-added")
                {
                    UnknownCommandError("user-added", response);
                }
                response = await stream.ReceiveStringAsync();
                if (response != "certificate-file")
                {
                    UnknownCommandError("certificate-file", response);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                await stream.ReceiveFileAsync(outputPath);
                await stream.SendStringAsync("everything-ok");
            }

            return true;
        }

        private async static Task<bool> Authenticate(SslClient stream, User user)
        {
            string response;
            response = await stream.ReceiveStringAsync();
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

        public async static Task<bool> CreateNewGroup(Group group, UserCertificate certificate)
        {
            string response;

            using (SslClient stream = new SslClient(_configuration.IP, _configuration.Port, certificate))
            {
                await stream.SendStringAsync("login");
                await Authenticate(stream, new User(certificate));
                await stream.SendStringAsync("new-group");
                response = await stream.ReceiveStringAsync();
                if (response != "provide-group-name")
                {
                    UnknownCommandError("provide-group-name", response);
                }
                await stream.SendStringAsync("group-name");
                await stream.SendStringAsync(group.Name);
                response = await stream.ReceiveStringAsync();
                if (response != "provide-group-password")
                {
                    UnknownCommandError("provide-group-password", response);
                }
                await stream.SendStringAsync("group-password");
                await stream.SendStringAsync(group.Password);
                response = await stream.ReceiveStringAsync();
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

        public async static Task<bool> Connect(UserCertificate certificate)
        {
            try
            {
                using (SslClient stream = new SslClient(_configuration.IP, _configuration.Port, certificate))
                {
                    await stream.SendStringAsync("connect");
                    await Authenticate(stream, new User(certificate));
                }
            }
            catch (AuthenticationError error)
            {
                return false;
            }
            return true;
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
