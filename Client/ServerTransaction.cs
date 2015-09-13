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

        private static bool AuthenticateMyself(SslClient stream, string id, string login, string mail)
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
            // TODO Odszyfrowywanie wiadomości
            string message = chiper;
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
    }
}
