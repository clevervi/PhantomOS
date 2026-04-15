using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using PhantomOS.Core;

namespace PhantomOS.Services
{
    public class IsoService
    {
        public delegate void ProgressHandler(string message, double percentage);
        public event ProgressHandler? OnProgress;

        public async Task<bool> ProcessWimAsync(string wimPath, string mountPath, List<string> tweaksToApply, bool debloat)
        {
            try
            {
                if (!Directory.Exists(mountPath)) Directory.CreateDirectory(mountPath);

                // 1. Mount Image
                UpdateProgress("Montando imagen WIM (esto puede tardar)...", 10);
                if (!await RunDismAsync($"/Mount-Image /ImageFile:\"{wimPath}\" /Index:1 /MountDir:\"{mountPath}\""))
                    return false;

                // 2. Apply Registry Tweaks (Offline)
                UpdateProgress("Cargando registro offline y aplicando ajustes...", 40);
                await ApplyOfflineRegistryTweaks(mountPath, tweaksToApply);

                // 3. Debloat (Provisioned Packages)
                if (debloat)
                {
                    UpdateProgress("Eliminando aplicaciones preinstaladas (Debloat)...", 70);
                    await RunDebloatAsync(mountPath);
                }

                // 4. Unmount and Commit
                UpdateProgress("Guardando cambios y desmontando imagen...", 90);
                if (!await RunDismAsync($"/Unmount-Image /MountDir:\"{mountPath}\" /Commit"))
                    return false;

                UpdateProgress("¡Imagen WIM procesada con éxito!", 100);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error procesando WIM", ex);
                return false;
            }
        }

        private async Task ApplyOfflineRegistryTweaks(string mountPath, List<string> tweakIds)
        {
            string softwareHive = Path.Combine(mountPath, @"Windows\System32\config\SOFTWARE");
            
            // Load Hive
            await RunRegAsync($"load HKLM\\OFFLINE_SOFTWARE \"{softwareHive}\"");

            // Example: Disable Telemetry in offline hive
            await RunRegAsync("add HKLM\\OFFLINE_SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection /v AllowTelemetry /t REG_DWORD /d 0 /f");
            
            // Unload Hive
            await RunRegAsync("unload HKLM\\OFFLINE_SOFTWARE");
        }

        private async Task RunDebloatAsync(string mountPath)
        {
            string[] appsToRemove = {
                "Microsoft.Clipchamp",
                "Microsoft.BingNews",
                "Microsoft.GetHelp",
                "Microsoft.YourPhone",
                "Microsoft.GamingApp"
            };

            foreach (var app in appsToRemove)
            {
                await RunDismAsync($"/Image:\"{mountPath}\" /Remove-ProvisionedAppxPackage /PackageName:{app}");
            }
        }

        private async Task<bool> RunDismAsync(string args)
        {
            return await RunProcessAsync("dism.exe", args);
        }

        private async Task<bool> RunRegAsync(string args)
        {
            return await RunProcessAsync("reg.exe", args);
        }

        private async Task<bool> RunProcessAsync(string fileName, string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null) return false;
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }

        private void UpdateProgress(string message, double percentage)
        {
            OnProgress?.Invoke(message, percentage);
        }
    }
}
