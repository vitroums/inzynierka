using System;
using System.Windows.Forms;
using Client.Certificates;

namespace ClientApplication
{
    public partial class NewUserForm : Form
    {
        public CertificateInfo NewUserInfo { get; private set; }
        public string PathToSave { get; private set; }

        public NewUserForm()
        {
            InitializeComponent();
            NewUserInfo = new CertificateInfo()
            {
                Country = countryTextBox.Text,
                State = stateTextBox.Text,
                City = cityTextBox.Text,
                Organization = organizationTextBox.Text,
                Unit = unitTextBox.Text
            };
        }

        private void countryTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.Country = ((TextBox)sender).Text;
        }

        private void stateTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.State = ((TextBox)sender).Text;
        }

        private void cityTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.City = ((TextBox)sender).Text;
        }

        private void organizationTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.Organization = ((TextBox)sender).Text;
        }

        private void unitTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.Unit = ((TextBox)sender).Text;
        }

        private void commonNameTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.CommonName = ((TextBox)sender).Text;
        }

        private void emailTextBox_TextChanged(object sender, EventArgs e)
        {
            NewUserInfo.Email = ((TextBox)sender).Text;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                okButton_Click(this, new EventArgs());
            }
            else if (e.KeyCode == Keys.Escape)
            {
                cancelButton_Click(this, new EventArgs());
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!NewUserInfo.Validate())
            {
                MessageBox.Show("Fill all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DialogResult = DialogResult.OK;
                PathToSave = pathTextBox.Text + "\\" + commonNameTextBox.Text + ".pfx";
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void NewUserForm_Load(object sender, EventArgs e)
        {
            pathTextBox.Text = Application.StartupPath + "\\Certificates";
        }

        private void selectPathButton_Click(object sender, EventArgs e)
        {
            var folderBroswerDialog = new FolderBrowserDialog();
            if (folderBroswerDialog.ShowDialog() == DialogResult.OK)
            {
                pathTextBox.Text = folderBroswerDialog.SelectedPath;
            }
        }
    }
}
