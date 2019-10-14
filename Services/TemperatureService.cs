using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace pidashboard.Services
{
    public class TemperatureService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TemperatureService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public double? GetCurrentTemperature()
        {
            var client = _httpClientFactory.CreateClient();
            var response = client.GetAsync(_configuration["TemperatureUri"]).Result;
            var body = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<double>(body);
            if (result == Double.MinValue)
            {
                return null;
            }

            return result;
        } 
    }
}