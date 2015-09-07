﻿using System;
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
        private void ButtonNewUserClick(object sender, EventArgs e)
        {
            using (SslClient stream = new SslClient("127.0.0.1", 9999))
            {
                var country = Interaction.InputBox("Podaj nazwę kraju", "Kraj", "PL");
                var state = Interaction.InputBox("Podaj nazwę wojewodztwa/regionu", "Region", "Pomerania");
                var city = Interaction.InputBox("Podaj nazwę miasta", "Miasto", "Gdansk");
                var organization = Interaction.InputBox("Podaj nazwę organizacji", "Organizacja", "PG");
                var unit = Interaction.InputBox("Podaj nazwę oddziału/jednostki", "Oddział", "ETI");
                var email = Interaction.InputBox("Podaj email", "Email", "fail@example.com");
                var login = Interaction.InputBox("Podaj login", "Login", "");
                if (!ValidateCertPools(country, state, city, organization, unit, email, login))
                {
                    Console.WriteLine("ERROR! Niepoprawne dane do wygenerowania certyfikatu");
                    return;
                }
                var certPools = country + ";" + state + ";" + city + ";" + organization + ";" + unit + ";" + ";" + email + ";" + login;
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

                        //const string data = "PL;pomerania;Gdansk;PG;ETI;;mail@example.com;n5cccdd8546ba";
                        string nick = "nanaba";
                        stream.SendString(certPools);
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
                            UpdateGroupList();   
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
        private void ButtonNewGroupClick(object sender, EventArgs e)
        {
        using (SslClient stream = new SslClient("127.0.0.1", 9999))
            {
                var nazwaGrupy = Interaction.InputBox("Podaj nazwę grupy", "Nowa grupa", "mojagrupa");
                if (nazwaGrupy != "")
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
                            UpdateGroupList();
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
        }
        // connect to group
        private void ButtonConnectToGroupClick(object sender, EventArgs e)
        {
            if (Connected)
            {
                string selectedGroup = listBox1.SelectedItem.ToString();
                Console.WriteLine("Selected group: {0}", selectedGroup);
                string password = textBox3.Text;
                if (password != "")
                {
                    if (dba.ValidatePassword(password, selectedGroup))
                    {
                        dba = new DropboxApi(selectedGroup);
                        Console.WriteLine("Successfully connected to {0} group!", selectedGroup);
                        dba.LoadDatabase();
                        UpdateUserList();
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
        public void UpdateGroupList()
        {
            listBox1.DataSource = dba.GetGroupsNamesList();
        }
        public void UpdateUserList()
        {
            listBox2.DataSource = dba.GetUsersList();
        }
        // choose cert
        private void ButtonChooseCertClick(object sender, EventArgs e)
        {
        }

        private bool ValidateCertPools(string country, string state, string city, string organization, string unit, string email, string login)
        {
            if (country == "")
                return false;
            if (state == "")
                return false;
            if (city == "")
                return false;
            if (organization == "")
                return false;
            if (unit == "")
                return false;
            if (email == "")
                return false;
            if (login == "")
                return false;
            return true;
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
