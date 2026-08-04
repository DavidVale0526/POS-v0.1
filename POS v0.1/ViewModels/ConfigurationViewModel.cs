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

        // Permisos para el rol "Usuario"
        private bool _allowUserSalesHistory;
        public bool AllowUserSalesHistory { get => _allowUserSalesHistory; set { _allowUserSalesHistory = value; OnPropertyChanged(); } }

        private bool _allowUserInventory;
        public bool AllowUserInventory { get => _allowUserInventory; set { _allowUserInventory = value; OnPropertyChanged(); } }

        private bool _allowUserPurchases;
        public bool AllowUserPurchases { get => _allowUserPurchases; set { _allowUserPurchases = value; OnPropertyChanged(); } }

        private bool _allowUserRepairs;
        public bool AllowUserRepairs { get => _allowUserRepairs; set { _allowUserRepairs = value; OnPropertyChanged(); } }

        private bool _allowUserTreasury;
        public bool AllowUserTreasury { get => _allowUserTreasury; set { _allowUserTreasury = value; OnPropertyChanged(); } }

        private bool _allowUserClients;
        public bool AllowUserClients { get => _allowUserClients; set { _allowUserClients = value; OnPropertyChanged(); } }

        private bool _allowUserReports;
        public bool AllowUserReports { get => _allowUserReports; set { _allowUserReports = value; OnPropertyChanged(); } }

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

                    // Guardar permisos
                    _config.AllowUserSalesHistory = AllowUserSalesHistory;
                    _config.AllowUserInventory = AllowUserInventory;
                    _config.AllowUserPurchases = AllowUserPurchases;
                    _config.AllowUserRepairs = AllowUserRepairs;
                    _config.AllowUserTreasury = AllowUserTreasury;
                    _config.AllowUserClients = AllowUserClients;
                    _config.AllowUserReports = AllowUserReports;

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

                    _main.ShowToast("Configuración guardada correctamente.", "Success");
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

                // Cargar permisos
                AllowUserSalesHistory = _config.AllowUserSalesHistory;
                AllowUserInventory = _config.AllowUserInventory;
                AllowUserPurchases = _config.AllowUserPurchases;
                AllowUserRepairs = _config.AllowUserRepairs;
                AllowUserTreasury = _config.AllowUserTreasury;
                AllowUserClients = _config.AllowUserClients;
                AllowUserReports = _config.AllowUserReports;
            }
            catch { }
        }
    }
}
