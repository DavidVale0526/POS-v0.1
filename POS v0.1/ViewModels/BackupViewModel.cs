using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;

namespace POS_v0_1.ViewModels
{
    public class BackupViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private const string DbFileName = "pos_system.db";

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
                try
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "SQLite Database|*.db",
                        FileName = $"pos_backup_{DateTime.Now:yyyyMMdd_HHmm}.db"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        File.Copy(DbFileName, dialog.FileName, true);
                        System.Windows.MessageBox.Show("Copia de seguridad generada con éxito.", "Respaldo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al exportar: {ex.Message}");
                }
            });

            ImportCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrEmpty(SelectedFilePath))
                {
                    System.Windows.MessageBox.Show("Por favor, seleccione un archivo de respaldo.");
                    return;
                }

                var result = System.Windows.MessageBox.Show("¡ADVERTENCIA! Esta acción reemplazará todos los datos actuales y cerrará la aplicación para aplicar los cambios. ¿Desea continuar?",
                    "Confirmar Restauración", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        // Cerramos la conexión a la base de datos si fuera necesario (aunque SQLite suele permitir el reemplazo del archivo si no hay handles abiertos)
                        // Para asegurar, cerramos la aplicación después del reemplazo.

                        File.Copy(SelectedFilePath, DbFileName, true);

                        System.Windows.MessageBox.Show("Base de datos restaurada. La aplicación se cerrará ahora.", "Restauración Exitosa");
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error al restaurar: {ex.Message}");
                    }
                }
            });

            ChooseFileCommand = new RelayCommand(o =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "SQLite Database|*.db"
                };

                if (dialog.ShowDialog() == true)
                {
                    SelectedFilePath = dialog.FileName;
                }
            });

            BackCommand = new RelayCommand(o =>
            {
                _main.CurrentView = new ConfigurationViewModel(_main);
            });
        }
    }
}
