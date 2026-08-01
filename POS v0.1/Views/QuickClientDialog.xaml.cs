using System.Windows;
using POS_v0_1.Models;

namespace POS_v0_1.Views
{
    public partial class QuickClientDialog : Window
    {
        public Client Client { get; private set; }

        public QuickClientDialog()
        {
            InitializeComponent();
            NameBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Client = new Client
            {
                Name = NameBox.Text,
                Phone = PhoneBox.Text,
                Email = EmailBox.Text,
                CreatedAt = System.DateTime.Now
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
