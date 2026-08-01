using System.Windows.Controls;

namespace POS_v0_1.Views
{
    public partial class CreateRepairView : UserControl
    {
        public CreateRepairView()
        {
            InitializeComponent();
        }

        private void PinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateRepairViewModel vm && sender is PasswordBox pb)
            {
                vm.SellerPin = pb.Password;
            }
        }
    }
}
