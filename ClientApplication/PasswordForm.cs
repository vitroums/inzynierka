using System;
using System.Security;
using System.Windows.Forms;

namespace ClientApplication
{
    public partial class PasswordForm : Form
    {
        public string Password { get; private set; }

        public PasswordForm()
        {
            InitializeComponent();
        }

        private void passwordTextBox_TextChanged(object sender, System.EventArgs e)
        {
            Password = ((TextBox)sender).Text;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Input password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
    }
}
