using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Web3IoT.Models;
using System.Text.Json.Serialization;


namespace Web3IoT.Services
{
    public class MqttService
    {
        private IMqttClient _mqttClient;
        private readonly ILogger<MqttService> _logger;
        private readonly Dictionary<string, SensorData> _sensorDataCache;
        private readonly MqttFactory _factory;

        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger;
            _sensorDataCache = new Dictionary<string, SensorData>();
            _factory = new MqttFactory();
        }

        // Khởi tạo và kết nối MQTT client
        public async Task InitializeAsync()
        {
            _mqttClient = _factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer("wss://test.mosquitto.org:8081/mqtt")
                .WithClientId($"anhnguyenduc04/iot_{Guid.NewGuid():N}")
                .WithCleanSession()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

            try
            {
                await _mqttClient.ConnectAsync(options, default);
                _logger.LogInformation("Đã kết nối tới MQTT broker");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kết nối MQTT");
                throw;
            }
        }

        // Subscribe vào topic của một sensor
        public async Task SubscribeToSensorAsync(string sensorName)
        {
            if (_mqttClient?.IsConnected != true)
            {
                _logger.LogWarning("Chưa kết nối tới MQTT broker");
                return;
            }

            var topic = $"{sensorName}";
            var mqttSubscribeOptions = _factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                .Build();

            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, default);
            _logger.LogInformation("Đã đăng ký thành công topic: {Topic}", topic);
        }

        // Sự kiện khi kết nối thành công
        private Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.LogInformation("Đã kết nối thành công tới MQTT broker");
            return Task.CompletedTask;
        }

        // Sự kiện khi mất kết nối
        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogWarning("Kết nối MQTT đã đóng");
            return Task.CompletedTask;
        }

        // Sự kiện khi nhận được tin nhắn
        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var topic = arg.ApplicationMessage.Topic;
            var payload = arg.ApplicationMessage.PayloadSegment;
            var message = Encoding.UTF8.GetString(payload);

            _logger.LogInformation("Nhận tin nhắn từ {Topic}: {Message}", topic, message);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var data = JsonSerializer.Deserialize<SensorData>(message, options);
                var sensorName = topic.Split('/').Last();
                UpdateSensorData(sensorName, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý dữ liệu từ topic {Topic}: {Message}", topic, message);
            }

            return Task.CompletedTask;
        }

        // Cập nhật dữ liệu cảm biến
        private void UpdateSensorData(string sensorName, SensorData data)
        {
            _sensorDataCache[sensorName] = data;
            _logger.LogInformation(
                "Cập nhật dữ liệu cảm biến {SensorName}: Nhiệt độ: {Temperature}°C, Độ ẩm: {Humidity}%, Độ ẩm đất: {SoilMoisture}%, Quạt: {Fan}, Bơm: {Pump}",
                sensorName, 
                data.Temperature, 
                data.Humidity, 
                data.SoilMoisture,
                data.Fan ? "Bật" : "Tắt",
                data.Pump ? "Bật" : "Tắt");
        }

        // Lấy dữ liệu cảm biến từ cache
        public SensorData GetSensorData(string sensorName)
        {
            return _sensorDataCache.TryGetValue(sensorName, out var data) ? data : null;
        }

        // Gửi lệnh điều khiển cho sensor
        public async Task SendCommandAsync(string sensorName, string command, string value)
        {
            if (_mqttClient?.IsConnected != true)
            {
                _logger.LogWarning("Chưa kết nối tới MQTT broker");
                return;
            }

            var topic = $"{sensorName}/{command}";
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(value))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(applicationMessage, default);
            _logger.LogInformation("Đã gửi lệnh {Command} = {Value} tới cảm biến {SensorName}", command, value, sensorName);
        }

        // Gửi lệnh bật/tắt nước
        public async Task ToggleWaterAsync(string sensorName, bool isOn)
        {
            await SendCommandAsync(sensorName, "pump", isOn ? "ON" : "OFF");
        }

        // Gửi lệnh bật/tắt đèn
        public async Task ToggleLightAsync(string sensorName, bool isOn)
        {
            await SendCommandAsync(sensorName, "light", isOn ? "ON" : "OFF");
        }

        // Gửi thời gian ngủ
        public async Task SendSleepTimeAsync(string sensorName, int sleepTime)
        {
            if (sleepTime <= 0)
            {
                _logger.LogWarning("Vui lòng nhập số giây hợp lệ (lớn hơn 0)");
                return;
            }

            await SendCommandAsync(sensorName, "sleep", sleepTime.ToString());
        }
    }

    // Class định nghĩa dữ liệu cảm biến
    public class SensorData
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        [JsonPropertyName("soilMoisture")]
        public double SoilMoisture { get; set; }

        [JsonPropertyName("fan")]
        public bool Fan { get; set; }

        [JsonPropertyName("pump")]
        public bool Pump { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}