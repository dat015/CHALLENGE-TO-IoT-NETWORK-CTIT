using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockChainForIoT.data;
using BlockChainForIoT.dto;
using Microsoft.AspNetCore.Mvc;

namespace BlockChainForIoT.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FarmController(AppDbContext context)
        {
            _context = context;
        }
         
        [HttpPost("add_new_crop")]
        public async Task<IActionResult> AddNewCrop([FromBody] model.Crop crop)
        {
            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Crop added", Crop = crop });
        }

        [HttpPost("add_new_sensor")]
        public async Task<IActionResult> AddNewSensor([FromBody] sensor_dto sensor)
        {
            var newSensor = new model.Sensor
            {
                Name = sensor.name,
                sensorCode = sensor.sensorCode
            };
            _context.Sensors.Add(newSensor);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Sensor added", Sensor = sensor });
        }
        
    }
}