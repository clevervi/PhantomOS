using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace PhantomOS.Core
{
    [SupportedOSPlatform("windows")]
    public static class RegistryManager
    {
        private static readonly string BackupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", "registry");

        static RegistryManager()
        {
            if (!Directory.Exists(BackupDir))
            {
                Directory.CreateDirectory(BackupDir);
            }
        }

        /// <summary>
        /// Backs up a registry key to a .reg file.
        /// </summary>
        public static bool BackupKey(string keyPath)
        {
            try
            {
                string safeName = keyPath.Replace("\\", "_").Replace(":", "");
                string fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.reg";
                string fullPath = Path.Combine(BackupDir, fileName);

                Process process = new Process();
                process.StartInfo.FileName = "reg.exe";
                process.StartInfo.Arguments = $"export \"{keyPath}\" \"{fullPath}\" /y";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Backup del registro creado: {fileName}");
                    return true;
                }
                
                Logger.Warning($"Fallo al exportar backup de registro para {keyPath}. Código: {process.ExitCode}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creando backup de registro para {keyPath}", ex);
                return false;
            }
        }

        public static bool SetValue(string keyPath, string valueName, object value, RegistryValueKind kind = RegistryValueKind.DWord)
        {
            try
            {
                string root = keyPath.Split('\\')[0];
                string subKeyPath = keyPath.Substring(root.Length + 1);

                RegistryKey baseKey = root switch
                {
                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                    "HKEY_CURRENT_USER" => Registry.CurrentUser,
                    "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                    "HKEY_USERS" => Registry.Users,
                    _ => throw new ArgumentException($"Raíz de registro no soportada: {root}")
                };

                // Backup before setting if it doesn't exist yet in this session? 
                // Suggestion: Always backup before a "Batch" of changes. 
                // For atomic, we do it here.

                using (RegistryKey key = baseKey.CreateSubKey(subKeyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, kind);
                        Logger.Info($"Valor de registro establecido: {keyPath}\\{valueName} = {value}");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error estableciendo valor de registro {valueName}", ex);
                return false;
            }
        }
    }
}
