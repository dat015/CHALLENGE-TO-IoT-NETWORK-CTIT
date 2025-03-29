// using MQTTnet;
// using MQTTnet.Client;
// using MQTTnet.Protocol;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Linq;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Web3IoT.Models;
// using System.Text.Json.Serialization;
// using System.Net.Http;
// using System.Net.Http.Json;
// using System.Timers;

// namespace Web3IoT.Services
// {
//     public class MqttService
//     {
//         private IMqttClient _mqttClient;
//         private readonly ILogger<MqttService> _logger;
//         private readonly Dictionary<string, SensorData> _sensorDataCache;
//         private readonly MqttFactory _factory;
//         private readonly HttpClient _httpClient;
//         private System.Timers.Timer _timer;

//         public MqttService(ILogger<MqttService> logger)
//         {
//             _logger = logger;
//             _sensorDataCache = new Dictionary<string, SensorData>();
//             _factory = new MqttFactory();
//             _httpClient = new HttpClient();
//         }

//         public async Task InitializeAsync()
//         {
//             _mqttClient = _factory.CreateMqttClient();

//             var options = new MqttClientOptionsBuilder()
//                 .WithWebSocketServer("wss://test.mosquitto.org:8081/mqtt")
//                 .WithClientId($"anhnguyenduc04/iot_{Guid.NewGuid():N}")
//                 .WithCleanSession()
//                 .Build();

//             _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
//             _mqttClient.ConnectedAsync += OnConnectedAsync;
//             _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

//             try
//             {
//                 await _mqttClient.ConnectAsync(options, default);
//                 _logger.LogInformation("Đã kết nối tới MQTT broker");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Lỗi kết nối MQTT");
//                 throw;
//             }

//             // Khởi tạo Timer để lấy dữ liệu từ MQTT mỗi 2 phút
//             _timer = new System.Timers.Timer(120000);
//             _timer.Elapsed += async (sender, e) => await FetchAndSendDataAsync();
//             _timer.Start();
//         }

//         public async Task SubscribeToSensorAsync(string sensorName)
//         {
//             if (_mqttClient?.IsConnected != true)
//             {
//                 _logger.LogWarning("Chưa kết nối tới MQTT broker");
//                 return;
//             }

//             var topic = $"{sensorName}";
//             var mqttSubscribeOptions = _factory.CreateSubscribeOptionsBuilder()
//                 .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
//                 .Build();

//             await _mqttClient.SubscribeAsync(mqttSubscribeOptions, default);
//             _logger.LogInformation("Đã đăng ký thành công topic: {Topic}", topic);
//         }

//         private Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
//         {
//             _logger.LogInformation("Đã kết nối thành công tới MQTT broker");
//             return Task.CompletedTask;
//         }

//         private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
//         {
//             _logger.LogWarning("Kết nối MQTT đã đóng");
//             return Task.CompletedTask;
//         }

//         private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
//         {
//             var topic = arg.ApplicationMessage.Topic;
//             var payload = arg.ApplicationMessage.PayloadSegment;
//             var message = Encoding.UTF8.GetString(payload);

//             _logger.LogInformation("Nhận tin nhắn từ {Topic}: {Message}", topic, message);

//             try
//             {
//                 var options = new JsonSerializerOptions
//                 {
//                     PropertyNameCaseInsensitive = true
//                 };
//                 var data = JsonSerializer.Deserialize<SensorData>(message, options);
//                 var sensorName = topic.Split('/').Last();
//                 UpdateSensorData(sensorName, data);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Lỗi xử lý dữ liệu từ topic {Topic}: {Message}", topic, message);
//             }

//             return Task.CompletedTask;
//         }

//         private void UpdateSensorData(string sensorName, SensorData data)
//         {
//             _sensorDataCache[sensorName] = data;
//             _logger.LogInformation(
//                 "Cập nhật dữ liệu cảm biến {SensorName}: Nhiệt độ: {Temperature}°C, Độ ẩm: {Humidity}%, Độ ẩm đất: {SoilMoisture}%, Quạt: {Fan}, Bơm: {Pump}",
//                 sensorName, 
//                 data.Temperature, 
//                 data.Humidity, 
//                 data.SoilMoisture,
//                 data.Fan ? "Bật" : "Tắt",
//                 data.Pump ? "Bật" : "Tắt");
//         }

//         public SensorData GetSensorData(string sensorName)
//         {
//             return _sensorDataCache.TryGetValue(sensorName, out var data) ? data : null;
//         }

//         public async Task SendCommandAsync(string sensorName, string command, string value)
//         {
//             if (_mqttClient?.IsConnected != true)
//             {
//                 _logger.LogWarning("Chưa kết nối tới MQTT broker");
//                 return;
//             }

//             var topic = $"{sensorName}/{command}";
//             var applicationMessage = new MqttApplicationMessageBuilder()
//                 .WithTopic(topic)
//                 .WithPayload(Encoding.UTF8.GetBytes(value))
//                 .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
//                 .Build();

//             await _mqttClient.PublishAsync(applicationMessage, default);
//             _logger.LogInformation("Đã gửi lệnh {Command} = {Value} tới cảm biến {SensorName}", command, value, sensorName);
//         }

//         public async Task ToggleWaterAsync(string sensorName, bool isOn)
//         {
//             await SendCommandAsync(sensorName, "pump", isOn ? "ON" : "OFF");
//         }

//         public async Task ToggleLightAsync(string sensorName, bool isOn)
//         {
//             await SendCommandAsync(sensorName, "light", isOn ? "ON" : "OFF");
//         }

//         public async Task SendSleepTimeAsync(string sensorName, int sleepTime)
//         {
//             if (sleepTime <= 0)
//             {
//                 _logger.LogWarning("Vui lòng nhập số giây hợp lệ (lớn hơn 0)");
//                 return;
//             }

//             await SendCommandAsync(sensorName, "sleep", sleepTime.ToString());
//         }

//         private async Task FetchAndSendDataAsync()
//         {
//             // Fetch data from MQTT
//             foreach (var sensorName in _sensorDataCache.Keys)
//             {
//                 var sensorData = GetSensorData(sensorName);
//                 if (sensorData != null)
//                 {
//                     var blockchainData = new 
//                     {
//                         Timestamp = DateTime.UtcNow,
//                         Humidity = sensorData.Humidity,
//                         Temperature = sensorData.Temperature,
//                         SoilMoisture = sensorData.SoilMoisture,
//                         SensorId = 11 // Assuming sensorName is the SensorCode
//                     };

//                     var response = await _httpClient.PostAsJsonAsync("http://localhost:80/api/Blockchain/add_trans_sensor", blockchainData);
//                     if (response.IsSuccessStatusCode)
//                     {
//                         _logger.LogInformation("Đã gửi dữ liệu cảm biến {SensorName} lên blockchain thành công", sensorName);
//                     }
//                     else
//                     {
//                         _logger.LogError("Lỗi khi gửi dữ liệu cảm biến {SensorName} lên blockchain: {StatusCode}", sensorName, response.StatusCode);
//                     }
//                 }
//             }
//         }
//     }

//     public class BlockchainData
//     {
//         public DateTime Timestamp { get; set; }
//         public double Humidity { get; set; }
//         public double Temperature { get; set; }
//         public double SoilMoisture { get; set; }
//         public string SensorId { get; set; }
//     }

//     public class SensorData
//     {
//         [JsonPropertyName("temperature")]
//         public double Temperature { get; set; }

//         [JsonPropertyName("humidity")]
//         public double Humidity { get; set; }

//         [JsonPropertyName("soilMoisture")]
//         public double SoilMoisture { get; set; }
        
//         [JsonPropertyName("fan")]
//         public bool Fan { get; set; }

//         [JsonPropertyName("pump")]
//         public bool Pump { get; set; }
//     }
// }