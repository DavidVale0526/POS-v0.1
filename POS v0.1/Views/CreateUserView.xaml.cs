using System.Windows;
using System.Windows.Controls;
using POS_v0_1.ViewModels;

namespace POS_v0_1.Views
{
    public partial class CreateUserView : UserControl
    {
        public CreateUserView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateUserViewModel vm)
            {
                if (sender is PasswordBox pb)
                {
                    if (pb.Name == "PassBox")
                        vm.Password = pb.Password;
                    else if (pb.Name == "ConfirmPassBox")
                        vm.ConfirmPassword = pb.Password;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateUserViewModel vm)
            {
                PassBox.Password = vm.Password;
                ConfirmPassBox.Password = vm.ConfirmPassword;
            }
        }
    }
}
