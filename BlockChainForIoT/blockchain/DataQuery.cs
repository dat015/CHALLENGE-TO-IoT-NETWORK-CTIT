using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.blockchain
{
    public class DataQuery
    {
        private readonly IpfsService _ipfsService;
        private readonly List<block> _chain;

        public DataQuery(IpfsService ipfsService, List<block> chain)
        {
            _ipfsService = ipfsService;
            _chain = chain;
        }

        public async Task<List<block>> GetChain()
        {
            if(!_ipfsService.LoadBlockDataFromIpfs(_chain[0].IpfsCid).Result.Contains("Index")) return new List<block>();
            return _chain;
        }
        public async Task<List<string>> GetBlocksByCropCode(int cropCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!_ipfsService.CropCodeToCids.ContainsKey(cropCode)) return new List<string>();
            var blockCids = _ipfsService.CropCodeToCids[cropCode];
            var result = new List<string>();

            foreach (var cid in blockCids)
            {
                try
                {
                    string jsonData = await _ipfsService.LoadBlockDataFromIpfs(cid);
                    var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
                    DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());

                    if (transaction["CropCode"].GetInt32() == cropCode &&
                        (startDate == null || timestamp >= startDate) &&
                        (endDate == null || timestamp <= endDate))
                    {
                        result.Add(jsonData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing CID {cid}: {ex.Message}");
                }
            }
            return result;
        }

        public async Task<List<string>> GetBlocksBySensorId(int sensorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!_ipfsService.SensorIdToCids.ContainsKey(sensorId)) return new List<string>();

            // Sao chép danh sách để tránh lỗi khi tập hợp bị thay đổi
            var blockCids = _ipfsService.SensorIdToCids[sensorId].ToList();
            Console.WriteLine($"SensorId: {sensorId}, blockCids: {blockCids.Count}");
            var result = new List<string>();

            foreach (var cid in blockCids)
            {
                try
                {
                    string jsonData = await _ipfsService.LoadBlockDataFromIpfs(cid);
                    var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
                    DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());
                    Console.WriteLine($"SensorId: {transaction["SensorId"].GetInt32()}");
                    if (transaction["SensorId"].GetInt32() == sensorId &&
                        (startDate == null || timestamp >= startDate) &&
                        (endDate == null || timestamp <= endDate))
                    {
                        result.Add(jsonData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing CID {cid}: {ex.Message}");
                }
            }
            return result;
        }

        public async Task<List<string>> GetBlocksByActionId(int actionId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!_ipfsService.ActionIdToCids.ContainsKey(actionId)) return new List<string>();
            var blockCids = _ipfsService.ActionIdToCids[actionId];
            var result = new List<string>();

            foreach (var cid in blockCids)
            {
                try
                {
                    string jsonData = await _ipfsService.LoadBlockDataFromIpfs(cid);
                    var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
                    DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());

                    if (transaction["ActionId"].GetInt32() == actionId &&
                        (startDate == null || timestamp >= startDate) &&
                        (endDate == null || timestamp <= endDate))
                    {
                        result.Add(jsonData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing CID {cid}: {ex.Message}");
                }
            }
            return result;
        }
    }
}