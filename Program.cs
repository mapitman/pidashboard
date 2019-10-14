using System;
using System.Collections;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using Microsoft.Extensions.Configuration;
using pidashboard.ViewModels;
using pidashboard.Views;
using Microsoft.Extensions.DependencyInjection;
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
            var builder = new ConfigurationBuilder()
                .AddYamlFile("appsettings.yaml")
                .AddYamlFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.yaml", true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<MainView>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<IAssetLoader, AssetLoader>();
            serviceCollection.AddSingleton<TemperatureService>();
            serviceCollection.AddHttpClient();
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
                  throw new Exception("Application.Exit doesn't work, so throwing an exception to exit the app :(");
              }
            }
        }
    }
}