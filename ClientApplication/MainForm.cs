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
        string choosenCert = "certificate.pfx";
        
        
        public MainForm()
        {
            InitializeComponent();
            // output do textBox'a i konsoli
            Console.SetOut(new MultiTextWriter(new ControlWriter(textBox1), Console.Out));
            // output tylko do textbox'a       
            //Console.SetOut(new ControlWriter(textBox1));   
            
        }
        // send file to selected user
        private void button1_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                if(ConnectedToGroup)
                {
                    User selectedUser = (User)listBox2.SelectedItem;
                    OpenFileDialog ofd = new OpenFileDialog();
                    DialogResult result = ofd.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string fPath = ofd.FileName;
                        string fName = ofd.SafeFileName;
                        if (!checkBox1.Checked)
                        {
                            dba.UploadFile(fPath, fName, selectedUser.guid);
                            Console.WriteLine("Successfully uploaded: {0}", fName);
                            if (selectedUser.nick == login)
                            {
                                listBox3.DataSource = dba.GetFilesList(GUID);
                            }
                        }
                        else
                        {
                            // upload z szyfrowaniem
                            StreamReader streamReader = new StreamReader(fPath);
                            string fContent = streamReader.ReadToEnd();
                            streamReader.Close();

                            // TODO: pobranie publicznego selectedUser'a
                            ServerTransaction.EncryptString(fContent, "selectedUser_publicKey");
                            dba.UploadFile(fPath, fName, selectedUser.guid);
                            Console.WriteLine("Successfully uploaded");
                        }
                                     
                    }
                    
                }
                else
                {
                    Console.WriteLine("ERROR! Not connected to the group");
                }
                
            }
            else
            {
                Console.WriteLine("ERROR! Not connected to the server");
            }
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
                UpdateGroupList();
                textBox4.Text = login;
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
            if (Connected)
            {
                if (GUID != "" && login != "" && email != "")
                {
                    try
                    {
                        var name = Interaction.InputBox("Podaj nazwę grupy", "Nazwa", "");
                        var password = Interaction.InputBox("Podaj hasło grupy", "Hasło", "");
                        ServerTransaction.CreateNewGroup(name, password, GUID, login, email);
                        MessageBox.Show("Group \"" + name + "\" created", "New group");
                        UpdateGroupList();
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
            else
            {
                Console.WriteLine("ERROR! Not connected to the server");
            }
            
        }
        // connect to group
        private void ButtonConnectToGroupClick(object sender, EventArgs e)
        {
            if (Connected)
            {
                List<string> s = new List<string>();
                listBox2.DataSource = s;
                listBox3.DataSource = s;
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
                        listBox2.DataSource = dba.AddUserIfNotExist(GUID, email, login);
                        listBox3.DataSource = dba.GetFilesList(GUID);
                        ConnectedToGroup = true;
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

        void UpdateGroupList()
        {
            listBox1.DataSource = dba.GetGroupsNamesList();
        }

        // choose cert
        private void ButtonChooseCertClick(object sender, EventArgs e)
        {
            SslClient stream = new SslClient("127.0.0.1", 12345);
            GUID = "110d98ac-69b5-11e5-aeca-f6d8a6108e1a";
            email = "wert@example.com";
            login = "wert";
            if (ServerTransaction.Authenticate(stream, "110d98ac-69b5-11e5-aeca-f6d8a6108e1a", "wert", "wert@example.com"))
                {
                    Console.WriteLine("Authenticated");
                    Connected = true;
                    UpdateGroupList();
                    textBox4.Text = login;
                }
                else
                {
                    Console.WriteLine("Failed");
                    Connected = false;
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

        // download selected file
        private void button5_Click(object sender, EventArgs e)
        {
            if (Connected)
            {
                if (ConnectedToGroup)
                {
                    string selectedFile = (string)listBox3.SelectedItem;
                    if (!checkBox1.Checked)
                    {
                        dba.GetFile(selectedFile, GUID);
                        Console.WriteLine("Successfully download: {0}", selectedFile);

                    }
                    else
                    {
                        // download z deszyfrowaniem
                        dba.GetFile(selectedFile, GUID);
                        Console.WriteLine("Successfully download: {0}", selectedFile);
                        StreamReader streamReader = new StreamReader(selectedFile);
                        string fContent = streamReader.ReadToEnd();
                        streamReader.Close();
                        string decrypted = ServerTransaction.DecryptString(fContent, choosenCert);
                        System.IO.File.WriteAllText("dec"+selectedFile, decrypted);
                        Console.WriteLine("Successfully decrypted");
                    }

                }
                else
                {
                    Console.WriteLine("ERROR! Not connected to the group");
                }

            }
            else
            {
                Console.WriteLine("ERROR! Not connected to the server");
            }
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
