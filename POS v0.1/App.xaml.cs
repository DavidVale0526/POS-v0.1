using System;
using System.IO;
using System.Linq;
using System.Windows;
using POS_v0_1.Services;

namespace POS_v0_1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Captura global de errores no manejados
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
                HandleFatalException(ev.ExceptionObject as Exception, "AppDomain");

            DispatcherUnhandledException += (s, ev) => {
                HandleFatalException(ev.Exception, "Dispatcher");
                ev.Handled = true;
            };
        }

        private void HandleFatalException(Exception ex, string source)
        {
            if (ex == null) return;
            Logger.LogError(ex, $"Crasheo fatal desde {source}");

            MessageBox.Show($"Ocurrió un error inesperado:\n{ex.Message}\n\nEl detalle ha sido registrado en la carpeta de Logs para su revisión técnica.",
                "Error del Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PerformAutoBackup();
            base.OnExit(e);
        }

        private void PerformAutoBackup()
        {
            try
            {
                using (var db = new Data.AppDbContext())
                {
                    var config = db.BusinessConfigs.FirstOrDefault();
                    if (config == null || !config.AutoBackupEnabled) return;

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string backupDir = Path.Combine(baseDir, "Backups");
                    if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);

                    string dbPath = Path.Combine(baseDir, "pos_system.db");
                    if (File.Exists(dbPath))
                    {
                        string fileName = $"auto_backup_{DateTime.Now:yyyyMMdd}.db";
                        File.Copy(dbPath, Path.Combine(backupDir, fileName), true);

                        config.LastBackupDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fallo en Auto-Backup al cerrar");
            }
        }
    }
}
