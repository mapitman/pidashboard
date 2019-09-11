using Avalonia.Media;

namespace pidashboard.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ViewItem Temperature => new ViewItem
        {
            Text = "73° F",
            Foreground = new SolidColorBrush(Colors.Red)
        };

        public string Humidity => "43%";
    }

    public class ViewItem
    {
        public string Text { get; set; }
        public Brush Foreground { get; set; }
    }
}