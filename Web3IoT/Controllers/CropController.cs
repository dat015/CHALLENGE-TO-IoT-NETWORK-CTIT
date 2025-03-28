using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web3IoT.Data;
using Web3IoT.DTO;
using Web3IoT.Models;
using ZXing;
using System.Drawing;
using System.IO;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

namespace Web3IoT.Controllers
{
    public class CropController : Controller
    {
        private readonly ILogger<CropController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public CropController(ApplicationDbContext context, ILogger<CropController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Crop
        public async Task<IActionResult> Index()
        {
            try
            {
                var crops = await _context.Crops.ToListAsync() ?? new List<Models.Crop>();
                return View(crops);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching crops");
                return View("Error");
            }
        }

        // GET: Crop/Create
        [HttpGet]
        [Route("/Crop/Create")]
        public async Task<IActionResult> Create()
        {
            var sensors = await _context.Sensors.ToListAsync() ?? new List<Sensor>();
            ViewData["Sensors"] = sensors; // Gán danh sách Sensors vào ViewData
            return View();
        }

        // POST: Crop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Crop/CreateCrop")]
        public async Task<IActionResult> CreateCrop([Bind("Name,Location,SensorId")] Models.Crop crop)
        {
            if (ModelState.IsValid)
            {
                string fullName = User.FindFirst(ClaimTypes.Name)?.Value;
                try
                {
                    var sensor = await _context.Sensors.FindAsync(crop.SensorId);
                    if (sensor == null)
                    {
                        _logger.LogError("Sensor with ID {SensorId} not found", crop.SensorId);
                        ModelState.AddModelError("", "Không tìm thấy sensor tương ứng.");
                        return View("Create");
                    }

                    var httpClient = _httpClientFactory.CreateClient();
                    httpClient.BaseAddress = new Uri("http://localhost:80/");
                    var data = new
                    {
                        cropCode = 0,
                        timestamp = DateTime.UtcNow.ToString("O"),
                        cropName = crop.Name,
                        location = crop.Location ?? "string",
                        farmerName = fullName ?? "string",
                        description = "string",
                        sensorId = sensor.SensorCode
                    };

                    string jsonPayload = JsonSerializer.Serialize(data);
                    _logger.LogInformation("Payload gửi đi: {Payload}", jsonPayload);

                    var response = await httpClient.PostAsJsonAsync("api/blockchain/register_crop", data);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("API register_crop failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                        ModelState.AddModelError("", "Không thể đăng ký crop trên blockchain.");
                        return View("Create");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API response: {Response}", responseContent);

                    var cropResponse = JsonSerializer.Deserialize<CropResponse>(responseContent);
                    int cropId = cropResponse.cropCode;

                    // Tạo mã QR
                    var qrCodeUrl = Url.Action("GetTrace", "Trace", new { code = cropId }, Request.Scheme);
                    var writer = new BarcodeWriter<Bitmap>
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new QrCodeEncodingOptions
                        {
                            Height = 400,
                            Width = 400,
                            Margin = 0
                        },
                        Renderer = new ZXing.Windows.Compatibility.BitmapRenderer() // Renderer hợp lệ
                    };

                    using (var qrCodeImage = writer.Write(qrCodeUrl))
                    {
                        using (var ms = new MemoryStream())
                        {
                            qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            string qrCodeBase64 = Convert.ToBase64String(ms.ToArray());

                            var newCrop = new Models.Crop()
                            {
                                Name = crop.Name,
                                Location = crop.Location,
                                DatePlanted = DateTime.UtcNow,
                                SensorId = crop.SensorId,
                                CropCode = cropId,
                                QRCode = qrCodeBase64
                            };
                            await _context.Crops.AddAsync(newCrop);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("New crop created successfully");

                            ViewBag.QRCodeBase64 = qrCodeBase64;
                            return View("CreateSuccess", newCrop);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating new crop");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo mới cây trồng. Vui lòng thử lại.");
                }
            }
            return View("Index");
        }
        // // GET: Crop/Details/5
        // public async Task<IActionResult> Details(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     try
        //     {
        //         var httpClient = _httpClientFactory.CreateClient();
        //         httpClient.BaseAddress = new Uri("http://localhost:80/");
        //         var response = await httpClient.GetAsync($"api/blockchain/crop/{id}");
        //         response.EnsureSuccessStatusCode();
        //         var crop = await response.Content.ReadFromJsonAsync<Crop>();

        //         if (crop == null)
        //         {
        //             return NotFound();
        //         }
        //         return View(crop);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error fetching crop details for ID: {Id}", id);
        //         return View("Error");
        //     }
        // }

        // // GET: Crop/Edit/5
        // public async Task<IActionResult> Edit(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     try
        //     {
        //         var httpClient = _httpClientFactory.CreateClient();
        //         httpClient.BaseAddress = new Uri("http://localhost:80/");
        //         var response = await httpClient.GetAsync($"api/blockchain/crop/{id}");
        //         response.EnsureSuccessStatusCode();
        //         var crop = await response.Content.ReadFromJsonAsync<Crop>();

        //         if (crop == null)
        //         {
        //             return NotFound();
        //         }
        //         return View(crop);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error fetching crop for edit with ID: {Id}", id);
        //         return View("Error");
        //     }
        // }

        // POST: Crop/Edit/5

    }
}