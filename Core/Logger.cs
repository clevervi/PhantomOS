using System;
using System.IO;

namespace PhantomOS.Core
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "phantom_os.log");

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllLines(LogFilePath, new[] { logLine });
                
                // Also write to console for debug
                Console.WriteLine(logLine);
            }
            catch
            {
                // Last resort: failsafe
            }
        }

        public static void Info(string message) => Log(message, "INFO");
        public static void Warning(string message) => Log(message, "WARN");
        public static void Error(string message, Exception ex = null) 
        {
            string fullMessage = ex != null ? $"{message} | Exception: {ex.Message}" : message;
            Log(fullMessage, "ERROR");
        }
    }
}
