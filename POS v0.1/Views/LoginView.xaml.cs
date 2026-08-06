using System.Windows;
using System.Windows.Controls;
using POS_v0_1.ViewModels;

namespace POS_v0_1.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PassBox.Password;
            }
        }
    }
}
