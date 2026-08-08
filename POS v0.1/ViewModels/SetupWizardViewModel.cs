using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class SetupWizardViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // Business Info
        private string _businessName;
        public string BusinessName { get => _businessName; set { _businessName = value; OnPropertyChanged(); } }

        // Admin User Info
        private string _adminName;
        public string AdminName { get => _adminName; set { _adminName = value; OnPropertyChanged(); } }

        private string _adminEmail;
        public string AdminEmail { get => _adminEmail; set { _adminEmail = value; OnPropertyChanged(); } }

        private string _adminPassword;
        public string AdminPassword { get => _adminPassword; set { _adminPassword = value; OnPropertyChanged(); } }

        private string _adminPin;
        public string AdminPin { get => _adminPin; set { _adminPin = value; OnPropertyChanged(); } }

        // Security Questions
        public System.Collections.ObjectModel.ObservableCollection<string> SecurityQuestions { get; } = new System.Collections.ObjectModel.ObservableCollection<string>
        {
            "¿Nombre de tu primera mascota?",
            "¿Ciudad donde naciste?",
            "¿Nombre de tu escuela primaria?",
            "¿Marca de tu primer auto?",
            "¿Nombre de tu abuelo materno?"
        };

        private string _selectedQuestion1;
        public string SelectedQuestion1 { get => _selectedQuestion1; set { _selectedQuestion1 = value; OnPropertyChanged(); } }

        private string _answer1;
        public string Answer1 { get => _answer1; set { _answer1 = value; OnPropertyChanged(); } }

        private string _selectedQuestion2;
        public string SelectedQuestion2 { get => _selectedQuestion2; set { _selectedQuestion2 = value; OnPropertyChanged(); } }

        private string _answer2;
        public string Answer2 { get => _answer2; set { _answer2 = value; OnPropertyChanged(); } }

        public ICommand FinishSetupCommand { get; }

        public SetupWizardViewModel(MainViewModel main)
        {
            _main = main;
            SelectedQuestion1 = SecurityQuestions[0];
            SelectedQuestion2 = SecurityQuestions[1];
            FinishSetupCommand = new RelayCommand(o => FinishSetup());
        }

        private void FinishSetup()
        {
            if (string.IsNullOrWhiteSpace(BusinessName) || string.IsNullOrWhiteSpace(AdminName) ||
                string.IsNullOrWhiteSpace(AdminEmail) || string.IsNullOrWhiteSpace(AdminPassword) ||
                string.IsNullOrWhiteSpace(AdminPin) || string.IsNullOrWhiteSpace(Answer1) || string.IsNullOrWhiteSpace(Answer2))
            {
                MessageBox.Show("Por favor complete todos los campos obligatorios, incluyendo las preguntas de seguridad.", "Asistente de Configuración", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AdminPin.Length != 4 || !AdminPin.All(char.IsDigit))
            {
                MessageBox.Show("El PIN debe ser de exactamente 4 números.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Crear el primer administrador
                var admin = new User
                {
                    Name = AdminName,
                    Email = AdminEmail,
                    Password = AdminPassword,
                    Pin = AdminPin,
                    Role = "Administrador",
                    RegistrationDate = DateTime.Now,
                    SecurityQuestion1 = SelectedQuestion1,
                    SecurityAnswer1 = Answer1.Trim().ToLower(),
                    SecurityQuestion2 = SelectedQuestion2,
                    SecurityAnswer2 = Answer2.Trim().ToLower()
                };

                _main.Db.Users.Add(admin);

                // 2. Configurar el negocio
                var config = _main.Db.BusinessConfigs.FirstOrDefault() ?? new BusinessConfiguration();
                config.BusinessName = BusinessName;

                if (config.Id == 0) _main.Db.BusinessConfigs.Add(config);
                else _main.Db.BusinessConfigs.Update(config);

                _main.Db.SaveChanges();

                // 3. Actualizar estado global
                _main.Users.Add(admin);
                _main.BusinessConfig = config;

                MessageBox.Show("¡Configuración completada con éxito! Ahora puedes iniciar sesión.", "Bienvenido", MessageBoxButton.OK, MessageBoxImage.Information);

                // 4. Mandar al Login
                _main.CurrentView = new LoginViewModel(_main);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración inicial: {ex.Message}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
