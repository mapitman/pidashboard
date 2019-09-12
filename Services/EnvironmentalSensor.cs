using System.Threading;
using Iot.Device.DHTxx;
using Iot.Units;

namespace pidashboard.Services
{
    public class EnvironmentalSensor
    {
        private readonly DhtBase _sensor;

        public EnvironmentalSensor(DhtWrapper sensor)
        {
            _sensor = sensor.DhtSensor;
        }

        public EnvironmentalSensorData GetData()
        {
            var result = new EnvironmentalSensorData();
            if (_sensor == null)
            {
                result.Temperature = Temperature.FromFahrenheit(73.5);
                result.Humidity = 60.5;
            }
            else
            {
                result.Temperature = _sensor.Temperature;
                result.Humidity = _sensor.Humidity;
                for (var i = 0; i < 10; i++)
                {
                    if (_sensor.IsLastReadSuccessful) break;
                    Thread.Sleep(1000);
                    result.Temperature = _sensor.Temperature;
                    result.Humidity = _sensor.Humidity;
                }

            }

            return result;
        }
        
    }

    public class EnvironmentalSensorData
    {
        public Temperature Temperature { get; set; }
        public double Humidity { get; set; }
    }

    public class DhtWrapper
    {
        private readonly DhtBase _dhtsensor;

        public DhtWrapper(DhtBase dhtsensor)
        {
            _dhtsensor = dhtsensor;
        }

        public DhtBase DhtSensor => _dhtsensor;
    }
}