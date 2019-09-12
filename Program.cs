using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Logging.Serilog;
using Iot.Device.DHTxx;
using pidashboard.ViewModels;
using pidashboard.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pidashboard.Services;

namespace pidashboard
{
    internal class Program
    {
        private static ServiceProvider _serviceProvider;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            RegisterServices(args);

            if (((IList) args).Contains("--fbdev"))
            {
                Console.CursorVisible = false;
                Console.WriteLine(">> Run in Mode: Framebuffer");
                AppBuilder.Configure<App>().InitializeWithLinuxFramebuffer(tl =>
                {
                    tl.Content = _serviceProvider.GetRequiredService<MainView>();
                    ThreadPool.QueueUserWorkItem(_ => ConsoleSilencer());
                });
            }
            else
                BuildAvaloniaApp().Start(AppMain, args);
            Console.CursorVisible = true;
        }

        private static void RegisterServices(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            DhtBase dhtsensor = null;
            if (!args.Contains("--fake"))
            {
                dhtsensor = new Dht22(2);
            }
            
            serviceCollection.AddSingleton<DhtWrapper>(x => new DhtWrapper(dhtsensor));
            serviceCollection.AddSingleton<MainView>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<EnvironmentalSensor>();
            serviceCollection.AddSingleton<MainViewModel>();
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();
        }

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            var window = _serviceProvider.GetRequiredService<MainWindow>();
            app.Run(window);
        }
        
        private static void ConsoleSilencer()
        {
            Console.CursorVisible = false;
            while (true)
            {
              var k = Console.ReadKey(true);
              if (k.Key == ConsoleKey.Q)
              {
                  Console.CursorVisible = true;
                  Console.Clear();
                  Console.WriteLine("Q pressed... exiting...");
                  Application.Current.Exit();
              }
            }
        }
    }
}