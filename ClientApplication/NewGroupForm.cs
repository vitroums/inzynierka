using System.Windows.Forms;
using Client.ServerData;
using System;

namespace ClientApplication
{
    public partial class NewGroupForm : Form
    {
        public GroupInfo NewGroupInfo { get; set; }
        public NewGroupForm()
        {
            InitializeComponent();
            NewGroupInfo = new GroupInfo()
            {
                Name = nameTextBox.Text
            };
        }

        private void nameTextBox_TextChanged(object sender, System.EventArgs e)
        {
            NewGroupInfo.Name = ((TextBox)sender).Text;
        }

        private void passwordTextBox_TextChanged(object sender, System.EventArgs e)
        {
            NewGroupInfo.Password = ((TextBox)sender).Text;
        }

        private void repeatPasswordTextBox_TextChanged(object sender, System.EventArgs e)
        {
            NewGroupInfo.RepeatPassword = ((TextBox)sender).Text;
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
            if (!NewGroupInfo.Validate())
            {
                MessageBox.Show("Fill all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
    }
}
