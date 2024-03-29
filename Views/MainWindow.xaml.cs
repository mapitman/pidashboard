using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using pidashboard.ViewModels;

namespace pidashboard.Views
{
    public class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel, MainView view)
        {
            Content = view;
            DataContext = viewModel;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }
    }
}