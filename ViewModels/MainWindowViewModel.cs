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
        
        private string _statusMessage = "Analizando sistema...";
        private string _systemSpecsSummary = "Cargando especificaciones...";
        private HardwareInfo? _currentHardware;

        public ObservableCollection<AtomicTweak> Tweaks { get; }
        public ObservableCollection<TweakProfile> Profiles { get; }

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

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<Unit, Unit> SmartFixCommand { get; }
        public ReactiveCommand<TweakProfile, Unit> SelectProfileCommand { get; }

        public MainWindowViewModel()
        {
            _optimizationService = new OptimizationService();
            _hardwareService = new HardwareService();
            _recommendationService = new RecommendationService();

            Tweaks = new ObservableCollection<AtomicTweak>(TweakCatalog.Tweaks);
            Profiles = new ObservableCollection<TweakProfile>(TweakCatalog.Profiles);

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            SmartFixCommand = ReactiveCommand.CreateFromTask(ApplySmartFixAsync);
            SelectProfileCommand = ReactiveCommand.Create<TweakProfile>(SelectProfile);

            // Trigger Automatic Hardware Scan
            _ = Task.Run(RunHardwareScanAsync);
        }

        private async Task RunHardwareScanAsync()
        {
            try
            {
                _currentHardware = _hardwareService.GetSystemInfo();
                SystemSpecsSummary = _currentHardware.DetailedSummary;
                
                // Get Recommendations
                var recommendedIds = _recommendationService.GetRecommendedTweakIds(_currentHardware, Tweaks.ToList());
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var tweak in Tweaks)
                    {
                        tweak.IsRecommended = recommendedIds.Contains(tweak.Id);
                        // Also check current real-world status
                        _optimizationService.CheckTweakStatus(tweak);
                    }
                    StatusMessage = $"Análisis completado. Encontradas {recommendedIds.Count} recomendaciones para tu equipo.";
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Fallo en el escaneo automático inicial", ex);
                StatusMessage = "Error al analizar el hardware.";
            }
        }

        private void SelectProfile(TweakProfile profile)
        {
            StatusMessage = $"Perfil seleccionado: {profile.Name}";
            foreach (var tweak in Tweaks)
            {
                tweak.IsApplied = profile.TweakIds.Contains(tweak.Id);
            }
        }

        private async Task ApplySmartFixAsync()
        {
            StatusMessage = "Aplicando 'Smart Fix' recomendado para tu equipo...";
            foreach (var tweak in Tweaks)
            {
                if (tweak.IsRecommended && tweak.Risk == RiskLevel.Safe)
                {
                    tweak.IsApplied = true;
                }
            }
            await ApplyChangesAsync();
        }

        private async Task ApplyChangesAsync()
        {
            StatusMessage = "Iniciando proceso de optimización...";
            
            await Task.Run(() =>
            {
                SystemRestoreManager.CreateRestorePoint("PhantomOS Session");
                var toApply = Tweaks.Where(t => t.IsApplied).ToList();
                var appliedList = new List<AtomicTweak>();

                foreach (var tweak in toApply)
                {
                    StatusMessage = $"Aplicando: {tweak.Name}...";
                    if (_optimizationService.ApplyTweak(tweak))
                    {
                        appliedList.Add(tweak);
                    }
                }

                if (appliedList.Any())
                {
                    _optimizationService.GenerateReport(appliedList);
                    StatusMessage = "Optimizaciones completadas. Reiniciando Explorer...";
                    _optimizationService.RestartExplorer();
                }

                StatusMessage = "¡Optimización inteligente completada!";
            });
        }
    }
}
