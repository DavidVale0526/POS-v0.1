using System;
using System.Windows;
using POS_v0_1.Models;

namespace POS_v0_1.Views
{
    public partial class StockAdjustmentDialog : Window
    {
        private Product _product;
        public int Adjustment { get; private set; }
        public string Reason { get; private set; }

        public StockAdjustmentDialog(Product product)
        {
            InitializeComponent();
            _product = product;
            ProductNameText.Text = "Producto: " + product.Name;
            CurrentStockText.Text = "Stock Actual: " + product.Stock;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AdjustAmountBox.Text, out int result))
            {
                Adjustment = result;
                Reason = string.IsNullOrWhiteSpace(ReasonBox.Text) ? "Ajuste Manual" : ReasonBox.Text;

                if (_product.Stock + Adjustment < 0)
                {
                    MessageBox.Show("El ajuste dejaría el stock en negativo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Ingrese un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
