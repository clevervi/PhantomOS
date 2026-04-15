using System;
using System.Management;

namespace PhantomOS.Core
{
    public static class SystemRestoreManager
    {
        /// <summary>
        /// Creates a system restore point if the feature is enabled.
        /// </summary>
        public static bool CreateRestorePoint(string description)
        {
            try
            {
                Logger.Info($"Intentando crear punto de restauración: {description}...");
                
                ManagementScope scope = new ManagementScope("\\\\.\\root\\default");
                ManagementPath path = new ManagementPath("SystemRestore");
                ObjectGetOptions options = new ObjectGetOptions();
                
                using (ManagementClass processClass = new ManagementClass(scope, path, options))
                {
                    ManagementBaseObject inParams = processClass.GetMethodParameters("CreateRestorePoint");
                    inParams["Description"] = description;
                    inParams["RestorePointType"] = 12; // APPLICATION_INSTALL
                    inParams["EventType"] = 100;       // BEGIN_SYSTEM_CHANGE

                    ManagementBaseObject outParams = processClass.InvokeMethod("CreateRestorePoint", inParams, null);
                    uint returnValue = (uint)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        Logger.Info("Punto de restauración creado con éxito.");
                        return true;
                    }
                    else
                    {
                        Logger.Warning($"No se pudo crear el punto de restauración. Código de retorno: {returnValue}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error crítico al crear el punto de restauración", ex);
                return false;
            }
        }
    }
}
