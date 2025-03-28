using Microsoft.AspNetCore.Mvc;
using BlockchainMVC.Interfaces;
using BlockchainMVC.DTOs;
using System.Threading.Tasks;

namespace BlockchainMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IIoTDeviceService _deviceService;
        private readonly IBlockchainService _blockchainService;

        public DeviceController(IIoTDeviceService deviceService, IBlockchainService blockchainService)
        {
            _deviceService = deviceService;
            _blockchainService = blockchainService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDevice(string deviceId, string deviceType)
        {
            var result = await _deviceService.RegisterDeviceAsync(deviceId, deviceType);
            if (result)
            {
                // Add to blockchain
                await _blockchainService.AddBlockAsync($"Device registered: {deviceId}");
            }
            return Ok(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string deviceId, string status)
        {
            var result = await _deviceService.UpdateDeviceStatusAsync(deviceId, status);
            if (result)
            {
                // Add to blockchain
                await _blockchainService.AddBlockAsync($"Device {deviceId} status updated: {status}");
            }
            return Ok(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetDeviceData(string deviceId)
        {
            var data = await _deviceService.GetDeviceDataAsync(deviceId);
            return Ok(new { data });
        }
    }
} 