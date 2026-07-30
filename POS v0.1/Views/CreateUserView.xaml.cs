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

        // Manejo manual de PasswordBox ya que no soporta Binding directo por seguridad
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateUserViewModel vm)
            {
                // En una app real usaríamos SecureString o un Helper
                // Para este v0.1, sincronizaremos al hacer clic en Guardar o mediante eventos
            }
        }
    }
}
