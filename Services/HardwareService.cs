using System;
using System.Management;
using System.Runtime.Versioning;
using PhantomOS.Core;
using PhantomOS.Models;
using System.Threading.Tasks;

namespace PhantomOS.Services
{
    [SupportedOSPlatform("windows")]
    public class HardwareService
    {
        public async Task<HardwareInfo> GetSpecsAsync()
        {
            return await Task.Run(() => GetSystemInfo());
        }

        public HardwareInfo GetSystemInfo()
        {
            var info = new HardwareInfo();

            try
            {
                // 1. CPU Info
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        info.CpuName = obj["Name"]?.ToString() ?? "Unknown";
                        info.CpuCores = (uint)obj["NumberOfCores"];
                        info.CpuThreads = (uint)obj["ThreadCount"];
                        info.CpuArchitecture = obj["Architecture"]?.ToString() == "9" ? "x64" : "x86";
                    }
                }

                // 2. RAM Info
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        ulong totalRamKb = (ulong)obj["TotalVisibleMemorySize"];
                        info.TotalRamGb = totalRamKb / (1024.0 * 1024.0);
                    }
                }

                // 3. GPU Info
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        info.GpuName = obj["Name"]?.ToString() ?? "Unknown GPU";
                        break; // Get the primary one
                    }
                }

                // 4. Disk Info (SSD vs HDD)
                // Note: Requires root\Microsoft\Windows\Storage
                try
                {
                    var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
                    scope.Connect();
                    var query = new ObjectQuery("SELECT FriendlyName, MediaType FROM MSFT_PhysicalDisk");
                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            info.DiskModel = obj["FriendlyName"]?.ToString() ?? "";
                            // 3 = HDD, 4 = SSD
                            var mediaType = obj["MediaType"]?.ToString();
                            if (mediaType == "4") info.IsSSD = true;
                        }
                    }
                }
                catch (Exception diskEx)
                {
                    Logger.Warning($"No se pudo determinar el tipo de disco mediante MSFT_PhysicalDisk: {diskEx.Message}");
                    // Fallback to a simpler check or assume HDD for safety
                }

                // 5. Laptop Detection
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    info.IsLaptop = searcher.Get().Count > 0;
                }

                info.OsVersion = Environment.OSVersion.ToString();

                Logger.Info($"Análisis de hardware completado: {info.DetailedSummary}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error recolectando información de hardware", ex);
            }

            return info;
        }
    }
}
