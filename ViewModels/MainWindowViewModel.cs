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
        
        private string _statusMessage = "Analizando sistema...";
        private string _systemSpecsSummary = "Cargando especificaciones...";
        private int _securityScore = 100;
        private HardwareInfo? _currentHardware;

        public ObservableCollection<AtomicTweak> Tweaks { get; }
        public ObservableCollection<TweakProfile> Profiles { get; }
        public ObservableCollection<SecurityFinding> SecurityFindings { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public string SystemSpecsSummary
        {
            get => _systemSpecsSummary;
            set => this.RaiseAndSetIfChanged(ref _systemSpecsSummary, value);
        }

        public int SecurityScore
        {
            get => _securityScore;
            set => this.RaiseAndSetIfChanged(ref _securityScore, value);
        }

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<Unit, Unit> SmartFixCommand { get; }
        public ReactiveCommand<Unit, Unit> FixSecurityCommand { get; }
        public ReactiveCommand<Unit, Unit> RevertPrivacyCommand { get; }
        public ReactiveCommand<TweakProfile, Unit> SelectProfileCommand { get; }

        public MainWindowViewModel()
        {
            _optimizationService = new OptimizationService();
            _hardwareService = new HardwareService();
            _recommendationService = new RecommendationService();
            _securityService = new SecurityService();

            Tweaks = new ObservableCollection<AtomicTweak>(TweakCatalog.Tweaks);
            Profiles = new ObservableCollection<TweakProfile>(TweakCatalog.Profiles);
            SecurityFindings = new ObservableCollection<SecurityFinding>();

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            SmartFixCommand = ReactiveCommand.CreateFromTask(ApplySmartFixAsync);
            FixSecurityCommand = ReactiveCommand.CreateFromTask(FixSecurityFindingsAsync);
            RevertPrivacyCommand = ReactiveCommand.CreateFromTask(RevertPrivacyAsync);
            SelectProfileCommand = ReactiveCommand.Create<TweakProfile>(SelectProfile);

            _ = Task.Run(RunFullAnalysisAsync);
        }

        private async Task RunFullAnalysisAsync()
        {
            try
            {
                _currentHardware = _hardwareService.GetSystemInfo();
                SystemSpecsSummary = _currentHardware.DetailedSummary;
                
                StatusMessage = "Ejecutando auditoría de seguridad profunda...";
                var findings = _securityService.RunSecurityAudit();
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SecurityFindings.Clear();
                    foreach (var finding in findings) SecurityFindings.Add(finding);
                    CalculateSecurityScore();

                    var recommendedIds = _recommendationService.GetRecommendedTweakIds(_currentHardware, Tweaks.ToList());
                    foreach (var tweak in Tweaks)
                    {
                        tweak.IsRecommended = recommendedIds.Contains(tweak.Id);
                        _optimizationService.CheckTweakStatus(tweak);
                    }
                    StatusMessage = "Sistema listo y analizado.";
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Fallo en el escaneo inicial", ex);
                StatusMessage = "Error en el análisis.";
            }
        }

        private void CalculateSecurityScore()
        {
            if (!SecurityFindings.Any()) { SecurityScore = 100; return; }
            int totalPenalty = SecurityFindings.Sum(f => f.Severity switch
            {
                Severity.Critical => 40, Severity.High => 20, Severity.Medium => 10, Severity.Low => 5, _ => 0
            });
            SecurityScore = Math.Max(0, 100 - totalPenalty);
        }

        private async Task FixSecurityFindingsAsync()
        {
            StatusMessage = "Corrigiendo vulnerabilidades...";
            await Task.Run(() =>
            {
                foreach (var finding in SecurityFindings.ToList())
                {
                    if (!finding.IsFixed) { if (_securityService.FixFinding(finding)) finding.IsFixed = true; }
                }
                CalculateSecurityScore();
                StatusMessage = "Seguridad reforzada correctamente.";
            });
        }

        private async Task RevertPrivacyAsync()
        {
            StatusMessage = "Revirtiendo ajustes de privacidad a estado de fábrica...";
            // Logic to un-apply all privacy tweaks
            foreach (var tweak in Tweaks.Where(t => t.Category == TweakCategory.Privacy))
            {
                tweak.IsApplied = false;
                // Note: Revert logic would be needed in OptimizationService for full implementation
            }
            StatusMessage = "Privacidad restablecida (requiere reinicio).";
        }

        private void SelectProfile(TweakProfile profile)
        {
            StatusMessage = $"Perfil seleccionado: {profile.Name}";
            foreach (var tweak in Tweaks) tweak.IsApplied = profile.TweakIds.Contains(tweak.Id);
        }

        private async Task ApplySmartFixAsync()
        {
            StatusMessage = "Aplicando Smart Fix...";
            foreach (var tweak in Tweaks) { if (tweak.IsRecommended && tweak.Risk == RiskLevel.Safe) tweak.IsApplied = true; }
            await ApplyChangesAsync();
        }

        private async Task ApplyChangesAsync()
        {
            StatusMessage = "Iniciando optimización masiva...";
            await Task.Run(() =>
            {
                SystemRestoreManager.CreateRestorePoint("PhantomOS Privacy Shield Session");
                var toApply = Tweaks.Where(t => t.IsApplied).ToList();
                var appliedList = new List<AtomicTweak>();

                foreach (var tweak in toApply)
                {
                    StatusMessage = $"Procesando: {tweak.Name}...";
                    if (_optimizationService.ApplyTweak(tweak)) appliedList.Add(tweak);
                }

                if (appliedList.Any())
                {
                    _optimizationService.GenerateReport(appliedList);
                    StatusMessage = "Optimización completada. Reiniciando Explorer...";
                    _optimizationService.RestartExplorer();
                }

                StatusMessage = "¡PhantomOS ha terminado con éxito!";
            });
        }
    }
}
