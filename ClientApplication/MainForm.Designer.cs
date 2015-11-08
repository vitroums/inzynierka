namespace ClientApplication
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupsListBox = new System.Windows.Forms.ListBox();
            this.groupsHeaderLabel = new System.Windows.Forms.Label();
            this.identitiesHeaderLabel = new System.Windows.Forms.Label();
            this.usersListBox = new System.Windows.Forms.ListBox();
            this.usersHeaderLabel = new System.Windows.Forms.Label();
            this.connectToGroupButton = new System.Windows.Forms.Button();
            this.newUserButton = new System.Windows.Forms.Button();
            this.loadFromFileButton = new System.Windows.Forms.Button();
            this.sendFilesButton = new System.Windows.Forms.Button();
            this.filesListBox = new System.Windows.Forms.ListBox();
            this.myFileslabel = new System.Windows.Forms.Label();
            this.downloadFilesButton = new System.Windows.Forms.Button();
            this.newGroupButton = new System.Windows.Forms.Button();
            this.encryptCheckBox = new System.Windows.Forms.CheckBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.identitiesListBox = new System.Windows.Forms.ListBox();
            this.identityPanel = new System.Windows.Forms.Panel();
            this.groupsPanel = new System.Windows.Forms.Panel();
            this.usersPanel = new System.Windows.Forms.Panel();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.loginLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.filesPanel = new System.Windows.Forms.Panel();
            this.deleteFilesButton = new System.Windows.Forms.Button();
            this.refreshFilesButton = new System.Windows.Forms.Button();
            this.getCertificatesButton = new System.Windows.Forms.Button();
            this.identityPanel.SuspendLayout();
            this.groupsPanel.SuspendLayout();
            this.usersPanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.filesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupsListBox
            // 
            this.groupsListBox.FormattingEnabled = true;
            this.groupsListBox.ItemHeight = 16;
            this.groupsListBox.Location = new System.Drawing.Point(8, 23);
            this.groupsListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupsListBox.Name = "groupsListBox";
            this.groupsListBox.Size = new System.Drawing.Size(251, 180);
            this.groupsListBox.TabIndex = 0;
            // 
            // groupsHeaderLabel
            // 
            this.groupsHeaderLabel.AutoSize = true;
            this.groupsHeaderLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.groupsHeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupsHeaderLabel.Location = new System.Drawing.Point(4, 0);
            this.groupsHeaderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.groupsHeaderLabel.Name = "groupsHeaderLabel";
            this.groupsHeaderLabel.Size = new System.Drawing.Size(70, 20);
            this.groupsHeaderLabel.TabIndex = 2;
            this.groupsHeaderLabel.Text = "Groups";
            // 
            // identitiesHeaderLabel
            // 
            this.identitiesHeaderLabel.AutoSize = true;
            this.identitiesHeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.identitiesHeaderLabel.Location = new System.Drawing.Point(4, 0);
            this.identitiesHeaderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.identitiesHeaderLabel.Name = "identitiesHeaderLabel";
            this.identitiesHeaderLabel.Size = new System.Drawing.Size(70, 20);
            this.identitiesHeaderLabel.TabIndex = 3;
            this.identitiesHeaderLabel.Text = "Identity";
            // 
            // usersListBox
            // 
            this.usersListBox.FormattingEnabled = true;
            this.usersListBox.ItemHeight = 16;
            this.usersListBox.Location = new System.Drawing.Point(8, 25);
            this.usersListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.usersListBox.Name = "usersListBox";
            this.usersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.usersListBox.Size = new System.Drawing.Size(251, 180);
            this.usersListBox.TabIndex = 5;
            // 
            // usersHeaderLabel
            // 
            this.usersHeaderLabel.AutoSize = true;
            this.usersHeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.usersHeaderLabel.Location = new System.Drawing.Point(4, 0);
            this.usersHeaderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.usersHeaderLabel.Name = "usersHeaderLabel";
            this.usersHeaderLabel.Size = new System.Drawing.Size(59, 20);
            this.usersHeaderLabel.TabIndex = 6;
            this.usersHeaderLabel.Text = "Users";
            // 
            // connectToGroupButton
            // 
            this.connectToGroupButton.Location = new System.Drawing.Point(268, 59);
            this.connectToGroupButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectToGroupButton.Name = "connectToGroupButton";
            this.connectToGroupButton.Size = new System.Drawing.Size(132, 28);
            this.connectToGroupButton.TabIndex = 8;
            this.connectToGroupButton.Text = "Connect to group";
            this.connectToGroupButton.UseVisualStyleBackColor = true;
            this.connectToGroupButton.Click += new System.EventHandler(this.connectToGroupButton_Click);
            // 
            // newUserButton
            // 
            this.newUserButton.Location = new System.Drawing.Point(116, 133);
            this.newUserButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.newUserButton.Name = "newUserButton";
            this.newUserButton.Size = new System.Drawing.Size(100, 28);
            this.newUserButton.TabIndex = 9;
            this.newUserButton.Text = "New user";
            this.newUserButton.UseVisualStyleBackColor = true;
            this.newUserButton.Click += new System.EventHandler(this.newUserButton_Click);
            // 
            // loadFromFileButton
            // 
            this.loadFromFileButton.Location = new System.Drawing.Point(224, 133);
            this.loadFromFileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.loadFromFileButton.Name = "loadFromFileButton";
            this.loadFromFileButton.Size = new System.Drawing.Size(176, 28);
            this.loadFromFileButton.TabIndex = 10;
            this.loadFromFileButton.Text = "Load certificate from file";
            this.loadFromFileButton.UseVisualStyleBackColor = true;
            this.loadFromFileButton.Click += new System.EventHandler(this.loadFromFileButton_Click);
            // 
            // sendFilesButton
            // 
            this.sendFilesButton.Location = new System.Drawing.Point(268, 25);
            this.sendFilesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sendFilesButton.Name = "sendFilesButton";
            this.sendFilesButton.Size = new System.Drawing.Size(132, 87);
            this.sendFilesButton.TabIndex = 11;
            this.sendFilesButton.Text = "Send files to selected users";
            this.sendFilesButton.UseVisualStyleBackColor = true;
            this.sendFilesButton.Click += new System.EventHandler(this.sendFilesButton_Click);
            // 
            // filesListBox
            // 
            this.filesListBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.filesListBox.FormattingEnabled = true;
            this.filesListBox.ItemHeight = 16;
            this.filesListBox.Location = new System.Drawing.Point(8, 25);
            this.filesListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.filesListBox.Name = "filesListBox";
            this.filesListBox.Size = new System.Drawing.Size(391, 468);
            this.filesListBox.TabIndex = 12;
            // 
            // myFileslabel
            // 
            this.myFileslabel.AutoSize = true;
            this.myFileslabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.myFileslabel.Location = new System.Drawing.Point(4, 0);
            this.myFileslabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.myFileslabel.Name = "myFileslabel";
            this.myFileslabel.Size = new System.Drawing.Size(75, 20);
            this.myFileslabel.TabIndex = 13;
            this.myFileslabel.Text = "My files";
            // 
            // downloadFilesButton
            // 
            this.downloadFilesButton.Location = new System.Drawing.Point(8, 537);
            this.downloadFilesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.downloadFilesButton.Name = "downloadFilesButton";
            this.downloadFilesButton.Size = new System.Drawing.Size(125, 65);
            this.downloadFilesButton.TabIndex = 14;
            this.downloadFilesButton.Text = "Download selected files";
            this.downloadFilesButton.UseVisualStyleBackColor = true;
            this.downloadFilesButton.Click += new System.EventHandler(this.downloadFilesButton_Click);
            // 
            // newGroupButton
            // 
            this.newGroupButton.Location = new System.Drawing.Point(268, 23);
            this.newGroupButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.newGroupButton.Name = "newGroupButton";
            this.newGroupButton.Size = new System.Drawing.Size(132, 28);
            this.newGroupButton.TabIndex = 15;
            this.newGroupButton.Text = "New group";
            this.newGroupButton.UseVisualStyleBackColor = true;
            this.newGroupButton.Click += new System.EventHandler(this.newGroupButton_Click);
            // 
            // encryptCheckBox
            // 
            this.encryptCheckBox.AutoSize = true;
            this.encryptCheckBox.Checked = true;
            this.encryptCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.encryptCheckBox.Location = new System.Drawing.Point(8, 508);
            this.encryptCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.encryptCheckBox.Name = "encryptCheckBox";
            this.encryptCheckBox.Size = new System.Drawing.Size(136, 21);
            this.encryptCheckBox.TabIndex = 18;
            this.encryptCheckBox.Text = "En(de)crypt data";
            this.encryptCheckBox.UseVisualStyleBackColor = true;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(8, 133);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 28);
            this.connectButton.TabIndex = 21;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // identitiesListBox
            // 
            this.identitiesListBox.FormattingEnabled = true;
            this.identitiesListBox.ItemHeight = 16;
            this.identitiesListBox.Location = new System.Drawing.Point(8, 25);
            this.identitiesListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.identitiesListBox.Name = "identitiesListBox";
            this.identitiesListBox.Size = new System.Drawing.Size(391, 100);
            this.identitiesListBox.TabIndex = 22;
            // 
            // identityPanel
            // 
            this.identityPanel.Controls.Add(this.identitiesListBox);
            this.identityPanel.Controls.Add(this.connectButton);
            this.identityPanel.Controls.Add(this.identitiesHeaderLabel);
            this.identityPanel.Controls.Add(this.loadFromFileButton);
            this.identityPanel.Controls.Add(this.newUserButton);
            this.identityPanel.Location = new System.Drawing.Point(16, 15);
            this.identityPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.identityPanel.Name = "identityPanel";
            this.identityPanel.Size = new System.Drawing.Size(408, 169);
            this.identityPanel.TabIndex = 23;
            // 
            // groupsPanel
            // 
            this.groupsPanel.Controls.Add(this.groupsHeaderLabel);
            this.groupsPanel.Controls.Add(this.groupsListBox);
            this.groupsPanel.Controls.Add(this.newGroupButton);
            this.groupsPanel.Controls.Add(this.connectToGroupButton);
            this.groupsPanel.Location = new System.Drawing.Point(16, 191);
            this.groupsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupsPanel.Name = "groupsPanel";
            this.groupsPanel.Size = new System.Drawing.Size(408, 213);
            this.groupsPanel.TabIndex = 24;
            // 
            // usersPanel
            // 
            this.usersPanel.Controls.Add(this.getCertificatesButton);
            this.usersPanel.Controls.Add(this.usersHeaderLabel);
            this.usersPanel.Controls.Add(this.usersListBox);
            this.usersPanel.Controls.Add(this.sendFilesButton);
            this.usersPanel.Location = new System.Drawing.Point(16, 411);
            this.usersPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.usersPanel.Name = "usersPanel";
            this.usersPanel.Size = new System.Drawing.Size(408, 213);
            this.usersPanel.TabIndex = 25;
            // 
            // statusBar
            // 
            this.statusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.loginLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 641);
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusBar.Size = new System.Drawing.Size(855, 25);
            this.statusBar.TabIndex = 26;
            this.statusBar.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(101, 20);
            this.toolStripStatusLabel1.Text = "Connected as:";
            // 
            // loginLabel
            // 
            this.loginLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.loginLabel.Name = "loginLabel";
            this.loginLabel.Size = new System.Drawing.Size(101, 20);
            this.loginLabel.Text = "Unconnected";
            // 
            // filesPanel
            // 
            this.filesPanel.Controls.Add(this.deleteFilesButton);
            this.filesPanel.Controls.Add(this.refreshFilesButton);
            this.filesPanel.Controls.Add(this.myFileslabel);
            this.filesPanel.Controls.Add(this.filesListBox);
            this.filesPanel.Controls.Add(this.downloadFilesButton);
            this.filesPanel.Controls.Add(this.encryptCheckBox);
            this.filesPanel.Location = new System.Drawing.Point(432, 15);
            this.filesPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.filesPanel.Name = "filesPanel";
            this.filesPanel.Size = new System.Drawing.Size(408, 609);
            this.filesPanel.TabIndex = 27;
            // 
            // deleteFilesButton
            // 
            this.deleteFilesButton.Location = new System.Drawing.Point(275, 537);
            this.deleteFilesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.deleteFilesButton.Name = "deleteFilesButton";
            this.deleteFilesButton.Size = new System.Drawing.Size(125, 65);
            this.deleteFilesButton.TabIndex = 20;
            this.deleteFilesButton.Text = "Delete files";
            this.deleteFilesButton.UseVisualStyleBackColor = true;
            this.deleteFilesButton.Click += new System.EventHandler(this.deleteFilesButton_Click);
            // 
            // refreshFilesButton
            // 
            this.refreshFilesButton.Location = new System.Drawing.Point(141, 537);
            this.refreshFilesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.refreshFilesButton.Name = "refreshFilesButton";
            this.refreshFilesButton.Size = new System.Drawing.Size(125, 65);
            this.refreshFilesButton.TabIndex = 19;
            this.refreshFilesButton.Text = "Refresh files";
            this.refreshFilesButton.UseVisualStyleBackColor = true;
            this.refreshFilesButton.Click += new System.EventHandler(this.refreshFilesButton_Click);
            // 
            // getCertificatesButton
            // 
            this.getCertificatesButton.Location = new System.Drawing.Point(268, 119);
            this.getCertificatesButton.Name = "getCertificatesButton";
            this.getCertificatesButton.Size = new System.Drawing.Size(132, 86);
            this.getCertificatesButton.TabIndex = 12;
            this.getCertificatesButton.Text = "Download certificates of selected users";
            this.getCertificatesButton.UseVisualStyleBackColor = true;
            this.getCertificatesButton.Click += new System.EventHandler(this.getCertificatesButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 666);
            this.Controls.Add(this.filesPanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.usersPanel);
            this.Controls.Add(this.groupsPanel);
            this.Controls.Add(this.identityPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "CloudPKI";
            this.identityPanel.ResumeLayout(false);
            this.identityPanel.PerformLayout();
            this.groupsPanel.ResumeLayout(false);
            this.groupsPanel.PerformLayout();
            this.usersPanel.ResumeLayout(false);
            this.usersPanel.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.filesPanel.ResumeLayout(false);
            this.filesPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox groupsListBox;
        private System.Windows.Forms.Label groupsHeaderLabel;
        private System.Windows.Forms.Label identitiesHeaderLabel;
        private System.Windows.Forms.ListBox usersListBox;
        private System.Windows.Forms.Button connectToGroupButton;
        private System.Windows.Forms.Button newUserButton;
        private System.Windows.Forms.Button loadFromFileButton;
        private System.Windows.Forms.Button sendFilesButton;
        private System.Windows.Forms.ListBox filesListBox;
        private System.Windows.Forms.Label myFileslabel;
        private System.Windows.Forms.Button downloadFilesButton;
        private System.Windows.Forms.Button newGroupButton;
        private System.Windows.Forms.CheckBox encryptCheckBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ListBox identitiesListBox;
        private System.Windows.Forms.Label usersHeaderLabel;
        private System.Windows.Forms.Panel identityPanel;
        private System.Windows.Forms.Panel groupsPanel;
        private System.Windows.Forms.Panel usersPanel;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel loginLabel;
        private System.Windows.Forms.Panel filesPanel;
        private System.Windows.Forms.Button refreshFilesButton;
        private System.Windows.Forms.Button deleteFilesButton;
        private System.Windows.Forms.Button getCertificatesButton;
    }
}

