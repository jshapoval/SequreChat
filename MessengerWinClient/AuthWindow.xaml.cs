using System.Windows;

namespace MessengerWinClient
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Email
        {
            get { return EmailTextBox.Text; }
        }

        public string Password
        {
            get { return PwdBox.Password; }
        }
    }
}
