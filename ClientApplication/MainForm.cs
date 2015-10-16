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
using System.Security.Cryptography.X509Certificates;


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
            listBox4.DataSource = GetCertificates();
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
                            DropboxApi da = new DropboxApi();
                            da.GetFile(selectedUser.guid + ".crt");
                            var temp = Path.GetTempFileName();
                            ServerTransaction.EncryptFile(fPath, temp, selectedUser.guid + ".crt");
                            dba.UploadFile(temp, fName, selectedUser.guid);
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
            using (var newUserForm = new NewUserForm())
            {
                newUserForm.ShowDialog();
                if (newUserForm.DialogResult == DialogResult.OK)
                {
                    var newUserInfo = newUserForm.NewUserInfo;
                    try
                    {
                        GUID = ServerTransaction.CreateNewUser(newUserInfo.CommonName, newUserInfo.Email, newUserInfo.ToString(), "1234", "123456");
                        MessageBox.Show("Welcome, your ID: " + GUID, "New user");
                        Console.WriteLine("Connected as: \n {0}", GUID);
                        Connected = true;
                        UpdateGroupList();
                        textBox4.Text = login;
                        login = newUserInfo.CommonName;
                        email = newUserInfo.Email;
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
        }
        // new group
        private void ButtonNewGroupClick(object sender, EventArgs e)
        {
            if (Connected)
            {
                using (var newGroupForm = new NewGroupForm())
                {
                    newGroupForm.ShowDialog();
                    var newGroupInfo = newGroupForm.NewGroupInfo;
                    if (newGroupForm.DialogResult == DialogResult.OK)
                    {
                        if (GUID != "" && login != "" && email != "")
                        {
                            try
                            {
                                ServerTransaction.CreateNewGroup(newGroupInfo.Name, newGroupInfo.Name, GUID, login, email);
                                MessageBox.Show("Group \"" + newGroupInfo.Name + "\" created", "New group");
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
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                choosenCert = ofd.FileName;
            }
            Console.WriteLine("Choose certificate: {0}", ofd.FileName);


            //SslClient stream = new SslClient("127.0.0.1", 12345);

            //GUID = "debf63fe-6b72-11e5-9c5f-28d244eb956f";
            //email = "1234";
            //login = "12345";
            //if (ServerTransaction.Authenticate(stream, GUID, login, email))
            //    {
            //        Console.WriteLine("Authenticated");
            //        Connected = true;
            //        UpdateGroupList();
            //        textBox4.Text = login;
            //    }
            //    else
            //    {
            //        Console.WriteLine("Failed");
            //        Connected = false;
            //    }
            
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
                        ServerTransaction.DecryptFile(selectedFile, "decrypted.exe", choosenCert);
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

        public List<X509Certificate2> GetCertificates()
        {
            Console.WriteLine("Getting certs..");
            var store = new X509Store(StoreLocation.CurrentUser);
            List<X509Certificate2> validCertList = new List<X509Certificate2>();

            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates;
            foreach (var certificate in certificates)
            {
                string issuerName = certificate.GetIssuerName();
                string[] pools = issuerName.Split(',');
                foreach(string pool in pools)
                {
                    if (pool == " CN=PKI Cloud")
                    {
                        validCertList.Add(new X509Certificate2(certificate));
                        break;
                    }
                }
            }

            store.Close();

            return validCertList;
        }

        // connect button
        private void button7_Click(object sender, EventArgs e)
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
