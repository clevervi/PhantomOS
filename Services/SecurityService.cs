using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Newtonsoft.Json;
using PhantomOS.Core;
using PhantomOS.Models;
using System.Threading.Tasks;

namespace PhantomOS.Services
{
    [SupportedOSPlatform("windows")]
    public class SecurityService
    {
        private readonly string _toolsPath;

        public SecurityService()
        {
            _toolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
            if (!Directory.Exists(_toolsPath)) Directory.CreateDirectory(_toolsPath);
        }

        public async Task<List<SecurityFinding>> RunAuditAsync()
        {
            return await Task.Run(() => RunSecurityAudit());
        }

        public List<SecurityFinding> RunSecurityAudit()
        {
            Logger.Info("Iniciando auditoría de seguridad del sistema...");
            var findings = new List<SecurityFinding>();

            // 1. Perform Native Checks (Built-in)
            findings.AddRange(RunNativeChecks());

            // 2. Perform Seatbelt Checks (If binary exists)
            string seatbeltPath = Path.Combine(_toolsPath, "Seatbelt.exe");
            if (File.Exists(seatbeltPath))
            {
                findings.AddRange(RunSeatbeltScan(seatbeltPath));
            }
            else
            {
                Logger.Warning("Seatbelt.exe no encontrado en /Tools. Usando solo auditoría nativa.");
            }

            return findings;
        }

        public int CalculateScore(List<SecurityFinding> findings)
        {
            if (!findings.Any()) return 100;
            
            // Critical: -25, High: -15, Medium: -5
            int penalty = findings.Sum(f => f.Severity switch
            {
                Severity.Critical => 25,
                Severity.High => 15,
                Severity.Medium => 5,
                _ => 2
            });

            return Math.Max(0, 100 - penalty);
        }

        private List<SecurityFinding> RunNativeChecks()
        {
            var nativeFindings = new List<SecurityFinding>();

            // Check UAC Status
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    var uacValue = key?.GetValue("EnableLUA");
                    if (uacValue == null || (int)uacValue == 0)
                    {
                        nativeFindings.Add(new SecurityFinding
                        {
                            Id = "sec_uac_disabled",
                            Title = "Control de Cuentas de Usuario (UAC) Desactivado",
                            Description = "El UAC está desactivado, lo que permite que el malware se ejecute con privilegios administrativos sin aviso.",
                            Severity = Severity.Critical,
                            Recommendation = "Activar el UAC para prevenir ejecuciones no autorizadas.",
                            RegistryKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            RegistryValueName = "EnableLUA",
                            RegistryValueData = 1
                        });
                    }
                }
            }
            catch (Exception ex) { Logger.Error("Error auditando UAC", ex); }

            // Check Windows Firewall (Simplified)
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"))
                {
                    var fwEnabled = key?.GetValue("EnableFirewall");
                    if (fwEnabled == null || (int)fwEnabled == 0)
                    {
                        nativeFindings.Add(new SecurityFinding
                        {
                            Id = "sec_firewall_off",
                            Title = "Windows Firewall Desactivado",
                            Description = "El cortafuegos del sistema no está protegiendo las conexiones entrantes.",
                            Severity = Severity.High,
                            Recommendation = "Activar el firewall de Windows inmediatamente.",
                            RegistryKey = @"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile",
                            RegistryValueName = "EnableFirewall",
                            RegistryValueData = 1
                        });
                    }
                }
            }
            catch (Exception ex) { Logger.Error("Error auditando Firewall", ex); }

            // Add static "educational" findings if everything is okay
            if (!nativeFindings.Any())
            {
                Logger.Info("No se encontraron vulnerabilidades críticas en la auditoría nativa.");
            }

            return nativeFindings;
        }

        private List<SecurityFinding> RunSeatbeltScan(string path)
        {
            var results = new List<SecurityFinding>();
            try
            {
                Logger.Info("Iniciando escaneo profundo con Seatbelt...");
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-group=system -format=json",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string json = process?.StandardOutput.ReadToEnd() ?? "";
                    process?.WaitForExit();

                    if (!string.IsNullOrEmpty(json))
                    {
                        var output = JsonConvert.DeserializeObject<List<dynamic>>(json); // Seatbelt returns a list of results
                        if (output != null)
                        {
                            foreach (var item in output)
                            {
                                // Logic to map Seatbelt dynamic output to SecurityFinding
                                // e.g., if item.Type == "UAC" and item.EnableLUA == false
                                string type = item.Type?.ToString() ?? "";
                                if (type == "UAC" && item.EnableLUA == false)
                                {
                                    results.Add(new SecurityFinding {
                                        Id = "sb_uac",
                                        Title = "UAC Vulnerability found by Seatbelt",
                                        Description = "Highly critical security risk detected by the deep engine.",
                                        Severity = Severity.Critical
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error en escaneo profundo", ex);
            }
            return results;
        }

        public bool FixFinding(SecurityFinding finding)
        {
            Logger.Info($"Arreglando vulnerabilidad: {finding.Title}...");
            try
            {
                if (!string.IsNullOrEmpty(finding.RegistryKey))
                {
                    return RegistryManager.SetValue(finding.RegistryKey, finding.RegistryValueName!, finding.RegistryValueData!, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al arreglar {finding.Id}", ex);
            }
            return false;
        }
    }
}
