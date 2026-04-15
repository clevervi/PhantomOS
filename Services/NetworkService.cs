using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading.Tasks;
using PhantomOS.Core;

namespace PhantomOS.Services
{
    public class NetworkService
    {
        private static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
        
        public class PingResult
        {
            public string Name { get; set; } = "";
            public long Latency { get; set; } = -1;
            public string Status => Latency < 0 ? "Offline" : $"{Latency}ms";
            public string Color => Latency switch {
                < 0 => "#FF5555",
                < 50 => "#00FF99",
                < 100 => "#FFCC33",
                _ => "#FF5555"
            };
        }

        public async Task<List<PingResult>> GetGamingLatenciesAsync()
        {
            var targets = new Dictionary<string, string>
            {
                { "Google DNS", "8.8.8.8" },
                { "Cloudflare", "1.1.1.1" },
                { "Steam (Valve)", "steampowered.com" },
                { "Epic Games", "epicgames.com" },
                { "Riot Games", "riotgames.com" }
            };

            var results = new List<PingResult>();
            foreach (var target in targets)
            {
                long latency = await PingHostAsync(target.Value);
                results.Add(new PingResult { Name = target.Key, Latency = latency });
            }
            return results;
        }

        private async Task<long> PingHostAsync(string host)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 2000);
                if (reply.Status == IPStatus.Success)
                    return reply.RoundtripTime;
            }
            catch { /* Ignore ping errors */ }
            return -1;
        }

        public async Task<bool> ApplyZeroTelemetryAsync()
        {
            Logger.Info("Iniciando blindaje de Telemetría Zero...");
            bool success = true;

            try
            {
                // 1. Disable DiagTrack Service
                await Task.Run(() => {
                    success &= ServiceManager.SetStartType("DiagTrack", ServiceStartMode.Disabled);
                    success &= ServiceManager.StopService("DiagTrack");
                });

                // 2. Modify Hosts file (Administrative required)
                await Task.Run(() => {
                    var telemetryBlocks = new[]
                    {
                        "0.0.0.0 telemetry.microsoft.com",
                        "0.0.0.0 v10.events.data.microsoft.com",
                        "0.0.0.0 v20.events.data.microsoft.com",
                        "0.0.0.0 browser.events.data.microsoft.com",
                        "0.0.0.0 watson.telemetry.microsoft.com",
                        "0.0.0.0 oca.telemetry.microsoft.com",
                        "0.0.0.0 umwatson.events.data.microsoft.com"
                    };

                    string content = File.ReadAllText(HostsPath);
                    var linesToAdd = telemetryBlocks.Where(b => !content.Contains(b)).ToList();

                    if (linesToAdd.Any())
                    {
                        using var sw = File.AppendText(HostsPath);
                        sw.WriteLine("\n# PhantomOS Zero Telemetry Block");
                        foreach (var line in linesToAdd) sw.WriteLine(line);
                    }
                });

                Logger.Info("¡Blindaje de Telemetría Zero completado con éxito!");
            }
            catch (Exception ex)
            {
                Logger.Error("Error aplicando Telemetría Zero (¿Faltan privilegios de Admin?)", ex);
                success = false;
            }

            return success;
        }
    }
}
