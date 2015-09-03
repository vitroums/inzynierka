using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using System.IO;
using Microsoft.VisualBasic;


namespace ClientApplication
{
    public partial class Form1 : Form
    {

        DropboxApi dba = new DropboxApi();
        SslClient sc;
        string GUID = "";
        bool Connected = false;
        bool ConnectedToGroup = false;
        
        
        public Form1()
        {
            InitializeComponent();
            // output do textBox'a i konsoli
            Console.SetOut(new MultiTextWriter(new ControlWriter(textBox1), Console.Out));
            // output tylko do textbox'a       
            //Console.SetOut(new ControlWriter(textBox1));


            
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        // new user
        private void button3_Click(object sender, EventArgs e)
        {
            using (SslClient stream = new SslClient("127.0.0.1", 9999))
            {
                stream.SendString("new-user");
                // client -> server "new-user"
                string response = stream.ReceiveString();
                // client <- server "new-user-type"
                if (response == "new-user-type")
                {
                    stream.SendString("remote");
                    // client -> server "remote"
                    response = stream.ReceiveString();
                    // client <- server "provide-data"
                    if (response == "provide-data")
                    {
                        // country;state;city;organization;unit;name;email;login

                        const string data = "PL;pomerania;Gdansk;PG;ETI;;mail@example.com;n5cccdd8546ba";
                        string nick = "nanaba";
                        stream.SendString(data);
                        // client -> server data
                        response = stream.ReceiveString();
                        // client <- server guid
                        if (response != "user-exists")
                        {
                            string guid = response;
                            stream.ReceiveFile("cert2.pem");
                            // client <- server certificate
                            stream.ReceiveFile("key2.pem");
                            // client <- server key
                            stream.SendString("done");
                            // client -> server "done"

                            Console.WriteLine("Registered new user:");
                            Console.WriteLine("GUID: " + guid);
                            Console.WriteLine("Nick: " + nick);
                            GUID = guid;
                            textBox2.Text = Path.GetFullPath("cert2.pem");
                            Connected = true;
                            updateGroupList();   
                        }
                        // client <- server "user-exists"
                        else
                        {
                            Console.WriteLine("User already exists!");
                        }
                    }
                    // client <- server "unknown-command"
                    else
                    {
                        Console.WriteLine("Unknown command!");
                    }

                }
                // client <- server "unknown-command"
                else
                {
                    Console.WriteLine("Unknown command!");
                }
            }
        }
        // new group
        private void button6_Click(object sender, EventArgs e)
        {
        using (SslClient stream = new SslClient("127.0.0.1", 9999))
            {
                stream.SendString("new-group");
                // client -> server "new-user"
                string response = stream.ReceiveString();
                // client <- server "new-user-type"
                
                    if (response == "provide-data")
                    {
                        // country;state;city;organization;unit;name;email;login
                        var x = Interaction.InputBox("Podaj nazwę grupy", "Nowa grupa", "mojagrupa");
                        String password = Guid.NewGuid().ToString();
                        const string data = "PL;pomerania;Gdansk;PG;ETI;;mail@example.com;n5cccdd8546ba";
                        string nick = "nanaba";
                        stream.SendString(data);
                        // client -> server data
                        response = stream.ReceiveString();
                        // client <- server guid
                        if (response != "user-exists")
                        {
                            string guid = response;
                            stream.ReceiveFile("cert2.pem");
                            // client <- server certificate
                            stream.ReceiveFile("key2.pem");
                            // client <- server key
                            stream.SendString("done");
                            // client -> server "done"

                            Console.WriteLine("Registered new user:");
                            Console.WriteLine("GUID: " + guid);
                            Console.WriteLine("Nick: " + nick);
                            GUID = guid;
                            textBox2.Text = Path.GetFullPath("cert2.pem");
                            Connected = true;
                            updateGroupList();   
                        }
                        // client <- server "user-exists"
                        else
                        {
                            Console.WriteLine("User already exists!");
                        }
                    }
                    // client <- server "unknown-command"
                    else
                    {
                        Console.WriteLine("Unknown command!");
                    }

            }
        }
        // connect to group
        private void button2_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                string selectedGroup = listBox1.SelectedItem.ToString();
                Console.WriteLine("Selected group: {0}", selectedGroup);
                string password = textBox3.Text;
                if (password != "")
                {
                    if (dba.validatePassword(password, selectedGroup))
                    {
                        dba = new DropboxApi(selectedGroup);
                        Console.WriteLine("Successfully connected to {0} group!", selectedGroup);
                        dba.loadDB();
                        updateUserList();
                    }
                    else
                    {
                        Console.WriteLine("Wrong password!");
                    }
                }
                else
                {
                    Console.WriteLine("Type a password!");
                }
            }
            else
            {
                Console.WriteLine("You have to connect to the server first!");
            }

        }
        public void updateGroupList()
        {
            listBox1.DataSource = dba.getGroupsNamesList();
        }
        public void updateUserList()
        {
            listBox2.DataSource = dba.getUsersList();
        }
        // choose cert
        private void button4_Click(object sender, EventArgs e)
        {
        }

    }



    public class ControlWriter : TextWriter
    {
        private Control textbox;
        public ControlWriter(Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
    public class MultiTextWriter : TextWriter
    {
        private IEnumerable<TextWriter> writers;
        public MultiTextWriter(IEnumerable<TextWriter> writers)
        {
            this.writers = writers.ToList();
        }
        public MultiTextWriter(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in writers)
                writer.Close();
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
