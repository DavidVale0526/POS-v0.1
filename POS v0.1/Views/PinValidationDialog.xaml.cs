using System.Windows;
using System.Windows.Input;

namespace POS_v0_1.Views
{
    public partial class PinValidationDialog : Window
    {
        public string Pin { get; private set; }

        public PinValidationDialog()
        {
            InitializeComponent();
            PinBox.Focus();
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        private void PinBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Confirm();
            }
        }

        private void Confirm()
        {
            Pin = PinBox.Password;
            if (string.IsNullOrWhiteSpace(Pin))
            {
                MessageBox.Show("Por favor ingrese un PIN.", "PIN Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }
    }
}
