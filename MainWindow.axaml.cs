using Avalonia.Controls;
using PhantomOS.ViewModels;

namespace PhantomOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}