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
using Client.Errors;
using System.IO;
using Microsoft.VisualBasic;


namespace ClientApplication
{
    public partial class MainForm : Form
    {

        DropboxApi dba = new DropboxApi();
        SslClient sc;
        string GUID = "";
        string email = "";
        string login = "";
        bool Connected = false;
        bool ConnectedToGroup = false;
        
        
        public MainForm()
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

            var country = Interaction.InputBox("Podaj nazwę kraju", "Kraj", "PL");
            var state = Interaction.InputBox("Podaj nazwę wojewodztwa/regionu", "Region", "Pomerania");
            var city = Interaction.InputBox("Podaj nazwę miasta", "Miasto", "Gdansk");
            var organization = Interaction.InputBox("Podaj nazwę organizacji", "Organizacja", "PG");
            var unit = Interaction.InputBox("Podaj nazwę oddziału/jednostki", "Oddział", "ETI");
            email = Interaction.InputBox("Podaj email", "Email", "fail@example.com");
            login = Interaction.InputBox("Podaj login", "Login", "");
            if (!ValidateCertPools(country, state, city, organization, unit, email, login))
            {
                Console.WriteLine("ERROR! Niepoprawne dane do wygenerowania certyfikatu");
                return;
            }
            var certPools = country + ";" + state + ";" + city + ";" + organization + ";" + unit + ";" + login;
            try
            {
                GUID = ServerTransaction.CreateNewUser(login, email, certPools, "1234", "123456");
                MessageBox.Show("Welcome, your ID: " + GUID, "New user");
                Console.WriteLine("Connected as: \n {0}", GUID);
                Connected = true;
            }
            catch (UnknownCommadError error)
            {
                MessageBox.Show(error.Message);
            }
            catch (ServerResponseError error)
            {
                MessageBox.Show(error.Message);
            }
        }
        // new group
        private void ButtonNewGroupClick(object sender, EventArgs e)
        {
            if (GUID != "" && login != "" && email != "")
            {
                try
                {
                    var name = Interaction.InputBox("Podaj nazwę grupy", "Nazwa", "");
                    var password = Interaction.InputBox("Podaj hasło grupy", "Hasło", "");
                    ServerTransaction.CreateNewGroup(name, password, GUID, login, email);
                    MessageBox.Show("Group \"" + name + "\" created", "New group");
                }
                catch (UnknownCommadError error)
                {
                    MessageBox.Show(error.Message);
                }
                catch (ServerResponseError error)
                {
                    MessageBox.Show(error.Message);
                }
                catch (AuthenticationError error)
                {
                    MessageBox.Show(error.Message);
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
                        UpdateLists(GUID, email, login);
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
        public void UpdateLists(string guid, string email, string login)
        {
            UpdateGroupList();
            listBox2.DataSource = dba.AddUserIfNotExist(guid, email, login);
            listBox3.DataSource = dba.GetFilesList(guid);
        }
        void UpdateGroupList()
        {
            listBox1.DataSource = dba.GetGroupsNamesList();
        }

        // choose cert
        private void ButtonChooseCertClick(object sender, EventArgs e)
        {
            SslClient stream = new SslClient("127.0.0.1", 12345);
                
                if (ServerTransaction.Authenticate(stream, "9033a13a-5eb1-11e5-b3b0-f6d8a6108e1a", "mietek", "mietek@example.com"))
                {
                    Console.WriteLine("Authenticated");
                }
                else
                {
                    Console.WriteLine("Failed");
                }
            
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
