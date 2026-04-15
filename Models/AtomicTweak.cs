using ReactiveUI;
using Microsoft.Win32;

namespace PhantomOS.Models
{
    public class AtomicTweak : ReactiveObject
    {
        private bool _isApplied;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TweakCategory Category { get; set; }
        public RiskLevel Risk { get; set; }
        
        // Education Module
        public string WhyActivate { get; set; } = string.Empty;
        public string WhyDeactivate { get; set; } = string.Empty;
        
        // ... rest of properties
        public string? RegistryKey { get; set; }
        public string? RegistryValueName { get; set; }
        public object? RegistryValueData { get; set; }
        public RegistryValueKind RegistryValueKind { get; set; } = RegistryValueKind.DWord;
        
        public string? ServiceName { get; set; }
        
        public bool IsApplied 
        { 
            get => _isApplied;
            set => this.RaiseAndSetIfChanged(ref _isApplied, value);
        }
        public bool RequiresReboot { get; set; }
        public bool RequiresExplorerRestart { get; set; }

        // Logic (To be handled by OptimizationService)
    }
}
