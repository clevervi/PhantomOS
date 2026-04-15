using ReactiveUI;

namespace PhantomOS.Models
{
    public enum Severity
    {
        Informational,
        Low,
        Medium,
        High,
        Critical
    }

    public class SecurityFinding : ReactiveObject
    {
        private bool _isFixed;
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Severity Severity { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        
        // Technical fix details
        public string? RegistryKey { get; set; }
        public string? RegistryValueName { get; set; }
        public object? RegistryValueData { get; set; }

        public bool IsFixed
        {
            get => _isFixed;
            set => this.RaiseAndSetIfChanged(ref _isFixed, value);
        }
    }
}
