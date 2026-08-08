using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class User : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string? _email;
        public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _role = "Usuario";
        public string Role { get => _role; set { _role = value; OnPropertyChanged(); } }

        private DateTime _registrationDate = DateTime.Now;
        public DateTime RegistrationDate { get => _registrationDate; set { _registrationDate = value; OnPropertyChanged(); } }

        private string? _pin;
        public string? Pin { get => _pin; set { _pin = value; OnPropertyChanged(); } }

        private string? _password;
        public string? Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        // Recuperación de Contraseña
        public string? SecurityQuestion1 { get; set; }
        public string? SecurityAnswer1 { get; set; }
        public string? SecurityQuestion2 { get; set; }
        public string? SecurityAnswer2 { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
