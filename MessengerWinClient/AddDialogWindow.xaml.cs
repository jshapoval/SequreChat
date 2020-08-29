using System.Windows;

namespace MessengerWinClient
{
    /// <summary>
    /// Логика взаимодействия для AddDialogWindow.xaml
    /// </summary>
    public partial class AddDialogWindow : Window
    {
        public AddDialogWindow()
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
    }
}
