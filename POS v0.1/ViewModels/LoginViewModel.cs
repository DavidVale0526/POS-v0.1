using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace POS_v0_1.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel main)
        {
            _main = main;
            LoginCommand = new RelayCommand(o => Login());
        }

        private void Login()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Por favor ingrese sus credenciales.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _main.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);
            if (user != null)
            {
                _main.CurrentUser = user;
                _main.IsAuthenticated = true;
                // Navigate to the dedicated cash control view so the session is created/persisted correctly
                _main.CurrentView = new CashControlViewModel(_main);
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
