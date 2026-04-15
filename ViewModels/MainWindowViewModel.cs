using ReactiveUI;
using System.Reactive;

namespace PhantomOS.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _statusMessage = "Listo para optimizar";

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public MainWindowViewModel()
        {
            // Initial logic
        }
    }
}
