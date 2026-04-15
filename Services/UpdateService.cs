using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PhantomOS.Core;

namespace PhantomOS.Services
{
    public class UpdateService
    {
        private const string RepoUrl = "https://api.github.com/repos/clevervi/PhantomOS/releases/latest";
        private const string CurrentVersion = "0.7.0"; // Current assembly version

        public string LatestVersion { get; private set; } = CurrentVersion;
        public bool UpdateAvailable { get; private set; } = false;

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "PhantomOS-Updater");

                var response = await client.GetStringAsync(RepoUrl);
                using var doc = JsonDocument.Parse(response);
                
                string tagName = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
                LatestVersion = tagName.Replace("v", "");

                if (LatestVersion != CurrentVersion)
                {
                    UpdateAvailable = true;
                    Logger.Info($"[Update] Nueva versión disponible: v{LatestVersion}. Por favor, descarga la última versión de GitHub.");
                }
                else
                {
                    Logger.Info("[Update] PhantomOS está actualizado.");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"No se pudo verificar actualizaciones: {ex.Message}");
            }
        }
    }
}
