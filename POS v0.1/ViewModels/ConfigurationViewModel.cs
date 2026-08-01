using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private BusinessConfiguration _config;

        private string _businessName = "Mi Tienda";
        public string BusinessName
        {
            get => _businessName;
            set { _businessName = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private string _logoPath;
        public string LogoPath
        {
            get => _logoPath;
            set { _logoPath = value; OnPropertyChanged(); }
        }

        private string _ticketFormat = "58mm";
        public string TicketFormat
        {
            get => _ticketFormat;
            set { _ticketFormat = value; OnPropertyChanged(); }
        }

        private decimal _taxRate;
        public decimal TaxRate
        {
            get => _taxRate;
            set { _taxRate = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand ChooseLogoCommand { get; }
        public ICommand ShowBackupCommand { get; }

        public ConfigurationViewModel(MainViewModel main)
        {
            _main = main;
            LoadConfig();

            SaveCommand = new RelayCommand(o =>
            {
                try
                {
                    // Validación de Email
                    if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains("@"))
                    {
                        System.Windows.MessageBox.Show("El correo electrónico no es válido. Debe contener un '@'.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    _config.BusinessName = BusinessName ?? "Mi Tienda";
                    _config.Phone = Phone ?? "";
                    _config.Email = Email ?? "";
                    _config.Address = Address ?? "";
                    _config.LogoPath = LogoPath ?? "";
                    _config.TicketFormat = TicketFormat ?? "58mm";
                    _config.TaxRate = TaxRate;

                    if (_config.Id == 0)
                    {
                        _main.Db.BusinessConfigs.Add(_config);
                    }
                    else
                    {
                        _main.Db.BusinessConfigs.Update(_config);
                    }

                    _main.Db.SaveChanges();

                    // Actualizar en el MainViewModel para que se refleje en la vista principal
                    _main.BusinessConfig = _config;

                    System.Windows.MessageBox.Show("Configuración guardada correctamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    var innerMessage = ex.InnerException != null ? $"\nDetalle: {ex.InnerException.Message}" : "";
                    System.Windows.MessageBox.Show($"Error al guardar: {ex.Message}{innerMessage}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            });

            ChooseLogoCommand = new RelayCommand(o =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                };
                if (dialog.ShowDialog() == true)
                {
                    LogoPath = dialog.FileName;
                }
            });

            ShowBackupCommand = new RelayCommand(o =>
            {
                _main.CurrentView = new BackupViewModel(_main);
            });
        }

        private void LoadConfig()
        {
            try
            {
                _config = _main.Db.BusinessConfigs.FirstOrDefault() ?? new BusinessConfiguration();

                BusinessName = _config.BusinessName;
                Phone = _config.Phone;
                Email = _config.Email;
                Address = _config.Address;
                LogoPath = _config.LogoPath;
                TicketFormat = _config.TicketFormat;
                TaxRate = _config.TaxRate;
            }
            catch { }
        }
    }
}
