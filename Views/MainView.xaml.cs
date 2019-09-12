using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using pidashboard.ViewModels;

namespace pidashboard.Views
{
    public class MainView : UserControl
    {
        public MainView(MainViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}