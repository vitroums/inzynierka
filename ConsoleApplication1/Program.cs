using Client;
using System;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string login = "test-user6";
            string mail = "email@example.com";
            string id = ServerTransaction.CreateNewUser(login, mail, "PL;Pomerania;Gdansk;PG;ETI;ostojan", "haslodoklucza", "hasloodzyskiwania");
            ServerTransaction.CreateNewGroup("Grupa3", "tajnehaslogrupowe", id, login, mail);
            Console.Read();
        }
    }
}
