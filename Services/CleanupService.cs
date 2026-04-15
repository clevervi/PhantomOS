using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhantomOS.Core;

namespace PhantomOS.Services
{
    public class CleanupService
    {
        public class CleanupItem
        {
            public string Name { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public long Size { get; set; }
            public bool IsChecked { get; set; }
        }

        private readonly List<string> _cleanupPaths = new List<string>
        {
            Path.GetTempPath(),                                  // %TEMP%
            @"C:\Windows\Temp",                                  // Windows Temp
            @"C:\Windows\Prefetch",                              // Prefetch
            @"C:\Windows\SoftwareDistribution\Download",         // Windows Update Cache
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer") // Icon Cache
        };

        public async Task<List<CleanupItem>> ScanAsync()
        {
            Logger.Info("Iniciando escaneo de archivos basura...");
            var items = new List<CleanupItem>();

            foreach (var path in _cleanupPaths)
            {
                if (Directory.Exists(path))
                {
                    long size = await Task.Run(() => GetDirectorySize(path));
                    items.Add(new CleanupItem
                    {
                        Name = GetFriendlyName(path),
                        Path = path,
                        Size = size,
                        IsChecked = size > 0
                    });
                }
            }

            return items;
        }

        public async Task<long> CleanAsync(List<CleanupItem> selectedItems)
        {
            long totalFreed = 0;
            foreach (var item in selectedItems)
            {
                Logger.Info($"Limpiando: {item.Name}...");
                totalFreed += await Task.Run(() => DeleteDirectoryContents(item.Path));
            }
            return totalFreed;
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch { return 0; }
        }

        private long DeleteDirectoryContents(string path)
        {
            long freed = 0;
            var di = new DirectoryInfo(path);

            foreach (FileInfo file in di.EnumerateFiles())
            {
                try
                {
                    long size = file.Length;
                    file.Delete();
                    freed += size;
                }
                catch { /* File in use */ }
            }

            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                try
                {
                    freed += GetDirectorySize(dir.FullName);
                    dir.Delete(true);
                }
                catch { /* Directory in use */ }
            }

            return freed;
        }

        private string GetFriendlyName(string path)
        {
            if (path.Contains("SoftwareDistribution")) return "Cache de Windows Update";
            if (path.Contains("Prefetch")) return "Archivos Prefetch (Arranque)";
            if (path.Contains("Windows\\Temp")) return "Archivos Temporales del Sistema";
            if (path.EndsWith("Local\\Temp")) return "Archivos Temporales del Usuario";
            if (path.Contains("Explorer")) return "Caché de Iconos y Miniaturas";
            return "Carpeta Temporal";
        }
    }
}
