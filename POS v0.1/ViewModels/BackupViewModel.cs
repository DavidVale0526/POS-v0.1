using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;

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
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string targetDbPath = Path.Combine(baseDir, DbFileName);

                    var dialog = new SaveFileDialog
                    {
                        Filter = "SQLite Database|*.db",
                        FileName = $"pos_backup_{DateTime.Now:yyyyMMdd_HHmm}.db"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        if (File.Exists(targetDbPath))
                        {
                            File.Copy(targetDbPath, dialog.FileName, true);
                        }
                        else
                        {
                            File.Copy(DbFileName, dialog.FileName, true);
                        }
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
                        // Rutas absolutas para evitar confusiones de directorios
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string targetDbPath = Path.Combine(baseDir, DbFileName);
                        string walPath = targetDbPath + "-wal";
                        string shmPath = targetDbPath + "-shm";

                        // 1. Forzar cierre de conexiones de Entity Framework y liberar el pool de SQLite
                        try
                        {
                            _main.Db?.Database.CloseConnection();
                            _main.Db?.Dispose();
                            // Limpiar los pools de conexiones para soltar los archivos físicos
                            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        }
                        catch { }

                        // 2. Forzar recolección de basura para soltar handles pendientes de SQLite
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // 3. Copiar PRIMERO el archivo de respaldo seleccionado sobre la base de datos principal.
                        File.Copy(SelectedFilePath, targetDbPath, true);

                        // 4. Intentar eliminar los archivos auxiliares IGNORANDO por completo cualquier excepción si siguen bloqueados
                        try
                        {
                            if (File.Exists(walPath)) File.Delete(walPath);
                        }
                        catch
                        {
                            // Ignorar error si el archivo está en uso por otro handle secundario de SQLite
                        }

                        try
                        {
                            if (File.Exists(shmPath)) File.Delete(shmPath);
                        }
                        catch
                        {
                            // Ignorar error
                        }

                        System.Windows.MessageBox.Show("Base de datos restaurada correctamente. La aplicación se cerrará ahora.", "Restauración Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error al restaurar: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
