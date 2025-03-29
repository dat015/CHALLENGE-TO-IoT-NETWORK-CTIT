// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Threading;
// using System.Threading.Tasks;

// namespace Web3IoT.Services
// {
//     public class BlockchainSyncService : BackgroundService
//     {
//         private readonly ILogger<BlockchainSyncService> _logger;
//         private readonly IServiceScopeFactory _scopeFactory;
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

//         public BlockchainSyncService(
//             ILogger<BlockchainSyncService> logger,
//             IServiceScopeFactory scopeFactory,
//             IHttpClientFactory httpClientFactory)
//         {
//             _logger = logger;
//             _scopeFactory = scopeFactory;
//             _httpClientFactory = httpClientFactory;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             _logger.LogInformation("BlockchainSyncService đã khởi động.");

//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     await SendDataToBlockchainAsync();
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Lỗi khi gửi dữ liệu lên blockchain");
//                 }

//                 await Task.Delay(_interval, stoppingToken);
//             }

//             _logger.LogInformation("BlockchainSyncService đã dừng.");
//         }

//         private async Task SendDataToBlockchainAsync()
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var mqttService = scope.ServiceProvider.GetRequiredService<MqttService>();
//             var httpClient = _httpClientFactory.CreateClient();
//             httpClient.BaseAddress = new Uri("http://localhost:80/");

//             // Lấy tất cả dữ liệu từ cache của MqttService
//             var sensorDataCache = mqttService.GetType()
//                 .GetField("_sensorDataCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//                 ?.GetValue(mqttService) as Dictionary<string, SensorData>;

//             if (sensorDataCache == null || !sensorDataCache.Any())
//             {
//                 _logger.LogWarning("Không có dữ liệu cảm biến nào trong cache.");
//                 return;
//             }

//             foreach (var sensorEntry in sensorDataCache)
//             {
//                 var sensorName = sensorEntry.Key;
//                 var sensorData = sensorEntry.Value;

//                 if (sensorData == null)
//                 {
//                     _logger.LogWarning("Không có dữ liệu cho cảm biến {SensorName}", sensorName);
//                     continue;
//                 }

//                 var payload = new
//                 {
//                     sensorId = sensorName,
//                     temperature = sensorData.Temperature,
//                     humidity = sensorData.Humidity,
//                     soilMoisture = sensorData.SoilMoisture,
//                     timestamp = sensorData.Timestamp
//                 };

//                 try
//                 {
//                     var response = await httpClient.PostAsJsonAsync("api/Blockchain/add_trans_sensor", payload);
//                     response.EnsureSuccessStatusCode();
//                     _logger.LogInformation(
//                         "Đã gửi dữ liệu lên blockchain cho {SensorName}: Nhiệt độ: {Temperature}°C, Độ ẩm: {Humidity}%, Độ ẩm đất: {SoilMoisture}%",
//                         sensorName, sensorData.Temperature, sensorData.Humidity, sensorData.SoilMoisture);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Lỗi khi gửi dữ liệu cho cảm biến {SensorName}", sensorName);
//                 }
//             }
//         }
//     }
// }