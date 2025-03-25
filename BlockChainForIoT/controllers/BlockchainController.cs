using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockChainForIoT.blockchain;
using BlockChainForIoT.model;
using BlockChainForIoT.data;
using System.Text.Json;

namespace BlockChainForIoT.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly Blockchain _blockchain;
        private readonly AppDbContext _context;

        public BlockchainController(Blockchain blockchain, AppDbContext context)
        {
            _blockchain = blockchain;
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBlock([FromBody] ITransaction data)
        {
            try
            {
                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Block added", BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error adding block", Error = ex.Message });
            }
        }

        [HttpPost("add_trans_sensor")]
        public async Task<IActionResult> AddBlockTransSensor([FromBody] TransactionSensor data)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(data.SensorId);
                if (sensor == null)
                    return NotFound(new { Message = "Thiết bị chưa đăng ký!", Success = false });

                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Block added", BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error adding sensor transaction", Error = ex.Message });
            }
        }

        [HttpPost("add_trans_action")]
        public async Task<IActionResult> AddBlockTransAction([FromBody] TransactionAction data)
        {
            try
            {
                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Block added", BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error adding action transaction", Error = ex.Message });
            }
        }

        [HttpPost("register_crop")]
        public async Task<IActionResult> RegisterCrop([FromBody] TransactionOrigin data)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(data.sensorId);
                if (sensor == null)
                    return NotFound(new { Message = "Thiết bị chưa đăng ký!", Success = false });

                var crop = new Crop
                {
                    Name = data.CropName,
                    sensorId = data.sensorId,
                    DatePlanted = data.Timestamp
                };
                _context.Crops.Add(crop);
                await _context.SaveChangesAsync();

                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Crop registered", CropCode = data.CropCode, BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error registering crop", Error = ex.Message });
            }
        }

        [HttpPost("add_spraying")]
        public async Task<IActionResult> AddSpraying([FromBody] TransactionSpraying data)
        {
            try
            {
                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Spraying data added", BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error adding spraying data", Error = ex.Message });
            }
        }

        [HttpPost("add_fertilizing")]
        public async Task<IActionResult> AddFertilizing([FromBody] TransactionFertilizing data)
        {
            try
            {
                await _blockchain.AddBlock(data);
                var latestBlock = _blockchain.GetLatestBlock();
                return Ok(new { Message = "Fertilizing data added", BlockIndex = latestBlock.Index, IpfsCid = latestBlock.IpfsCid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error adding fertilizing data", Error = ex.Message });
            }
        }

        [HttpGet("chain")]
        public ActionResult<List<block>> GetChain()
        {
            Console.WriteLine("GetChain");
            if (!_blockchain.IsChainValid())
                return BadRequest(new { Message = "Blockchain is invalid" });
            Console.WriteLine("ok");

            return Ok(_blockchain.GetChain());
        }

        // [HttpGet("ipfs/{cid}")]
        // public async Task<IActionResult> GetChainFromIpfs(string cid)
        // {
        //     try
        //     {
        //         // Sử dụng LoadBlockDataFromIpfs thay vì LoadFromIpfsAsync để đồng bộ với Blockchain
        //         string json = await _blockchain.LoadBlockDataFromIpfs(cid);
        //         var chain = JsonSerializer.Deserialize<dynamic>(json);
        //         return Ok(chain);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { Message = "Error loading chain from IPFS", Error = ex.Message });
        //     }
        // }

        [HttpGet("trace/crop/{cropCode}")]
        public async Task<IActionResult> TraceCrop(int cropCode, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var transactions = await _blockchain.GetBlocksByCropCode(cropCode, startDate, endDate);
                if (transactions.Count == 0)
                    return NotFound(new { Message = "Không tìm thấy giao dịch cho CropCode này." });

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error tracing crop", Error = ex.Message });
            }
        }

        [HttpGet("trace/sensor/{sensorId}")]
        public async Task<IActionResult> TraceSensor(int sensorId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var transactions = await _blockchain.GetBlocksBySensorId(sensorId, startDate, endDate);
                if (transactions.Count == 0)
                    return NotFound(new { Message = "Không tìm thấy giao dịch cho SensorId này." });

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error tracing sensor", Error = ex.Message });
            }
        }

        [HttpGet("trace/action/{actionId}")]
        public async Task<IActionResult> TraceAction(int actionId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var transactions = await _blockchain.GetBlocksByActionId(actionId, startDate, endDate);
                if (transactions.Count == 0)
                    return NotFound(new { Message = "Không tìm thấy giao dịch cho ActionId này." });

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error tracing action", Error = ex.Message });
            }
        }
    }
}