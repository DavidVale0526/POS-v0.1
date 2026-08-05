using System;
using System.IO;

namespace POS_v0_1.Services
{
    public static class Logger
    {
        private static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                if (!Directory.Exists(LogFolder)) Directory.CreateDirectory(LogFolder);

                string logFile = Path.Combine(LogFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
                string message = $"[{DateTime.Now:HH:mm:ss}] ERROR: {context}\n" +
                                 $"Message: {ex.Message}\n" +
                                 $"StackTrace: {ex.StackTrace}\n" +
                                 (ex.InnerException != null ? $"Inner: {ex.InnerException.Message}\n" : "") +
                                 new string('-', 50) + "\n";

                File.AppendAllText(logFile, message);
            }
            catch
            {
                // No podemos loguear un fallo en el logger a disco, así que fallamos en silencio
            }
        }

        public static void LogInfo(string info)
        {
            try
            {
                if (!Directory.Exists(LogFolder)) Directory.CreateDirectory(LogFolder);
                string logFile = Path.Combine(LogFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] INFO: {info}\n");
            }
            catch { }
        }
    }
}
