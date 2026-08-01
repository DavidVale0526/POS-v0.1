using System.Windows;
using System.Windows.Input;

namespace POS_v0_1.Views
{
    public partial class PinValidationDialog : Window
    {
        public string Pin { get; private set; }
        public decimal? Amount { get; private set; }

        public PinValidationDialog(bool showAmount = false, decimal? defaultAmount = null)
        {
            InitializeComponent();
            PinBox.Focus();

            if (showAmount)
            {
                AmountPanel.Visibility = Visibility.Visible;
                Height = 350; // Aumentar tamaño para mostrar el campo de monto
                if (defaultAmount.HasValue)
                {
                    AmountBox.Text = defaultAmount.Value.ToString("N2");
                }
            }
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

            if (AmountPanel.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(AmountBox.Text))
            {
                if (decimal.TryParse(AmountBox.Text, out decimal result))
                {
                    Amount = result;
                }
                else
                {
                    MessageBox.Show("El monto ingresado no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
