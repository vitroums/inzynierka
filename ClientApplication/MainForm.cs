using System;
using System.ComponentModel;
using System.Windows.Forms;
using Client;
using Client.Errors;
using Client.ClientData;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace ClientApplication
{
    public partial class MainForm : Form
    {
        DropboxApi _cloud = new DropboxApi();
        UserInfo _userInformation;
        bool _connected = false;
        bool _connectedToGroup = false;
        Certificate _certificate = null;
        BindingList<Certificate> _identitiesList;
        string _certificatesPath = "D:\\Desktop\\";
        
        
        public MainForm()
        {
            InitializeComponent();
            ReloadButtonState();
            _userInformation = new UserInfo();
            _identitiesList = new BindingList<Certificate>(Certificates.GetCertificates());
            identitiesListBox.DataSource = _identitiesList;            
        }

        private void identitiesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var identitiesList = sender as ListBox;
            _certificate = identitiesList.SelectedItem as Certificate;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            _connected = ServerTransaction.Connect(_certificate);
            ReloadButtonState();
            if (_connected)
            {
                var login = _certificate.ToString();
                loginLabel.Text = login;
                var message = string.Format("Successfull logged as {0}", login);
                MessageBox.Show(message, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateGroupList();
                _userInformation.ID = _certificate.ID;
                _userInformation.Name = _certificate.ToString();
                _userInformation.Email = _certificate.Email;
            }
            else
            {
                loginLabel.Text = "Unconnected";
                MessageBox.Show("", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void newUserButton_Click(object sender, EventArgs e)
        {
            using (var newUserForm = new NewUserForm())
            {
                if (newUserForm.ShowDialog() == DialogResult.OK)
                {
                    var newUserInfo = newUserForm.NewUserInfo;
                    try
                    {
                        if (_connected)
                        {
                            ClearProperties();
                        }
                        _userInformation.ID = ServerTransaction.CreateNewUser(newUserInfo, "1234", "123456", _certificatesPath);
                        _userInformation.Name = newUserInfo.CommonName;
                        _userInformation.Email = newUserInfo.Email;
                        var newCertificatePath = _certificatesPath + _userInformation.Name + ".pfx";
                        Process addCertificateProcess = new Process();
                        addCertificateProcess.EnableRaisingEvents = false;
                        addCertificateProcess.StartInfo.FileName = newCertificatePath;
                        addCertificateProcess.Start();
                        _connected = true;
                        ReloadButtonState();
                        loginLabel.Text = _userInformation.Name;
                        var message = string.Format("Welcome {0}.\nYour ID is {1}", _userInformation.Name, _userInformation.ID);
                        MessageBox.Show(message, "New user", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateGroupList();
                        _identitiesList.Add(new Certificate(newCertificatePath));
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

        private void loadFromFileButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Certificate (.pfx)|*.pfx",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var certificate in openFileDialog.FileNames)
                {
                    _identitiesList.Add(new Certificate(certificate));
                }
            }

        }

        private void newGroupButton_Click(object sender, EventArgs e)
        {
            using (var newGroupForm = new NewGroupForm())
            {
                if (newGroupForm.ShowDialog() == DialogResult.OK)
                {
                    var newGroupInfo = newGroupForm.NewGroupInfo;
                    try
                    {
                        ServerTransaction.CreateNewGroup(newGroupInfo, _userInformation, _certificate);
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

        private void connectToGroupButton_Click(object sender, EventArgs e)
        {
            if (_connected)
            {
                using (var passwordForm = new PasswordForm())
                {
                    passwordForm.ShowDialog();
                    if (passwordForm.DialogResult == DialogResult.OK)
                    {
                        string password = passwordForm.Password;
                        string selectedGroup = groupsListBox.SelectedItem.ToString();
                        if (_cloud.ValidatePassword(password, selectedGroup))
                        {
                            _cloud = new DropboxApi(selectedGroup);
                            var message = string.Format("You are now connected to gropup \"{0}\"", selectedGroup);
                            MessageBox.Show(message, "Connection sucessfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _cloud.LoadDatabase();
                            usersListBox.DataSource = _cloud.AddUserIfNotExist(_userInformation);
                            filesListBox.DataSource = _cloud.GetFilesList(_userInformation.ID);
                            _connectedToGroup = true;
                        }
                        else
                        {
                            MessageBox.Show("Typed password is wrong", "Wrong password", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("You have to connect to server first", "Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void sendFilesButton_Click(object sender, EventArgs e)
        {
            var selectedUsers = usersListBox.SelectedItems;
            if (selectedUsers.Count > 0)
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = openFileDialog.SafeFileName;

                    foreach (UserInfo user in selectedUsers)
                    {
                        if (!encryptCheckBox.Checked)
                        {
                            _cloud.UploadFile(filePath, fileName, user.ID);
                            Console.WriteLine("Successfully uploaded: {0}", fileName);
                            if (user.Name == _userInformation.Name)
                            {
                                filesListBox.DataSource = _cloud.GetFilesList(_userInformation.ID);
                            }
                        }
                        else
                        {
                            // upload z szyfrowaniem
                            StreamReader streamReader = new StreamReader(filePath);
                            string fContent = streamReader.ReadToEnd();
                            streamReader.Close();

                            DropboxApi da = new DropboxApi();
                            da.GetFile(user.ID + ".crt");
                            var temp = Path.GetTempFileName();
                            ServerTransaction.EncryptFile(filePath, temp, user.ID + ".crt");
                            _cloud.UploadFile(temp, fileName, user.ID);
                            Console.WriteLine("Successfully uploaded");
                            if (user.Name == _userInformation.Name)
                            {
                                filesListBox.DataSource = _cloud.GetFilesList(_userInformation.ID);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Select users first", "Select users", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void downloadFilesButton_Click(object sender, EventArgs e)
        {
            string selectedFile = (string)filesListBox.SelectedItem;
            if (!encryptCheckBox.Checked)
            {
                _cloud.GetFile(selectedFile, _userInformation.ID);
                Console.WriteLine("Successfully download: {0}", selectedFile);

            }
            else
            {
                _cloud.GetFile(selectedFile, _userInformation.ID);
                Console.WriteLine("Successfully download: {0}", selectedFile);
                ServerTransaction.DecryptFile(selectedFile, "decrypted.exe", (RSACryptoServiceProvider)_certificate.PrivateKey);
                Console.WriteLine("Successfully decrypted");
            }
        }

        private void ReloadButtonState()
        {
            newGroupButton.Enabled = _connected;
            connectToGroupButton.Enabled = _connected;
            sendFilesButton.Enabled = _connected && _connectedToGroup;
            downloadFilesButton.Enabled = _connected && _connectedToGroup;
        }

        private void ClearProperties()
        {
            _cloud = new DropboxApi();
            _userInformation = new Client.ClientData.UserInfo();
            groupsListBox.DataSource = null;
            usersListBox.DataSource = null;
            filesListBox.DataSource = null;
        }

        private void UpdateGroupList()
        {
            groupsListBox.DataSource = _cloud.GetGroupsNamesList();
        }

        

       

        

        

        

        
    }
}
