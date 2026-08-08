using System.Windows;
using POS_v0_1.Models;

namespace POS_v0_1.Views
{
    public partial class QuickProviderDialog : Window
    {
        public Provider Provider { get; private set; }

        public QuickProviderDialog()
        {
            InitializeComponent();
            NameBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Provider = new Provider
            {
                Name = NameBox.Text,
                Phone = PhoneBox.Text,
                ContactPerson = ContactBox.Text,
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
