using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<User> Users => _main.Users;

        public ICommand ShowCreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UserManagementViewModel(MainViewModel main)
        {
            _main = main;
            ShowCreateUserCommand = new RelayCommand(o => _main.CurrentView = new CreateUserViewModel(_main));
            EditUserCommand = new RelayCommand(o =>
            {
                if (o is User user)
                {
                    _main.CurrentView = new CreateUserViewModel(_main, user);
                }
            });
            DeleteUserCommand = new RelayCommand(o =>
            {
                if (o is User user)
                {
                    var result = MessageBox.Show($"¿Está seguro de eliminar al usuario {user.Name}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _main.Users.Remove(user);
                        var dbUser = _main.Db.Users.Find(user.Id);
                        if (dbUser != null)
                        {
                            _main.Db.Users.Remove(dbUser);
                            _main.Db.SaveChanges();
                        }
                    }
                }
            });
        }
    }

    public class CreateUserViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly User _editingUser;

        public string Title => _editingUser == null ? "Crear Usuario" : "Editar Usuario";

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _role = "Usuario";
        public string Role { get => _role; set { _role = value; OnPropertyChanged(); } }

        private string _pin;
        public string Pin { get => _pin; set { _pin = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Usuario" };

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateUserViewModel(MainViewModel main, User user = null)
        {
            _main = main;
            _editingUser = user;

            if (_editingUser != null)
            {
                Name = _editingUser.Name;
                Email = _editingUser.Email;
                Role = _editingUser.Role;
                Pin = _editingUser.Pin;
                Password = _editingUser.Password;
                ConfirmPassword = _editingUser.Password;
            }

            SaveCommand = new RelayCommand(o => Save());
            BackCommand = new RelayCommand(o => _main.CurrentView = new UserManagementViewModel(_main));
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(Pin) && (Pin.Length < 4 || Pin.Length > 6 || !Pin.All(char.IsDigit)))
            {
                MessageBox.Show("El PIN debe ser de 4 a 6 dígitos numéricos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_editingUser == null)
            {
                var newUser = new User
                {
                    Name = Name,
                    Email = Email,
                    Role = Role,
                    Pin = Pin,
                    Password = Password,
                    RegistrationDate = DateTime.Now
                };

                _main.Db.Users.Add(newUser);
                _main.Db.SaveChanges();
                _main.Users.Add(newUser);
            }
            else
            {
                _editingUser.Name = Name;
                _editingUser.Email = Email;
                _editingUser.Role = Role;
                _editingUser.Pin = Pin;
                _editingUser.Password = Password;
                _main.Db.SaveChanges();
            }

            _main.CurrentView = new UserManagementViewModel(_main);
        }
    }
}
