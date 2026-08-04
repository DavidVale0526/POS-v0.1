using System;
using System.Windows;
using System.Windows.Controls;

namespace POS_v0_1.Views
{
    public partial class MixedPaymentDialog : Window
    {
        private decimal _total;
        public decimal CashAmount { get; private set; }
        public decimal CardAmount { get; private set; }
        public decimal TransferAmount { get; private set; }

        public MixedPaymentDialog(decimal total)
        {
            InitializeComponent();
            _total = total;
            TotalText.Text = total.ToString("C2");
            UpdateRemaining();
            CashBox.Focus();
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRemaining();
        }

        private void UpdateRemaining()
        {
            decimal cash = decimal.TryParse(CashBox.Text, out var c) ? cash = c : 0;
            decimal card = decimal.TryParse(CardBox.Text, out var r) ? card = r : 0;
            decimal trans = decimal.TryParse(TransferBox.Text, out var t) ? trans = t : 0;

            decimal totalEntered = cash + card + trans;
            decimal diff = _total - totalEntered;

            if (diff > 0)
            {
                RemainingLabel.Text = "Faltante:";
                RemainingText.Text = diff.ToString("C2");
                RemainingText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (diff < 0)
            {
                RemainingLabel.Text = "Cambio:";
                RemainingText.Text = Math.Abs(diff).ToString("C2");
                RemainingText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                RemainingLabel.Text = "Cubierto:";
                RemainingText.Text = "Exacto";
                RemainingText.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            decimal cash = decimal.TryParse(CashBox.Text, out var c) ? cash = c : 0;
            decimal card = decimal.TryParse(CardBox.Text, out var r) ? card = r : 0;
            decimal trans = decimal.TryParse(TransferBox.Text, out var t) ? trans = t : 0;

            if (cash + card + trans < _total)
            {
                MessageBox.Show("El monto total ingresado es menor al total de la venta.", "Pago Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CashAmount = cash;
            CardAmount = card;
            TransferAmount = trans;
            DialogResult = true;
            Close();
        }
    }
}
