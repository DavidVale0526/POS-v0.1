using System.Windows;
using System.Windows.Controls;
using POS_v0_1.ViewModels;

namespace POS_v0_1.Views
{
    public partial class PasswordRecoveryView : UserControl
    {
        public PasswordRecoveryView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordRecoveryViewModel vm)
            {
                vm.NewPassword = PassBox.Password;
            }
        }
    }
}
