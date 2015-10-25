using Client;
using Client.Errors;
using Client.Clients;
using Client.Certificates;
using Client.Cloud;
using Client.Groups;
using Client.Cryptography;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClientApplication
{
    public partial class MainForm : Form
    {
        Dropbox _cloud = null;
        User _currentUser = null;
        bool _connected = false;
        bool _connectedToGroup = false;
        UserCertificate _certificate = null;
        BindingList<UserCertificate> _identitiesList = new BindingList<UserCertificate>(CertificatesProcess.GetCertificates());
        
        public MainForm()
        {
            InitializeComponent();
            identitiesListBox.DataSource = _identitiesList;
            if (_identitiesList.Count == 0)
            {
                connectButton.Enabled = false;
            }
            ReloadButtonState();
            DisableGroups();
            DisableUsers();
            DisableFiles();         
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Connecting");
            statusBar.Items.Add(statusLabel);
            if (_connected)
                DisconnectUser();

            _certificate = identitiesListBox.SelectedItem as UserCertificate;
            if (await ServerTransaction.Connect(_certificate))
            {
                await ConnectUser();

                var login = _certificate.CommonName;
                loginLabel.Text = login;

                var message = string.Format("Successfull logged as {0}", login);
                MessageBox.Show(message, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _currentUser = _certificate;
            }
            else
            {
                MessageBox.Show("Faild to authenticate", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            EnableIdentity();
            if (_connected)
                EnableGroups();
            statusBar.Items.Remove(statusLabel);
        }

        private async void newUserButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Creating new user");
            statusBar.Items.Add(statusLabel);

            using (var newUserForm = new NewUserForm())
            {
                if (newUserForm.ShowDialog() == DialogResult.OK)
                {
                    var newUserInfo = newUserForm.NewUserInfo;
                    var pathToSave = newUserForm.PathToSave;
                    try
                    {
                        DisconnectUser();
                        await ServerTransaction.CreateNewUser(newUserInfo, pathToSave);

                        _certificate = new UserCertificate(pathToSave);
                        _currentUser = _certificate;

                        Process addCertificateProcess = new Process();
                        addCertificateProcess.EnableRaisingEvents = false;
                        addCertificateProcess.StartInfo.FileName = pathToSave;
                        addCertificateProcess.Start();

                        loginLabel.Text = _currentUser.CommonName;

                        var message = string.Format("Welcome {0}.", _currentUser.CommonName);
                        MessageBox.Show(message, "New user", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        _identitiesList.Add(_certificate);

                        await ConnectUser();
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
            EnableIdentity();
            if (_connected)
                EnableGroups();
            statusBar.Items.Remove(statusLabel);
        }

        private void loadFromFileButton_Click(object sender, EventArgs e)
        {
            var statusLabel = new ToolStripStatusLabel("Creating new user");
            statusBar.Items.Add(statusLabel);
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Certificate (.pfx)|*.pfx",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var certificate in openFileDialog.FileNames)
                {
                    _identitiesList.Add(new UserCertificate(certificate));
                }
            }
            connectButton.Enabled = true;
            statusBar.Items.Remove(statusLabel);
        }

        private async void newGroupButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Creating new group");
            statusBar.Items.Add(statusLabel);

            using (var newGroupForm = new NewGroupForm())
            {
                if (newGroupForm.ShowDialog() == DialogResult.OK)
                {
                    var newGroupInfo = newGroupForm.NewGroupInfo;
                    try
                    {
                        await ServerTransaction.CreateNewGroup(newGroupInfo, _certificate);
                        var message = string.Format("Group {0} created", newGroupInfo.Name);
                        MessageBox.Show(message, "New group");
                        await UpdateGroupList();
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

            EnableIdentity();
            EnableGroups();
            if (_connectedToGroup)
            {
                EnableUsers();
                EnabledFiles();
            }
            statusBar.Items.Remove(statusLabel);
        }

        private async void connectToGroupButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Connecting to group");
            statusBar.Items.Add(statusLabel);

            using (var passwordForm = new PasswordForm())
            {
                passwordForm.ShowDialog();
                if (passwordForm.DialogResult == DialogResult.OK)
                {
                    var password = passwordForm.Password;
                    var selectedGroup = groupsListBox.SelectedItem as Group;
                    if (Hasher.HashMD5String(password).Equals(selectedGroup.Password, StringComparison.OrdinalIgnoreCase))
                    {
                        await ConnectToGroup(selectedGroup.Name);   
                        var message = string.Format("You are now connected to gropup {0}", selectedGroup.Name);
                        MessageBox.Show(message, "Connection sucessfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Typed password is wrong", "Wrong password", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }

            EnableIdentity();
            EnableGroups();
            if (_connectedToGroup)
            {
                EnableUsers();
                EnabledFiles();
            }
            statusBar.Items.Remove(statusLabel);
        }

        private async void sendFilesButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Sending files");
            statusBar.Items.Add(statusLabel);
            var selectedUsers = usersListBox.SelectedItems;
            if (selectedUsers.Count > 0)
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = openFileDialog.SafeFileName;

                    foreach (User user in selectedUsers)
                    {
                        if (!encryptCheckBox.Checked)
                        {
                            statusLabel.Text = string.Format("Uploading {0} to cloud", fileName);
                            await _cloud.UploadFileAsync(filePath, user, fileName);
                            if (user == _currentUser)
                            {
                                filesListBox.DataSource = await _cloud.GetFilesListAsync(_currentUser);
                            }
                        }
                        else
                        {
                            var tempFile = Path.GetTempFileName();
                            var tempCertificate = Path.GetTempFileName();
                            statusLabel.Text = string.Format("Downloading certificate ({0})", user.CommonName);
                            await _cloud.DownloadCertificateAsync(tempCertificate, user);
                            statusLabel.Text = string.Format("Encrypting {0}", fileName);
                            await Encrypt.FileWithPublicKey(filePath, tempFile, new UserCertificate(tempCertificate));
                            statusLabel.Text = string.Format("Uploading {0} to cloud", fileName); ;
                            await _cloud.UploadFileAsync(tempFile, user, fileName);
                            if (user == _currentUser)
                            {
                                filesListBox.DataSource = await _cloud.GetFilesListAsync(_currentUser);
                            }
                            File.Delete(tempFile);
                            File.Delete(tempCertificate);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Select users first", "Select users", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            EnableAll();
            statusBar.Items.Remove(statusLabel);
        }

        private async void downloadFilesButton_Click(object sender, EventArgs e)
        {
            var statusLabel = new ToolStripStatusLabel("Downloading files");
            statusBar.Items.Add(statusLabel);
            var folderBroswerDialog = new FolderBrowserDialog();
            if (folderBroswerDialog.ShowDialog() == DialogResult.OK)
            {
                var selectedFiles = filesListBox.SelectedItems;
                foreach (CloudFile file in selectedFiles)
                {
                    if (!encryptCheckBox.Checked)
                    {
                        statusLabel.Text = string.Format("Downloading {0}", file.Name);
                        await _cloud.DownloadFileAsync("", _currentUser, file.Name);
                    }
                    else
                    {
                        var tempFile = Path.GetTempFileName();
                        statusLabel.Text = string.Format("Downloading {0}", file.Name);
                        await _cloud.DownloadFileAsync(tempFile, _currentUser, file.Name);
                        statusLabel.Text = string.Format("Decrypting {0}", file.Name); ;
                        var outputPath = string.Format("{0}\\{1}", folderBroswerDialog.SelectedPath, file.Name);
                        await Decrypt.FileWithPrivateKey(tempFile, outputPath, _certificate);
                    }
                }
            }
            


            EnableAll();
            statusBar.Items.Remove(statusLabel);
        }

        private async void refreshFilesButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Refreshing files");
            statusBar.Items.Add(statusLabel);

            filesListBox.DataSource = await _cloud.GetFilesListAsync(_currentUser);

            EnableAll();
            statusBar.Items.Remove(statusLabel);
        }

        private async void deleteFilesButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            var statusLabel = new ToolStripStatusLabel("Deleting files");
            statusBar.Items.Add(statusLabel);

            var selectedFiles = filesListBox.SelectedItems;
            foreach (CloudFile file in selectedFiles)
            {
                statusLabel.Text = string.Format("Deleting {0}", file.Name);
                await _cloud.DeleteFileAsync(_currentUser, file.Name);
            }

            statusLabel.Text = "Refreshing files";
            filesListBox.DataSource = await _cloud.GetFilesListAsync(_currentUser);

            EnableAll();
            statusBar.Items.Remove(statusLabel);
        }

        private void ClearProperties()
        {
            _cloud = new Dropbox();
            _currentUser = null;
            groupsListBox.DataSource = null;
            usersListBox.DataSource = null;
            filesListBox.DataSource = null;
        }

        private async Task UpdateGroupList()
        {
            groupsListBox.DataSource = await _cloud.GetGroupListAsync();
        }

        private async Task ConnectToGroup(string group)
        {
            _connectedToGroup = true;
            ReloadButtonState();
            _cloud = new Dropbox(group);
            usersListBox.DataSource = await _cloud.GetUsersListAsync(_currentUser);
            filesListBox.DataSource = await _cloud.GetFilesListAsync(_currentUser);
        }

        private void DisconnectFromGroup()
        {
            _connectedToGroup = false;
            ReloadButtonState();
            ClearProperties();
            DisableFiles();
            DisableUsers();
            DisableGroups();
        }

        private async Task ConnectUser()
        {
            DisconnectFromGroup();
            _connected = true;
            ReloadButtonState();
            await UpdateGroupList();
        }

        private void DisconnectUser()
        {
            _connected = false;
            loginLabel.Text = "Unconnected";
            DisconnectFromGroup();
        }

        
        #region Interfaces states controls
        private void ReloadButtonState()
        {
            newGroupButton.Enabled = _connected;
            connectToGroupButton.Enabled = _connected;
            sendFilesButton.Enabled = _connected && _connectedToGroup;
            downloadFilesButton.Enabled = _connected && _connectedToGroup;
            refreshFilesButton.Enabled = _connected && _connectedToGroup;
            if (_identitiesList.Count == 0)
            {
                connectButton.Enabled = false;
            }
            else
            {
                connectButton.Enabled = true;
            }
        }

        private void DisableIdentity()
        {
            identityPanel.Enabled = false;
        }

        private void DisableGroups()
        {
            groupsPanel.Enabled = false;
        }

        private void DisableUsers()
        {
            usersPanel.Enabled = false;
        }

        private void DisableFiles()
        {
            filesPanel.Enabled = false;
        }

        private void DisableAll()
        {
            DisableIdentity();
            DisableGroups();
            DisableUsers();
            DisableFiles();
        }

        private void EnableIdentity()
        {
            identityPanel.Enabled = true;
        }

        private void EnableGroups()
        {
            groupsPanel.Enabled = true;
        }

        private void EnableUsers()
        {
            usersPanel.Enabled = true;
        }

        private void EnabledFiles()
        {
            filesPanel.Enabled = true;
        }

        private void EnableAll()
        {
            EnableIdentity();
            EnableGroups();
            EnableUsers();
            EnabledFiles();
        }
        #endregion
    }
}
