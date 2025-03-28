using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Web3IoT.Data;
using Web3IoT.Models;

namespace Web3IoT.Services
{
    public class SensorDataService : BackgroundService
    {
        private readonly ILogger<SensorDataService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MqttService _mqttService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

        public SensorDataService(
            ILogger<SensorDataService> logger,
            IServiceProvider serviceProvider,
            MqttService mqttService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mqttService = mqttService;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSensorDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xử lý dữ liệu cảm biến");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessSensorDataAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Lấy danh sách tất cả các sensor
            var sensors = await dbContext.Sensors.ToListAsync();

            foreach (var sensor in sensors)
            {
                try
                {
                    // Lấy dữ liệu từ MQTT cache
                    var sensorData = _mqttService.GetSensorData(sensor.Name);
                    if (sensorData == null)
                    {
                        _logger.LogWarning("Không tìm thấy dữ liệu cho cảm biến {SensorName}", sensor.Name);
                        continue;
                    }

                    // Gửi dữ liệu lên blockchain
                    await SendDataToBlockchainAsync(sensor, sensorData);

                    _logger.LogInformation(
                        "Đã xử lý dữ liệu cảm biến {SensorName}: Nhiệt độ: {Temperature}°C, Độ ẩm: {Humidity}%, Độ ẩm đất: {SoilMoisture}%, Quạt: {Fan}, Bơm: {Pump}",
                        sensor.Name, 
                        sensorData.Temperature, 
                        sensorData.Humidity, 
                        sensorData.SoilMoisture,
                        sensorData.Fan ? "Bật" : "Tắt",
                        sensorData.Pump ? "Bật" : "Tắt");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xử lý dữ liệu cho cảm biến {SensorName}", sensor.Name);
                }
            }
        }

        private async Task SendDataToBlockchainAsync(Sensor sensor, SensorData data)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost:80/");

            var payload = new
            {
                sensorId = sensor.SensorCode,
                temperature = data.Temperature,
                humidity = data.Humidity,
                soilMoisture = data.SoilMoisture,
                fan = data.Fan,
                pump = data.Pump,
                timestamp = data.Timestamp
            };

            var response = await httpClient.PostAsJsonAsync("api/Blockchain/add_trans_sensor", payload);
            response.EnsureSuccessStatusCode();
        }
    }
} 