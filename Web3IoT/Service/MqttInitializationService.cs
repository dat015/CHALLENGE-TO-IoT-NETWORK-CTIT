// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System.Threading;
// using System.Threading.Tasks;
// using Web3IoT.Data;

// namespace Web3IoT.Services
// {
//     public class MqttInitializationService : IHostedService
//     {
//         private readonly ILogger<MqttInitializationService> _logger;
//         private readonly IServiceScopeFactory _scopeFactory;

//         public MqttInitializationService(ILogger<MqttInitializationService> logger, IServiceScopeFactory scopeFactory)
//         {
//             _logger = logger;
//             _scopeFactory = scopeFactory;
//         }

//         public async Task StartAsync(CancellationToken cancellationToken)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var mqttService = scope.ServiceProvider.GetRequiredService<MqttService>();
//             var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//             try
//             {
//                 // Khởi tạo MQTT
//                 await mqttService.InitializeAsync();

//                 // Subscribe vào tất cả các sensor từ database
//                 var sensors = await dbContext.Sensors.ToListAsync(cancellationToken);
//                 foreach (var sensor in sensors)
//                 {
//                     await mqttService.SubscribeToSensorAsync(sensor.Name);
//                 }

//                 _logger.LogInformation("MQTT đã được khởi tạo và subscribe thành công.");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Lỗi khởi tạo MQTT Service");
//                 throw;
//             }
//         }

//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             _logger.LogInformation("Dừng MqttInitializationService.");
//             return Task.CompletedTask;
//         }
//     }
// }