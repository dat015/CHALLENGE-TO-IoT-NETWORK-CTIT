// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Web3IoT.Data;
// using Web3IoT.Models;

// namespace Web3IoT.Services
// {
//     public class SensorDataService : BackgroundService
//     {
//         private readonly ILogger<SensorDataService> _logger;
//         private readonly IServiceProvider _serviceProvider;
//         private readonly MqttService _mqttService;
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

//         public SensorDataService(
//             ILogger<SensorDataService> logger,
//             IServiceProvider serviceProvider,
//             MqttService mqttService,
//             IHttpClientFactory httpClientFactory)
//         {
//             _logger = logger;
//             _serviceProvider = serviceProvider;
//             _mqttService = mqttService;
//             _httpClientFactory = httpClientFactory;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     await ProcessSensorDataAsync();
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Lỗi xử lý dữ liệu cảm biến");
//                 }

//                 await Task.Delay(_interval, stoppingToken);
//             }
//         }

//         private async Task ProcessSensorDataAsync()
//         {
//             using var scope = _serviceProvider.CreateScope();
//             var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//             var httpClient = _httpClientFactory.CreateClient();
//             httpClient.BaseAddress = new Uri("http://localhost:80/");

//             var sensors = await dbContext.Sensors.ToListAsync();

//             foreach (var sensor in sensors)
//             {
//                 try
//                 {
//                     var sensorData = _mqttService.GetSensorData(sensor.Name);
//                     if (sensorData == null)
//                     {
//                         _logger.LogWarning("Không tìm thấy dữ liệu cho cảm biến {SensorName}", sensor.Name);
//                         continue;
//                     }

//                     var payload = new
//                     {
//                         sensorId = sensor.SensorCode,
//                         temperature = sensorData.Temperature,
//                         humidity = sensorData.Humidity,
//                         soilMoisture = sensorData.SoilMoisture,
//                         timestamp = sensorData.Timestamp
//                     };

//                     var response = await httpClient.PostAsJsonAsync("api/Blockchain/add_trans_sensor", payload);
//                     response.EnsureSuccessStatusCode();

//                     _logger.LogInformation(
//                         "Đã gửi dữ liệu lên blockchain cho {SensorName}: Nhiệt độ: {Temperature}°C, Độ ẩm: {Humidity}%, Độ ẩm đất: {SoilMoisture}%",
//                         sensor.Name, sensorData.Temperature, sensorData.Humidity, sensorData.SoilMoisture);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Lỗi xử lý dữ liệu cho cảm biến {SensorName}", sensor.Name);
//                 }
//             }
//         }

//         private async Task SendDataToBlockchainAsync(Sensor sensor, SensorData data)
//         {
//             try
//             {

//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error when fetch Blockchain: " + ex.Message);
//                 throw ex;
//             }
//             var httpClient = _httpClientFactory.CreateClient();
//             httpClient.BaseAddress = new Uri("http://localhost:80/");

//             var payload = new
//             {
//                 sensorId = sensor.SensorCode,
//                 temperature = data.Temperature,
//                 humidity = data.Humidity,
//                 soilMoisture = data.SoilMoisture,
//                 timestamp = data.Timestamp
//             };


//             var response = await httpClient.PostAsJsonAsync("api/Blockchain/add_trans_sensor", payload);
//             response.EnsureSuccessStatusCode();
//         }
//     }
// }