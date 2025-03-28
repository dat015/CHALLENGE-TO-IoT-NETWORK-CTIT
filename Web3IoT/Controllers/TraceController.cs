using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web3IoT.Models;

namespace Web3IoT.Controllers
{
    [Route("[controller]")]
    public class TraceController : Controller
    {
        private readonly ILogger<TraceController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TraceController(ILogger<TraceController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [Route("/Trace/Index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/Trace/GetTrace")]
        public async Task<IActionResult> GetTrace(string code)
        {
            _logger.LogInformation("GetTrace called with code: {Code}", code);

            if (code == "")
            {
                _logger.LogWarning("Invalid code: {Code}", code);
                return View("Error");
            }
            if (string.IsNullOrEmpty(code) || !int.TryParse(code, out int cropId))
            {
                _logger.LogWarning("Invalid code format: {Code}", code);
                return View("Error");
            }

            try
            {
                var traces = await TraceCropAsync(cropId);
                if (traces == null)
                {
                    _logger.LogWarning("TraceCropAsync returned null for code: {Code}", code);
                    return View("Error");
                }

                ViewBag.Traces = traces;
                ViewBag.Code = code;
                _logger.LogInformation("Successfully fetched traces for code: {Code}", code);
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trace data for code: {Code}", code);
                return View("Error");
            }
        }

        private async Task<List<CropTrace>> TraceCropAsync(int cropId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost:80/");
            var apiUrl = $"api/blockchain/trace/crop/{cropId}";

            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                // Đọc nội dung response dưới dạng chuỗi
                string responseContent = await response.Content.ReadAsStringAsync();

                // Log nội dung response
                _logger.LogInformation("Response from API for cropId {CropId}: {ResponseContent}", cropId, responseContent);

                // Parse JSON thành danh sách các chuỗi JSON
                var jsonArray = JsonSerializer.Deserialize<List<string>>(responseContent);
                var traces = new List<CropTrace>();

                foreach (var jsonItem in jsonArray)
                {
                    // Parse từng chuỗi JSON thành đối tượng JsonElement
                    var item = JsonSerializer.Deserialize<JsonElement>(jsonItem);
                    var type = item.GetProperty("Type").GetString();

                    CropTrace trace = null;

                    switch (type)
                    {
                        case "Origin":
                            trace = new CropTrace
                            {
                                Type = "Origin",
                                CropCode = item.GetProperty("CropCode").GetInt32(),
                                Timestamp = DateTime.Parse(item.GetProperty("Timestamp").GetString()),
                                CropName = item.GetProperty("CropName").GetString(),
                                Location = item.GetProperty("Location").GetString(),
                                FarmerName = item.GetProperty("FarmerName").GetString(),
                                Description = item.GetProperty("Description").GetString(),
                                SensorId = item.GetProperty("sensorId").GetInt32()
                            };
                            break;

                        case "Spraying":
                            trace = new CropTrace
                            {
                                Type = "Spraying",
                                CropCode = item.GetProperty("CropCode").GetInt32(),
                                Timestamp = DateTime.Parse(item.GetProperty("Timestamp").GetString()),
                                PesticideName = item.GetProperty("PesticideName").GetString(),
                                Quantity = item.GetProperty("Quantity").GetInt32(),
                                Description = item.GetProperty("Description").GetString()
                            };
                            break;

                        case "Fertilizing":
                            trace = new CropTrace
                            {
                                Type = "Fertilizing",
                                CropCode = item.GetProperty("CropCode").GetInt32(),
                                Timestamp = DateTime.Parse(item.GetProperty("Timestamp").GetString()),
                                FertilizerName = item.GetProperty("FertilizerName").GetString(),
                                Quantity = item.GetProperty("Quantity").GetInt32(),
                                Description = item.GetProperty("Description").GetString()
                            };
                            break;

                        default:
                            throw new Exception($"Unknown type: {type}");
                    }

                    if (trace != null)
                    {
                        traces.Add(trace);
                    }
                }

                return traces;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracing crop with cropId {CropId}", cropId);
                throw new Exception($"Error tracing crop: {ex.Message}", ex);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("/Trace/Error")]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}