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
        private int _totalHealthScore = 100;
        private string _isoWimPath = "";
        private string _isoMountPath = @"C:\PhantomOS_Mount";
        private double _isoProgress = 0;
        private string _isoStatus = "Esperando archivo WIM...";

        public ObservableCollection<AtomicTweak> Tweaks { get; }
        public ObservableCollection<TweakProfile> Profiles { get; }
        public ObservableCollection<SecurityFinding> SecurityFindings { get; }
        public ObservableCollection<CleanupService.CleanupItem> CleanupItems { get; }

        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }
        public string SystemSpecsSummary { get => _systemSpecsSummary; set => this.RaiseAndSetIfChanged(ref _systemSpecsSummary, value); }
        public int SecurityScore { get => _securityScore; set => this.RaiseAndSetIfChanged(ref _securityScore, value); }
        public int TotalHealthScore { get => _totalHealthScore; set => this.RaiseAndSetIfChanged(ref _totalHealthScore, value); }
        
        public string IsoWimPath { get => _isoWimPath; set => this.RaiseAndSetIfChanged(ref _isoWimPath, value); }
        public double IsoProgress { get => _isoProgress; set => this.RaiseAndSetIfChanged(ref _isoProgress, value); }
        public string IsoStatus { get => _isoStatus; set => this.RaiseAndSetIfChanged(ref _isoStatus, value); }

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<Unit, Unit> SmartFixCommand { get; }
        public ReactiveCommand<Unit, Unit> FixSecurityCommand { get; }
        public ReactiveCommand<Unit, Unit> RevertPrivacyCommand { get; }
        public ReactiveCommand<Unit, Unit> RunCleanupCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessIsoCommand { get; }
        public ReactiveCommand<TweakProfile, Unit> SelectProfileCommand { get; }

        public MainWindowViewModel()
        {
            _optimizationService = new OptimizationService();
            _hardwareService = new HardwareService();
            _recommendationService = new RecommendationService();
            _securityService = new SecurityService();
            _cleanupService = new CleanupService();
            _isoService = new IsoService();

            Tweaks = new ObservableCollection<AtomicTweak>(TweakCatalog.Tweaks);
            Profiles = new ObservableCollection<TweakProfile>(TweakCatalog.Profiles);
            SecurityFindings = new ObservableCollection<SecurityFinding>();
            CleanupItems = new ObservableCollection<CleanupService.CleanupItem>();

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            SmartFixCommand = ReactiveCommand.CreateFromTask(ApplySmartFixAsync);
            FixSecurityCommand = ReactiveCommand.CreateFromTask(FixSecurityFindingsAsync);
            RevertPrivacyCommand = ReactiveCommand.CreateFromTask(RevertPrivacyAsync);
            RunCleanupCommand = ReactiveCommand.CreateFromTask(RunCleanupAsync);
            ProcessIsoCommand = ReactiveCommand.CreateFromTask(ProcessIsoAsync);
            SelectProfileCommand = ReactiveCommand.Create<TweakProfile>(SelectProfile);

            _isoService.OnProgress += (msg, pct) => { IsoStatus = msg; IsoProgress = pct; };

            _ = Task.Run(RunFullAnalysisAsync);
        }

        private async Task RunFullAnalysisAsync()
        {
            try
            {
                // 1. Hardware & Security
                _hardwareService.GetSystemInfo();
                var secFindings = _securityService.RunSecurityAudit();
                
                // 2. Cleanup Scan (Automatic as requested)
                var cleanupFound = await _cleanupService.ScanAsync();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SecurityFindings.Clear();
                    foreach (var f in secFindings) SecurityFindings.Add(f);
                    
                    CleanupItems.Clear();
                    foreach (var c in cleanupFound) CleanupItems.Add(c);

                    StatusMessage = $"Análisis completado. Encontrados {cleanupFound.Count} items de limpieza.";
                    CalculateTotalHealth();
                });
            }
            catch (Exception ex) { Logger.Error("Error en análisis", ex); }
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
            await RunFullAnalysisAsync(); // Refresh scan
        }

        private async Task ProcessIsoAsync()
        {
            if (string.IsNullOrEmpty(IsoWimPath)) { StatusMessage = "Error: Selecciona un archivo WIM primero."; return; }
            await _isoService.ProcessWimAsync(IsoWimPath, _isoMountPath, new List<string>(), true);
        }

        // ... existing methods (ApplyChangesAsync, SelectProfile, etc.)
        private async Task ApplyChangesAsync()
        {
            StatusMessage = "Iniciando optimización...";
            await Task.Run(() =>
            {
                SystemRestoreManager.CreateRestorePoint("PhantomOS Cleanup Session");
                var toApply = Tweaks.Where(t => t.IsApplied).ToList();
                foreach (var tweak in toApply) _optimizationService.ApplyTweak(tweak);
                StatusMessage = "¡Optimización completada con éxito!";
            });
        }

        private void SelectProfile(TweakProfile profile)
        {
            foreach (var tweak in Tweaks) tweak.IsApplied = profile.TweakIds.Contains(tweak.Id);
        }

        private async Task ApplySmartFixAsync()
        {
            StatusMessage = "Aplicando Smart Fix...";
            foreach (var tweak in Tweaks) { if (tweak.IsRecommended && tweak.Risk == RiskLevel.Safe) tweak.IsApplied = true; }
            await ApplyChangesAsync();
        }

        private async Task FixSecurityFindingsAsync()
        {
            foreach (var f in SecurityFindings.ToList()) { if (!f.IsFixed) { if (_securityService.FixFinding(f)) f.IsFixed = true; } }
            StatusMessage = "Seguridad reforzada.";
        }

        private async Task RevertPrivacyAsync()
        {
            foreach (var tweak in Tweaks.Where(t => t.Category == TweakCategory.Privacy)) tweak.IsApplied = false;
            StatusMessage = "Ajustes de privacidad revertidos.";
        }
    }
}
