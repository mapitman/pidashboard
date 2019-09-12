using System;
using Avalonia.Media;
using Avalonia.Threading;
using pidashboard.Services;
using ReactiveUI;

namespace pidashboard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly EnvironmentalSensor _environmentalSensor;
        private ViewItem _temperature = new ViewItem("n/a");
        private ViewItem _temperatureHigh = new ViewItem("High: n/a");
        private ViewItem _temperatureLow = new ViewItem("Low: n/a");
        private ViewItem _humidity = new ViewItem("n/a");
        private double _highTemp;
        private DateTime _highTempDateTime = DateTime.MinValue;
        private double _lowTemp;
        private DateTime _lowTempDateTime = DateTime.MinValue;
        private ViewItem _currentTime = new ViewItem("00:00");

        public MainViewModel(EnvironmentalSensor environmentalSensor)
        {
            _environmentalSensor = environmentalSensor;
            EnvironmentalDataUpdater();
            CurrentTimeUpdater();
            DispatcherTimer.Run(EnvironmentalDataUpdater, TimeSpan.FromSeconds(5));
            DispatcherTimer.Run(CurrentTimeUpdater, TimeSpan.FromSeconds(1));
        }

        private bool CurrentTimeUpdater()
        {
            CurrentTime.Text = DateTime.Now.ToShortTimeString();
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

        private bool EnvironmentalDataUpdater()
        {
            var d = _environmentalSensor.GetData();
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
            return true;
        }

        private static SolidColorBrush DetermineTemperatureColor(double temperature)
        {
            var color = Colors.Green;
            if (temperature > 74 && temperature < 77)
                color = Colors.Yellow;
            else if (temperature >= 77) color = Colors.Red;
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
}