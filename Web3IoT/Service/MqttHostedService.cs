// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System.Threading;
// using System.Threading.Tasks;

// namespace Web3IoT.Services
// {
//     public class MqttHostedService : IHostedService
//     {
//         private readonly MqttService _mqttService;
//         private readonly ILogger<MqttHostedService> _logger;

//         public MqttHostedService(MqttService mqttService, ILogger<MqttHostedService> logger)
//         {
//             _mqttService = mqttService;
//             _logger = logger;
//         }

//         public async Task StartAsync(CancellationToken cancellationToken)
//         {
//             _logger.LogInformation("Starting MQTT Hosted Service.");
//             await _mqttService.InitializeAsync();
//             // Add subscriptions here if needed
//             await _mqttService.SubscribeToSensorAsync("anhnguyenduc04/iot");
//         }

//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             _logger.LogInformation("Stopping MQTT Hosted Service.");
//             // Clean up resources if needed
//             return Task.CompletedTask;
//         }
//     }
// }