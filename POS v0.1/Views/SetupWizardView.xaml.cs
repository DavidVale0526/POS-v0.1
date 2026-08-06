using System.Windows;
using System.Windows.Controls;
using POS_v0_1.ViewModels;

namespace POS_v0_1.Views
{
    public partial class SetupWizardView : UserControl
    {
        public SetupWizardView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SetupWizardViewModel vm)
            {
                vm.AdminPassword = PassBox.Password;
            }
        }
    }
}
