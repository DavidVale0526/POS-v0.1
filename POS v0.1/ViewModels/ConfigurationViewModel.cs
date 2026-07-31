using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace POS_v0_1.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

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

            SaveCommand = new RelayCommand(o =>
            {
                // Lógica de guardado se implementará después
                System.Windows.MessageBox.Show("Configuración guardada (Simulado)");
            });

            ChooseLogoCommand = new RelayCommand(o =>
            {
                // Lógica de selección de imagen
            });

            ShowBackupCommand = new RelayCommand(o =>
            {
                _main.CurrentView = new BackupViewModel(_main);
            });
        }
    }
}
