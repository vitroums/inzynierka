using System;
using Client;
using System.Collections.Generic;
namespace ClientApplicationTest
{
    public class Application
    {
        public static void Main()
        {
            var client = new DropboxApi();
            
            
            //client.UploadFile("asd.txt", "sometrickygu1d");
            var sl = client.GetGroupsNamesList();
            Console.ReadKey();
            //using (SslClient stream = new SslClient("127.0.0.1", 9999))
            //{
            //    stream.SendString("new-user");
            //    // client -> server "new-user"
            //    string response = stream.ReceiveString();
            //    // client <- server "new-user-type"
            //    if (response == "new-user-type")
            //    {
            //        stream.SendString("remote");
            //        // client -> server "remote"
            //        response = stream.ReceiveString();
            //        // client <- server "provide-data"
            //        if (response == "provide-data")
            //        {
            //            // country;state;city;organization;unit;name;email;login
            //            const string data = "PL;pomerania;Gdansk;PG;ETI;;mail@example.com;login";
            //            stream.SendString(data);
            //            // client -> server data
            //            response = stream.ReceiveString();
            //            // client <- server guid
            //            if (response != "user-exists")
            //            {
            //                string guid = response;
            //                stream.ReceiveFile("cert2.pem");
            //                // client <- server certificate
            //                stream.ReceiveFile("key2.pem");
            //                // client <- server key
            //                stream.SendString("done");
            //                // client -> server "done"

            //                Console.WriteLine("Everything works fine!");
            //                Console.WriteLine("GUID: " + guid);
            //                Console.WriteLine("Certificate:");
            //                Console.WriteLine(System.IO.File.ReadAllText("cert2.pem"));
            //                Console.WriteLine("Key:");
            //                Console.WriteLine(System.IO.File.ReadAllText("key2.pem"));
            //            }
            //            // client <- server "user-exists"
            //            else
            //            {
            //                Console.WriteLine("User already exists!");
            //            }
            //        }
            //        // client <- server "unknown-command"
            //        else
            //        {
            //            Console.WriteLine("Unknown command!");
            //        }

            //    }
            //    // client <- server "unknown-command"
            //    else
            //    {
            //        Console.WriteLine("Unknown command!");
            //    }
            //}
            //Console.Write("Press any key to continue...");
            //Console.Read();
        }
    }
}