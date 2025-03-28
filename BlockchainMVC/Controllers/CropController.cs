using Microsoft.AspNetCore.Mvc;
using BlockchainMVC.Interfaces;
using BlockchainMVC.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlockchainMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CropController : ControllerBase
    {
        private readonly ICropService _cropService;
        private readonly IBlockchainService _blockchainService;

        public CropController(ICropService cropService, IBlockchainService blockchainService)
        {
            _cropService = cropService;
            _blockchainService = blockchainService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCrops()
        {
            var crops = await _cropService.GetAllCropsAsync();
            return Ok(crops);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCrop(int id)
        {
            var crop = await _cropService.GetCropByIdAsync(id);
            if (crop == null)
                return NotFound();
            return Ok(crop);
        }

        [HttpPost]
        public async Task<IActionResult> AddCrop([FromBody] CropDTO crop)
        {
            var result = await _cropService.AddCropAsync(crop);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Crop added: {crop.Name} - Type: {crop.Type}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _cropService.UpdateCropStatusAsync(id, status);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Crop {id} status updated: {status}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateCrop(int id)
        {
            var result = await _cropService.ValidateCropAsync(id);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Crop {id} validated");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpGet("{id}/origin")]
        public async Task<IActionResult> GetCropOrigin(int id)
        {
            var origin = await _cropService.GetCropOriginAsync(id);
            if (string.IsNullOrEmpty(origin))
                return NotFound();
            return Ok(new { origin });
        }
    }
} 