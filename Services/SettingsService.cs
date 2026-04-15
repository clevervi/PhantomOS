using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PhantomOS.Core;
using PhantomOS.Models;

namespace PhantomOS.Services
{
    public class SettingsService
    {
        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhantomOS");
        private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

        public PhantomSettings Settings { get; private set; } = new();

        public async Task LoadAsync()
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

                if (File.Exists(FilePath))
                {
                    string json = await File.ReadAllTextAsync(FilePath);
                    Settings = JsonSerializer.Deserialize<PhantomSettings>(json) ?? new();
                    Logger.Info("Configuración cargada correctamente.");
                }
                else
                {
                    Logger.Info("No se encontró configuración previa. Usando valores predeterminados.");
                    await SaveAsync(); // Create initial file
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al cargar la configuración", ex);
                Settings = new();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Settings, options);
                await File.WriteAllTextAsync(FilePath, json);
                Logger.Info("Configuración guardada en disco.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error al guardar la configuración", ex);
            }
        }
    }
}
