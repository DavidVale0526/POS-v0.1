using System.Windows.Controls;
using System.Windows.Input;
using POS_v0_1.ViewModels;

namespace POS_v0_1.Views
{
    public partial class PosView : UserControl
    {
        public PosView()
        {
            InitializeComponent();
            this.Loaded += PosView_Loaded;
        }

        private void PosView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Focus the hidden barcode input so keyboard-emulating scanners send their data here
            BarcodeInput?.Focus();
        }

        private void BarcodeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var code = BarcodeInput.Text?.Trim();
                if (!string.IsNullOrEmpty(code))
                {
                    if (DataContext is PosViewModel vm)
                    {
                        vm.HandleBarcodeScan(code);
                    }
                }
                BarcodeInput.Clear();
                e.Handled = true;
            }
        }
    }
}
