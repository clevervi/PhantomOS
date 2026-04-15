using System;
using System.ServiceProcess;
using System.Runtime.Versioning;

namespace PhantomOS.Core
{
    [SupportedOSPlatform("windows")]
    public static class ServiceManager
    {
        public static bool StopService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
                    {
                        Logger.Info($"Deteniendo servicio: {serviceName}");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        return true;
                    }
                    Logger.Info($"El servicio {serviceName} ya está detenido.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al detener el servicio {serviceName}", ex);
                return false;
            }
        }

        public static bool SetStartType(string serviceName, ServiceStartMode startMode)
        {
            try
            {
                // Note: Changing start type requires calling ChangeServiceConfig via P/Invoke or using sc.exe
                // ServiceController doesn't have a direct property for StartType that is writable in all .NET versions.
                // We'll use sc.exe for simplicity and reliability.
                string mode = startMode switch
                {
                    ServiceStartMode.Automatic => "auto",
                    ServiceStartMode.Disabled => "disabled",
                    ServiceStartMode.Manual => "demand",
                    _ => "demand"
                };

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "sc.exe";
                process.StartInfo.Arguments = $"config \"{serviceName}\" start= {mode}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Tipo de inicio de {serviceName} cambiado a {mode}");
                    return true;
                }
                
                Logger.Warning($"Fallo al cambiar tipo de inicio de {serviceName}. Código: {process.ExitCode}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al configurar tipo de inicio para {serviceName}", ex);
                return false;
            }
        }
    }
}
