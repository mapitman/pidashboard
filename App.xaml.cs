using Avalonia;
using Avalonia.Markup.Xaml;

namespace pidashboard
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}