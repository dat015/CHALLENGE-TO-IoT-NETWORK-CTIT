using Microsoft.AspNetCore.Mvc;
using BlockchainMVC.Interfaces;
using BlockchainMVC.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlockchainMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FertilizerController : ControllerBase
    {
        private readonly IFertilizerService _fertilizerService;
        private readonly IBlockchainService _blockchainService;

        public FertilizerController(IFertilizerService fertilizerService, IBlockchainService blockchainService)
        {
            _fertilizerService = fertilizerService;
            _blockchainService = blockchainService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFertilizers()
        {
            var fertilizers = await _fertilizerService.GetAllFertilizersAsync();
            return Ok(fertilizers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFertilizer(int id)
        {
            var fertilizer = await _fertilizerService.GetFertilizerByIdAsync(id);
            if (fertilizer == null)
                return NotFound();
            return Ok(fertilizer);
        }

        [HttpPost]
        public async Task<IActionResult> AddFertilizer([FromBody] FertilizerDTO fertilizer)
        {
            var result = await _fertilizerService.AddFertilizerAsync(fertilizer);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Fertilizer added: {fertilizer.Name} - Batch: {fertilizer.BatchNumber}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _fertilizerService.UpdateFertilizerStatusAsync(id, status);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Fertilizer {id} status updated: {status}");
                return Ok(new { success = true });
            }
            return BadRequest();
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateFertilizer(int id)
        {
            var result = await _fertilizerService.ValidateFertilizerAsync(id);
            if (result)
            {
                await _blockchainService.AddBlockAsync($"Fertilizer {id} validated");
                return Ok(new { success = true });
            }
            return BadRequest();
        }
    }
} 