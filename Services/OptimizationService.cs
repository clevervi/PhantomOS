using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using PhantomOS.Core;
using PhantomOS.Models;
using PhantomOS.Data;

namespace PhantomOS.Services
{
    [SupportedOSPlatform("windows")]
    public class OptimizationService
    {
        private static readonly string ReportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");

        public OptimizationService()
        {
            if (!Directory.Exists(ReportsDir))
            {
                Directory.CreateDirectory(ReportsDir);
            }
        }

        public List<AtomicTweak> GetCatalog()
        {
            return TweakCatalog.Tweaks;
        }

        public void CheckTweakStatus(AtomicTweak tweak)
        {
            try
            {
                if (!string.IsNullOrEmpty(tweak.RegistryKey))
                {
                    string root = tweak.RegistryKey.Split('\\')[0];
                    string subKeyPath = tweak.RegistryKey.Substring(root.Length + 1);
                    RegistryKey? baseKey = GetBaseKey(root);

                    using (RegistryKey? key = baseKey?.OpenSubKey(subKeyPath))
                    {
                        if (key != null)
                        {
                            object? value = key.GetValue(tweak.RegistryValueName);
                            if (value != null)
                            {
                                // Simple comparison for DWord or numeric
                                tweak.IsApplied = value.ToString() == tweak.RegistryValueData?.ToString();
                                return;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(tweak.ServiceName))
                {
                    using (ServiceController sc = new ServiceController(tweak.ServiceName))
                    {
                        tweak.IsApplied = sc.StartType == ServiceStartMode.Disabled || sc.Status == ServiceControllerStatus.Stopped;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error verificando estado de {tweak.Name}", ex);
            }
        }

        public bool ApplyTweak(AtomicTweak tweak)
        {
            Logger.Info($"Aplicando ajuste: {tweak.Name}...");
            bool success = false;

            // 1. Registry backup and set
            if (!string.IsNullOrEmpty(tweak.RegistryKey))
            {
                RegistryManager.BackupKey(tweak.RegistryKey);
                success = RegistryManager.SetValue(tweak.RegistryKey, tweak.RegistryValueName!, tweak.RegistryValueData!, tweak.RegistryValueKind);
            }
            // 2. Service handling
            else if (!string.IsNullOrEmpty(tweak.ServiceName))
            {
                success = ServiceManager.SetStartType(tweak.ServiceName, ServiceStartMode.Disabled) && 
                          ServiceManager.StopService(tweak.ServiceName);
            }
            // 3. Scheduled Task handling
            else if (!string.IsNullOrEmpty(tweak.ScheduledTaskPath))
            {
                success = DisableScheduledTask(tweak.ScheduledTaskPath);
            }

            if (success) tweak.IsApplied = true;
            return success;
        }

        public async Task<bool> ApplyBatchAsync(List<AtomicTweak> tweaks)
        {
            bool allSuccess = true;
            await Task.Run(() =>
            {
                foreach (var tweak in tweaks)
                {
                    if (!ApplyTweak(tweak))
                    {
                        allSuccess = false;
                        Logger.Warning($"No se pudo aplicar el ajuste: {tweak.Name}");
                    }
                }
            });
            return allSuccess;
        }

        private bool DisableScheduledTask(string taskPath)
        {
            try
            {
                Logger.Info($"Desactivando tarea programada: {taskPath}");
                var startInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Change /TN \"{taskPath}\" /Disable",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                    return process?.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error desactivando tarea {taskPath}", ex);
                return false;
            }
        }

        public void RestartExplorer()
        {
            try
            {
                Logger.Info("Reiniciando explorer.exe para aplicar cambios visuales...");
                foreach (var process in Process.GetProcessesByName("explorer"))
                {
                    process.Kill();
                    process.WaitForExit();
                }
                // Windows automatically restarts explorer if killed, but we can call it just in case
                Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                Logger.Error("Fallo al reiniciar explorer.exe", ex);
            }
        }

        public string GenerateReport(List<AtomicTweak> appliedTweaks)
        {
            var report = new
            {
                Timestamp = DateTime.Now,
                TotalApplied = appliedTweaks.Count,
                Tweaks = appliedTweaks.Select(t => new { t.Id, t.Name, t.Category })
            };

            string fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(ReportsDir, fileName);
            string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(path, json);
            Logger.Info($"Reporte generado exitosamente: {fileName}");
            return path;
        }

        private RegistryKey? GetBaseKey(string root)
        {
            return root switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                _ => null
            };
        }
    }
}
