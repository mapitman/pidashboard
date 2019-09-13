using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Newtonsoft.Json;
using pidashboard.Services;
using ReactiveUI;

namespace pidashboard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly EnvironmentalSensor _environmentalSensor;
        private readonly IAssetLoader _assets;
        private ViewItem _temperature = new ViewItem("n/a");
        private ViewItem _temperatureHigh = new ViewItem("High: n/a");
        private ViewItem _temperatureLow = new ViewItem("Low: n/a");
        private ViewItem _humidity = new ViewItem("n/a");
        private double _highTemp;
        private DateTime _highTempDateTime = DateTime.MinValue;
        private double _lowTemp;
        private DateTime _lowTempDateTime = DateTime.MinValue;
        private ViewItem _currentTime = new ViewItem("00:00");
        private Bitmap _imageFrame;
        private Bitmap[] _frames;
        private int _currentFrame = 0;

        public MainViewModel(EnvironmentalSensor environmentalSensor, IAssetLoader assets)
        {
            _environmentalSensor = environmentalSensor;
            _assets = assets;
            _frames = new Bitmap[]
            {
                new Bitmap(_assets.Open(new Uri("avares://pidashboard/Assets/nyan-0.png", UriKind.Absolute))), 
                new Bitmap(_assets.Open(new Uri("avares://pidashboard/Assets/nyan-1.png", UriKind.Absolute))),
                new Bitmap(_assets.Open(new Uri("avares://pidashboard/Assets/nyan-2.png", UriKind.Absolute))),
                new Bitmap(_assets.Open(new Uri("avares://pidashboard/Assets/nyan-3.png", UriKind.Absolute)))
            };
            
            EnvironmentalDataUpdater();
            CurrentTimeUpdater();
            DispatcherTimer.Run(EnvironmentalDataUpdater, TimeSpan.FromSeconds(30));
            DispatcherTimer.Run(CurrentTimeUpdater, TimeSpan.FromSeconds(1));
            DispatcherTimer.Run(NyanCatenator, TimeSpan.FromMilliseconds(150));
        }

        private bool NyanCatenator()
        {
            ImageFrame = _frames[_currentFrame];
            _currentFrame++;
            if (_currentFrame > 3)
                _currentFrame = 0;
            return true;
        }

        private bool CurrentTimeUpdater()
        {
            CurrentTime.Text = DateTime.Now.ToLongTimeString();
            return true;
        }

        public ViewItem Temperature
        {
            get => _temperature;
            private set => this.RaiseAndSetIfChanged(ref _temperature, value);
        }

        public ViewItem TemperatureHigh
        {
            get => _temperatureHigh;
            private set => this.RaiseAndSetIfChanged(ref _temperatureHigh, value);
        }
        
        public ViewItem TemperatureLow
        {
            get => _temperatureLow;
            private set => this.RaiseAndSetIfChanged(ref _temperatureLow, value);
        }
        
        public ViewItem Humidity
        {
            get => _humidity;
            private set => this.RaiseAndSetIfChanged(ref _humidity, value);
        }

        public ViewItem CurrentTime
        {
            get => _currentTime;
            private set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public Bitmap ImageFrame
        {
            get => _imageFrame;
            set =>  this.RaiseAndSetIfChanged(ref _imageFrame, value);
        }

        private bool EnvironmentalDataUpdater()
        {
            var d = _environmentalSensor.Data;
            if (d != null)
            {
                Temperature.Text = $"{d.Temperature.Fahrenheit:F2}° F";
                Temperature.Foreground = DetermineTemperatureColor(d.Temperature.Fahrenheit);
                Humidity.Text = $"{d.Humidity:F1}%";
                var now = DateTime.Now;
                if (d.Temperature.Fahrenheit > _highTemp || now > _highTempDateTime.AddDays(1))
                {
                    _highTemp = d.Temperature.Fahrenheit;
                    _highTempDateTime = now;
                    _temperatureHigh.Text = $"High: {_highTemp:F2}° F @ {_highTempDateTime.ToShortTimeString()}";
                }

                if (d.Temperature.Fahrenheit < _lowTemp || now > _lowTempDateTime.AddDays(1))
                {
                    _lowTemp = d.Temperature.Fahrenheit;
                    _lowTempDateTime = now;
                    _temperatureLow.Text = $"Low:  {_lowTemp:F2}° F @ {_lowTempDateTime.ToShortTimeString()}";
                }

                WriteEnvironmentalDataToFile(d);
            }

            return true;
        }

        private void WriteEnvironmentalDataToFile(EnvironmentalSensorData environmentalSensorData)
        {
            var data = new EnvironmentalData
            {
                Fahrenheit = environmentalSensorData.Temperature.Fahrenheit,
                Humidity = environmentalSensorData.Humidity
            };
            using (var file = File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".temperature.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, data);
                file.Flush();
            }
            
        }

        private static SolidColorBrush DetermineTemperatureColor(double temperature)
        {
            var color = Colors.Green;
            if (temperature > 73 && temperature <= 76)
                color = Colors.Yellow;
            else if (temperature > 76) color = Colors.Red;
            return new SolidColorBrush(color);
        }
    }

    public class ViewItem : ReactiveObject
    {
        public ViewItem(string text)
        {
            Text = text;
            Foreground = new SolidColorBrush(Colors.Gray);
        }
        private string _text;
        private Brush _foreground;

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => this.RaiseAndSetIfChanged(ref _foreground, value);
        }
    }

    public class EnvironmentalData
    {
        public double Fahrenheit { get; set; }
        public double Humidity { get; set; }
    }
}