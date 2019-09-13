using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Iot.Device.DHTxx;
using Iot.Units;

namespace pidashboard.Services
{
    public class EnvironmentalSensor
    {
        private readonly Timer _dataUpdateTimer;
        private readonly DhtBase _sensor;
        private readonly Timer _sensorReadTimer;
        private readonly List<EnvironmentalSensorData> _series = new List<EnvironmentalSensorData>();
        private object _locker = new object();

        public EnvironmentalSensor(DhtWrapper sensor)
        {
            _sensor = sensor.DhtSensor;
            _sensorReadTimer = new Timer(ReadData, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            _dataUpdateTimer = new Timer(UpdateData, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public EnvironmentalSensorData Data { get; private set; }

        private void UpdateData(object state)
        {
            if (_series.Count < 10) return;
            var result = new EnvironmentalSensorData();
            var standardFactor = 2.0;
            var temperatureMean = _series.Average(x => x.Temperature.Fahrenheit);
            var temperatureStandardDeviation =
                Math.Sqrt(_series.Select(x => Math.Pow(x.Temperature.Fahrenheit - temperatureMean, 2)).Sum() /
                          _series.Count);

            double filteredTemperature;
            if (Math.Abs(temperatureStandardDeviation) < 1)
                filteredTemperature = _series.First().Temperature.Fahrenheit;
            else
                filteredTemperature = _series.Where(x =>
                        x.Temperature.Fahrenheit >
                        temperatureMean - standardFactor * temperatureStandardDeviation)
                    .Where(x => x.Temperature.Fahrenheit <
                                temperatureMean + standardFactor * temperatureStandardDeviation)
                    .Average(x => x.Temperature.Fahrenheit);

            result.Temperature = Temperature.FromFahrenheit(filteredTemperature);

            var humidityMean = _series.Average(x => x.Humidity);
            var humidityStandardDeviation =
                Math.Sqrt(_series.Select(x => Math.Pow(x.Humidity - humidityMean, 2)).Sum() / _series.Count);

            double filteredHumidity;
            if (Math.Abs(humidityStandardDeviation) < 1)
                filteredHumidity = _series.First().Humidity;
            else
                filteredHumidity = _series.Where(x =>
                        x.Humidity > humidityMean - standardFactor * humidityStandardDeviation)
                    .Where(x => x.Humidity < humidityMean + standardFactor * humidityStandardDeviation)
                    .Average(x => x.Humidity);

            result.Humidity = filteredHumidity;

            Data = result;
            _series.Clear();
        }

        private void ReadData(object state)
        {
            var result = new EnvironmentalSensorData();
            if (_sensor == null)
            {
                result.Temperature = Temperature.FromFahrenheit(73.5);
                result.Humidity = 60.5;
                _series.Add(result);
            }
            else
            {
                lock (_locker)
                {
                    try
                    {
                        result.Temperature = _sensor.Temperature;
                        result.Humidity = _sensor.Humidity;
                        if (_sensor.IsLastReadSuccessful) _series.Add(result);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // TODO: add logging and log this error
                    }
                }

                
            }
        }
    }

    public class EnvironmentalSensorData
    {
        public Temperature Temperature { get; set; }
        public double Humidity { get; set; }
    }

    public class DhtWrapper
    {
        public DhtWrapper(DhtBase dhtsensor)
        {
            DhtSensor = dhtsensor;
        }

        public DhtBase DhtSensor { get; }
    }
}