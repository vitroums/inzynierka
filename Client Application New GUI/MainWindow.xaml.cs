using System.Collections.Generic;
using System.Windows;
using System.Security.Cryptography.X509Certificates;
using Client;
using System.Collections.ObjectModel;

namespace Client_Application_New_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Certificate> CertificatesList { get; private set; }
        public MainWindow()
        {
            CertificatesList = new ObservableCollection<Certificate>(Certificates.GetCertificates());
            InitializeComponent();
        }

        private void AddCertificateFromFileButton(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Certificate (*.pfx)|*pfx|Certificate (*.crt)|*.crt" };
            var result = openFileDialog.ShowDialog();
            if (result != false)
            {
                var certificateFilePath = openFileDialog.FileName;
                try
                {
                    CertificatesList.Add(new Certificate(certificateFilePath));
                    MessageBox.Show("Certificate loaded");
                }
                catch
                {
                    MessageBox.Show("Couldn't load certificate from file");
                }
            }

        }
    }
}
