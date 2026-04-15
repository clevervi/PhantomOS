using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using PhantomOS.Models;
using PhantomOS.Data;
using PhantomOS.Services;
using PhantomOS.Core;
using System.Collections.Generic;
using System.Windows.Input;

namespace PhantomOS.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly OptimizationService _optimizationService;
        private readonly HardwareService _hardwareService;
        private readonly RecommendationService _recommendationService;
        private readonly SecurityService _securityService;
        private readonly CleanupService _cleanupService;
        private readonly IsoService _isoService;
        
        private string _statusMessage = "Listo";
        private string _systemSpecsSummary = "Cargando...";
        private int _securityScore = 100;
        private readonly SettingsService _settingsService;
        private readonly NetworkService _networkService;
        private readonly UpdateService _updateService;

        private int _totalHealthScore = 100;
        private string _isoWimPath = "";
        private string _isoMountPath = @"C:\PhantomOS_Mount";
        private double _isoProgress = 0;
        private string _isoStatus = "Esperando archivo WIM...";
        private bool _isUpdateAvailable = false;
        private string _newVersionMessage = "";
        private bool _showWizard = false;

        public ObservableCollection<AtomicTweak> Tweaks { get; }
        public ObservableCollection<TweakProfile> Profiles { get; }
        public ObservableCollection<SecurityFinding> SecurityFindings { get; }
        public ObservableCollection<CleanupService.CleanupItem> CleanupItems { get; }
        public ObservableCollection<string> AppLogs { get; } = new();
        public ObservableCollection<NetworkService.PingResult> GamingLatencies { get; } = new();

        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }
        public string SystemSpecsSummary { get => _systemSpecsSummary; set => this.RaiseAndSetIfChanged(ref _systemSpecsSummary, value); }
        public int SecurityScore { get => _securityScore; set => this.RaiseAndSetIfChanged(ref _securityScore, value); }
        public int TotalHealthScore { get => _totalHealthScore; set => this.RaiseAndSetIfChanged(ref _totalHealthScore, value); }
        
        public string IsoWimPath { get => _isoWimPath; set => this.RaiseAndSetIfChanged(ref _isoWimPath, value); }
        public double IsoProgress { get => _isoProgress; set => this.RaiseAndSetIfChanged(ref _isoProgress, value); }
        public string IsoStatus { get => _isoStatus; set => this.RaiseAndSetIfChanged(ref _isoStatus, value); }
        public bool IsUpdateAvailable { get => _isUpdateAvailable; set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value); }
        public string NewVersionMessage { get => _newVersionMessage; set => this.RaiseAndSetIfChanged(ref _newVersionMessage, value); }
        public bool ShowWizard { get => _showWizard; set => this.RaiseAndSetIfChanged(ref _showWizard, value); }

        public ICommand ApplyCommand { get; }
        public ICommand SmartFixCommand { get; }
        public ICommand FixSecurityCommand { get; }
        public ICommand ApplyZeroTelemetryCommand { get; }
        public ICommand ProcessIsoCommand { get; }
        public ICommand RunCleanupCommand { get; }
        public ICommand ApplyWizardProfileCommand { get; }
        public ICommand ResetWizardCommand { get; }

        public MainWindowViewModel()
        {
            _optimizationService = new();
            _hardwareService = new HardwareService();
            _recommendationService = new RecommendationService();
            _securityService = new();
            _cleanupService = new();
            _isoService = new();
            _settingsService = new();
            _networkService = new();
            _updateService = new();

            Tweaks = new();
            Profiles = new();
            SecurityFindings = new();
            CleanupItems = new();

            // Subscribe to Logger
            Logger.OnLog += (msg) => 
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                    AppLogs.Insert(0, msg);
                    if (AppLogs.Count > 100) AppLogs.RemoveAt(100); // Keep last 100
                });
            };

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyTweaksAsync);
            SmartFixCommand = ReactiveCommand.CreateFromTask(RunSmartFixAsync);
            FixSecurityCommand = ReactiveCommand.CreateFromTask(FixSecurityAsync);
            ApplyZeroTelemetryCommand = ReactiveCommand.CreateFromTask(RunZeroTelemetryAsync);
            ProcessIsoCommand = ReactiveCommand.CreateFromTask(RunIsoProcessAsync);
            RunCleanupCommand = ReactiveCommand.CreateFromTask(RunCleanupAsync);
            ApplyWizardProfileCommand = ReactiveCommand.CreateFromTask<string>(ApplyWizardProfileAsync);
            ResetWizardCommand = ReactiveCommand.Create(() => ShowWizard = true);

            _ = AnalyzeAsync();
            _ = StartBackgroundLoops();
        }

        private async Task StartBackgroundLoops()
        {
            // 1. Update Check
            await _updateService.CheckForUpdatesAsync();
            IsUpdateAvailable = _updateService.UpdateAvailable;
            NewVersionMessage = $"Nueva v{_updateService.LatestVersion} disponible en GitHub.";

            // 2. Continuous Latency Monitor (Gaming Ping)
            while (true)
            {
                var pings = await _networkService.GetGamingLatenciesAsync();
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
                    GamingLatencies.Clear();
                    foreach (var p in pings) GamingLatencies.Add(p);
                });
                await Task.Delay(15000); // Every 15 seconds to avoid network noise
            }
        }

        private async Task AnalyzeAsync()
        {
            try
            {
                StatusMessage = "Analizando sistema...";
                Logger.Info("Iniciando análisis profundo de PhantomOS...");
                
                await _settingsService.LoadAsync();
                ShowWizard = _settingsService.Settings.IsFirstRun;

                var hardware = await _hardwareService.GetSpecsAsync();
                var allTweaks = _optimizationService.GetCatalog();
                var findings = await _securityService.RunAuditAsync();
                var cleanupFound = await _cleanupService.ScanAsync();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SystemSpecsSummary = $"{hardware.CpuName} | {hardware.TotalRamGb:F1}GB RAM | {hardware.GpuName}";
                    
                    Tweaks.Clear();
                    foreach (var tweak in allTweaks)
                    {
                        // Restore state from settings
                        if (_settingsService.Settings.AppliedTweakIds.Contains(tweak.Id))
                        {
                            tweak.IsApplied = true;
                        }
                        Tweaks.Add(tweak);
                    }

                    SecurityFindings.Clear();
                    foreach (var f in findings) SecurityFindings.Add(f);
                    SecurityScore = _securityService.CalculateScore(findings);

                    CleanupItems.Clear();
                    foreach (var c in cleanupFound) CleanupItems.Add(c);

                    StatusMessage = $"Análisis completado. Encontrados {cleanupFound.Count} items de limpieza.";
                    CalculateTotalHealth();
                });
            }
            catch (Exception ex) { Logger.Error("Error en análisis", ex); }
        }

        private async Task ApplyTweaksAsync()
        {
            try
            {
                StatusMessage = "Aplicando ajustes...";
                var selected = Tweaks.Where(t => t.IsApplied).ToList();
                
                bool result = await _optimizationService.ApplyBatchAsync(selected);

                if (result)
                {
                    // Save to settings
                    _settingsService.Settings.AppliedTweakIds = selected.Select(s => s.Id).ToList();
                    await _settingsService.SaveAsync();
                    
                    StatusMessage = "Optimización aplicada con éxito. Reinicio recomendado.";
                    CalculateTotalHealth();
                }
            }
            catch (Exception ex) { Logger.Error("Error al aplicar", ex); }
        }

        private void CalculateTotalHealth()
        {
            // Security accounts for 50%, Optimization for 50%
            int securityWeight = SecurityScore / 2;
            
            int appliedTweaks = Tweaks.Count(t => t.IsApplied);
            int recommendedTweaks = Tweaks.Count(t => t.IsRecommended);
            int optWeight = 0;
            if (recommendedTweaks > 0)
                optWeight = (appliedTweaks * 50) / recommendedTweaks;
            else
                optWeight = 50; // Every recommendation followed

            TotalHealthScore = securityWeight + optWeight;
        }

        private async Task RunCleanupAsync()
        {
            StatusMessage = "Limpiando archivos basura...";
            var selected = CleanupItems.Where(i => i.IsChecked).ToList();
            long freed = await _cleanupService.CleanAsync(selected);
            StatusMessage = $"Limpieza completada. Espacio liberado: {freed / 1024 / 1024} MB";
            await AnalyzeAsync(); // Refresh scan
        }

        private async Task RunIsoProcessAsync()
        {
            if (string.IsNullOrEmpty(IsoWimPath)) { StatusMessage = "Error: Selecciona un archivo WIM primero."; return; }
            await _isoService.ProcessWimAsync(IsoWimPath, _isoMountPath, new List<string>(), true);
        }

        private async Task RunSmartFixAsync()
        {
            StatusMessage = "Aplicando Smart Fix...";
            foreach (var tweak in Tweaks) { if (tweak.IsRecommended && tweak.Risk == RiskLevel.Safe) tweak.IsApplied = true; }
            await ApplyTweaksAsync();
        }

        private async Task FixSecurityAsync()
        {
            foreach (var f in SecurityFindings.ToList()) { if (!f.IsFixed) { if (_securityService.FixFinding(f)) f.IsFixed = true; } }
            StatusMessage = "Seguridad reforzada.";
            CalculateTotalHealth();
        }

        private async Task RevertPrivacyAsync()
        {
            foreach (var tweak in Tweaks.Where(t => t.Category == TweakCategory.Privacy)) tweak.IsApplied = false;
            StatusMessage = "Ajustes de privacidad revertidos.";
            await ApplyTweaksAsync();
        }

        private async Task RunZeroTelemetryAsync()
        {
            StatusMessage = "Aplicando blindaje Zero Telemetry...";
            bool success = await _networkService.ApplyZeroTelemetryAsync();
            StatusMessage = success ? "¡Zero Telemetry activado con éxito!" : "Fallo al aplicar Zero Telemetry (Ver Logs).";
            CalculateTotalHealth();
        }

        private async Task ApplyWizardProfileAsync(string profileName)
        {
            StatusMessage = $"Configurando perfil: {profileName}...";
            Logger.Info($"Asistente: Aplicando perfil predefinido '{profileName}'");

            switch (profileName)
            {
                case "Gaming":
                    foreach (var t in Tweaks) if (t.Category == TweakCategory.Performance || t.Category == TweakCategory.System) t.IsApplied = true;
                    break;
                case "Security":
                    foreach (var t in Tweaks) if (t.Category == TweakCategory.Privacy) t.IsApplied = true;
                    await RunZeroTelemetryAsync();
                    break;
                case "Balanced":
                    foreach (var t in Tweaks) if (t.IsRecommended) t.IsApplied = true;
                    break;
            }

            await ApplyTweaksAsync();
            
            _settingsService.Settings.IsFirstRun = false;
            await _settingsService.SaveAsync();
            ShowWizard = false;
            
            StatusMessage = "¡Configuración inicial completada!";
        }
    }
}
