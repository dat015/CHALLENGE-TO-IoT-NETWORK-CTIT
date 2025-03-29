using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Web3IoT.Data;
using Web3IoT.Models;
using System.Text.Json;
using Web3IoT.DTO;

namespace Web3IoT.Controllers
{
    [Route("[controller]")]
    public class SensorController : Controller
    {
        private readonly ILogger<SensorController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;


        public SensorController(IHttpClientFactory httpClientFactory, ILogger<SensorController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("/Sensor/Error")]
        public IActionResult Error()
        {
            return View("Error!");
        }

        // GET: Sensor
        [HttpGet]
        [Route("/Sensor")]
        public async Task<IActionResult> Index()
        {
            var sensors = await _context.Sensors
                .Include(s => s.Farm)
                .Include(s => s.Crops)
                .ToListAsync() ?? new List<Sensor>();
            return View(sensors);
        }

        // GET: Sensor/Details/5
        [HttpGet]
        [Route("/Sensor/Details")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sensor = await _context.Sensors
                .Include(s => s.Farm)
                .Include(s => s.Crops)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }
            return View(sensor);
        }

        // GET: Sensor/Create
        [HttpGet]
        [Route("/Sensor/Create")]

        public IActionResult Create()
        {
            //ViewData["FarmId"] = new SelectList(_context.Farms, "Id", "Name");
            return View();
        }

        // POST: Sensor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Sensor/CreateSensor")]

        public async Task<IActionResult> CreateSensor([Bind("Name")] Sensor sensor)
        {
            var sensor_exis = await _context.Sensors.Where(s => s.Name == sensor.Name).FirstOrDefaultAsync();
            if (sensor_exis != null)
            {
                return View("Create");
            }
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost:80/");
            var new_sensor = new Sensor()
            {
                Name = sensor.Name,
                FarmId = 2
            };
            try
            {
                await _context.Sensors.AddAsync(new_sensor);
                await _context.SaveChangesAsync();

                var sensorBlockchain = new
                {
                    name = sensor.Name,
                    sensorCode = new_sensor.Id.ToString()
                };
                string jsonPayload = JsonSerializer.Serialize(sensorBlockchain);
                Console.WriteLine("Payload gửi đi: " + jsonPayload);

                var response = await httpClient.PostAsJsonAsync("api/Blockchain/register_sensor", sensorBlockchain);
                response.EnsureSuccessStatusCode();

                // Đọc nội dung response dưới dạng JSON
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("response: " + responseContent);
                // Deserialize JSON để lấy CropCode (Id)
                var cropResponse = JsonSerializer.Deserialize<SensorReponse>(responseContent);
                int sensorId = cropResponse.SensorId;
                Console.WriteLine("ok" + sensorId);
                var update_sensor = await _context.Sensors
                    .Where(s => s.Name == sensor.Name)
                    .FirstOrDefaultAsync();
                update_sensor.SensorCode = sensorId;
                await _context.SaveChangesAsync();
                return View("Index");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Create");
            }



            return View("Index");
        }

        private bool SensorExists(int id)
        {
            return _context.Sensors.Any(e => e.Id == id);
        }

        [HttpGet]
        [Route("/Sensor/SensorDataModal")]

        public IActionResult SensorDataModal(int id)
        {
            var sensor = _context.Sensors.Find(id);
            if (sensor == null) return NotFound();

            return View(sensor);
        }
        [HttpGet]
        [Route("/Sensor/DeviceControl")]
        public IActionResult DeviceControl(int id)
        {
            var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id); 
            if (sensor == null)
            {
                return NotFound();
            }
            return View(sensor);
        }
    }
}