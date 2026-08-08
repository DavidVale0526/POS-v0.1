using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class PasswordRecoveryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private User? _foundUser;

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private bool _step1Visible = true;
        public bool Step1Visible { get => _step1Visible; set { _step1Visible = value; OnPropertyChanged(); } }

        private bool _step2Visible = false;
        public bool Step2Visible { get => _step2Visible; set { _step2Visible = value; OnPropertyChanged(); } }

        private bool _step3Visible = false;
        public bool Step3Visible { get => _step3Visible; set { _step3Visible = value; OnPropertyChanged(); } }

        // Step 2: Questions
        public string Question1 => _foundUser?.SecurityQuestion1 ?? "¿?";
        public string Question2 => _foundUser?.SecurityQuestion2 ?? "¿?";

        private string _answer1;
        public string Answer1 { get => _answer1; set { _answer1 = value; OnPropertyChanged(); } }

        private string _answer2;
        public string Answer2 { get => _answer2; set { _answer2 = value; OnPropertyChanged(); } }

        // Step 3: New Password
        private string _newPassword;
        public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }

        public ICommand CheckEmailCommand { get; }
        public ICommand VerifyAnswersCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand BackCommand { get; }

        public PasswordRecoveryViewModel(MainViewModel main)
        {
            _main = main;

            CheckEmailCommand = new RelayCommand(o =>
            {
                _foundUser = _main.Users.FirstOrDefault(u => u.Email?.Trim().ToLower() == Email?.Trim().ToLower());
                if (_foundUser == null)
                {
                    MessageBox.Show("No se encontró ningún usuario con ese correo.", "Recuperación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_foundUser.SecurityQuestion1))
                {
                    MessageBox.Show("Este usuario no tiene preguntas de seguridad configuradas. Contacta al soporte técnico.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Step1Visible = false;
                Step2Visible = true;
                OnPropertyChanged(nameof(Question1));
                OnPropertyChanged(nameof(Question2));
            });

            VerifyAnswersCommand = new RelayCommand(o =>
            {
                if (_foundUser == null) return;

                if (Answer1?.Trim().ToLower() == _foundUser.SecurityAnswer1 &&
                    Answer2?.Trim().ToLower() == _foundUser.SecurityAnswer2)
                {
                    Step2Visible = false;
                    Step3Visible = true;
                }
                else
                {
                    MessageBox.Show("Las respuestas no coinciden.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ResetPasswordCommand = new RelayCommand(o =>
            {
                if (_foundUser == null || string.IsNullOrWhiteSpace(NewPassword)) return;

                try
                {
                    var dbUser = _main.Db.Users.Find(_foundUser.Id);
                    if (dbUser != null)
                    {
                        dbUser.Password = NewPassword;
                        _main.Db.SaveChanges();

                        // Actualizar en memoria
                        var memUser = _main.Users.FirstOrDefault(u => u.Id == _foundUser.Id);
                        if (memUser != null) memUser.Password = NewPassword;

                        MessageBox.Show("Tu contraseña ha sido actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        _main.CurrentView = new LoginViewModel(_main);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar: " + ex.Message);
                }
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new LoginViewModel(_main));
        }
    }
}
