using System;
using System.Windows.Input;

namespace POS_v0_1.ViewModels
{
    public class BackupViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ChooseFileCommand { get; }
        public ICommand BackCommand { get; }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set { _selectedFilePath = value; OnPropertyChanged(); }
        }

        public BackupViewModel(MainViewModel main)
        {
            _main = main;

            ExportCommand = new RelayCommand(o =>
            {
                // Lógica de exportación SQLite se implementará después
                System.Windows.MessageBox.Show("Copia de seguridad generada (Simulado)");
            });

            ImportCommand = new RelayCommand(o =>
            {
                // Lógica de restauración SQLite se implementará después
                System.Windows.MessageBox.Show("Restauración completada (Simulado)");
            });

            ChooseFileCommand = new RelayCommand(o =>
            {
                // Lógica para abrir buscador de archivos
            });

            BackCommand = new RelayCommand(o =>
            {
                _main.CurrentView = new ConfigurationViewModel(_main);
            });
        }
    }
}
