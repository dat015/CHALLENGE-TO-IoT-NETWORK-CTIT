using Microsoft.AspNetCore.Mvc;
using BlockchainMVC.Interfaces;
using BlockchainMVC.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlockchainMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly ISensorService _sensorService;
        private readonly IBlockchainService _blockchainService;

        public SensorController(ISensorService sensorService, IBlockchainService blockchainService)
        {
            _sensorService = sensorService;
            _blockchainService = blockchainService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSensors()
        {
            var sensors = await _sensorService.GetAllSensorsAsync();
            return Ok(sensors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensor(int id)
        {
            var sensor = await _sensorService.GetSensorByIdAsync(id);
            if (sensor == null)
                return NotFound();
            return Ok(sensor);
        }

        [HttpGet("crop/{cropId}")]
        public async Task<IActionResult> GetSensorsByCrop(int cropId)
        {
            var sensors = await _sensorService.GetSensorsByCropIdAsync(cropId);
            return Ok(sensors);
        }

        [HttpPost]
        public async Task<IActionResult> AddSensor([FromBody] SensorDTO sensor)
        {
            var result = await _sensorService.AddSensorAsync(sensor);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Sensor added: {sensor.Name} - Type: {sensor.Type}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _sensorService.UpdateSensorStatusAsync(id, status);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Sensor {id} status updated: {status}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateSensor(int id)
        {
            var result = await _sensorService.ValidateSensorAsync(id);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Sensor {id} validated");
                return Ok(new { success = true });
            }
            return BadRequest();
        }
    }
} 