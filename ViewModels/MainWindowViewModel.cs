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
        private string _statusMessage = "Listo para optimizar";

        public ObservableCollection<AtomicTweak> Tweaks { get; }
        public ObservableCollection<TweakProfile> Profiles { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<TweakProfile, Unit> SelectProfileCommand { get; }

        public MainWindowViewModel()
        {
            _optimizationService = new OptimizationService();
            Tweaks = new ObservableCollection<AtomicTweak>(TweakCatalog.Tweaks);
            Profiles = new ObservableCollection<TweakProfile>(TweakCatalog.Profiles);

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            SelectProfileCommand = ReactiveCommand.Create<TweakProfile>(SelectProfile);

            // Check initial status
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            foreach (var tweak in Tweaks)
            {
                _optimizationService.CheckTweakStatus(tweak);
            }
        }

        private void SelectProfile(TweakProfile profile)
        {
            StatusMessage = $"Perfil seleccionado: {profile.Name}";
            foreach (var tweak in Tweaks)
            {
                tweak.IsApplied = profile.TweakIds.Contains(tweak.Id);
            }
            // Note: In a real app we'd need to trigger UI update for IsApplied if not using a reactive property
            // We'll fix this by making AtomicTweak reactive or manually refreshing
        }

        private async Task ApplyChangesAsync()
        {
            StatusMessage = "Iniciando proceso de optimización...";
            
            await Task.Run(() =>
            {
                // 1. Create System Restore Point
                SystemRestoreManager.CreateRestorePoint("PhantomOS Optimization Session");

                var toApply = Tweaks.ToList(); // Simplified: applying all that are "selected" in UI
                var appliedList = new List<AtomicTweak>();

                foreach (var tweak in toApply)
                {
                    StatusMessage = $"Aplicando: {tweak.Name}...";
                    if (_optimizationService.ApplyTweak(tweak))
                    {
                        appliedList.Add(tweak);
                    }
                }

                // 2. Generate Report
                if (appliedList.Any())
                {
                    _optimizationService.GenerateReport(appliedList);
                    StatusMessage = "Optimizaciones aplicadas. Reiniciando Explorer...";
                    _optimizationService.RestartExplorer();
                }

                StatusMessage = "¡Optimización completada con éxito!";
            });
        }
    }
}
