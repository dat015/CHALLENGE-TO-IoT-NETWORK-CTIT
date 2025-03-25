using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlockChainForIoT.model;

namespace BlockChainForIoT.blockchain
{
    public class TransactionManager
    {
        private readonly AuthorityManager _authorityManager;
        private readonly IpfsService _ipfsService;
        private readonly NetworkService _networkService;
        private readonly List<block> _chain;

        public TransactionManager(AuthorityManager authorityManager, IpfsService ipfsService, NetworkService networkService, List<block> chain)
        {
            _authorityManager = authorityManager;
            _ipfsService = ipfsService;
            _networkService = networkService;
            _chain = chain;
        }

        public async Task AddBlock(ITransaction transaction)
        {
            if (_authorityManager.PrivateKey == null)
            {
                throw new InvalidOperationException("This node is not an authority and cannot add blocks.");
            }

            // Lưu trữ giao dịch trên Pinata và nhận CID
            string jsonData = transaction.ToJsonString();
            string blockCid = await _ipfsService.PublishToPinataAsync(jsonData, $"transaction_{DateTime.UtcNow.Ticks}.json");

            // Tạo block mới
            string authorityPublicKey = Convert.ToBase64String(_authorityManager.PrivateKey.ExportRSAPublicKey());
            block newBlock = new block(_chain.Last().Index + 1, blockCid, _chain.Last().Hash, authorityPublicKey);

            // Ký block
            string dataToSign = newBlock.GetDataToSign();
            byte[] signature = _authorityManager.PrivateKey.SignData(Encoding.UTF8.GetBytes(dataToSign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            newBlock.Signature = Convert.ToBase64String(signature);
            newBlock.Hash = newBlock.CalculateHash();

            // Thêm block vào chuỗi
            _chain.Add(newBlock);

            // Cập nhật IdToCids
            UpdateDictionaries(jsonData, blockCid);

            // Cập nhật metadata và chuỗi trên Pinata
            await _ipfsService.UpdateMetadataOnIpfs();
            await _ipfsService.PublishToIpfsAsync(_chain);
            await _networkService.BroadcastBlockAsync(newBlock);
        }

        private void UpdateDictionaries(string jsonData, string blockCid)
        {
            var transactionData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
            int? cropCode = ExtractInt(transactionData, "CropCode");
            if (cropCode.HasValue)
            {
                if (!_ipfsService.CropCodeToCids.ContainsKey(cropCode.Value)) _ipfsService.CropCodeToCids[cropCode.Value] = new List<string>();
                _ipfsService.CropCodeToCids[cropCode.Value].Add(blockCid);
            }

            int? sensorId = ExtractInt(transactionData, "SensorId");
            if (sensorId.HasValue)
            {
                if (!_ipfsService.SensorIdToCids.ContainsKey(sensorId.Value)) _ipfsService.SensorIdToCids[sensorId.Value] = new List<string>();
                _ipfsService.SensorIdToCids[sensorId.Value].Add(blockCid);
            }

            int? actionId = ExtractInt(transactionData, "ActionId");
            if (actionId.HasValue)
            {
                if (!_ipfsService.ActionIdToCids.ContainsKey(actionId.Value)) _ipfsService.ActionIdToCids[actionId.Value] = new List<string>();
                _ipfsService.ActionIdToCids[actionId.Value].Add(blockCid);
            }
        }

        private int? ExtractInt(Dictionary<string, JsonElement> data, string key)
        {
            if (data.ContainsKey(key) && data[key].ValueKind != JsonValueKind.Null)
            {
                return data[key].ValueKind == JsonValueKind.Number ? data[key].GetInt32() : int.Parse(data[key].GetString());
            }
            return null;
        }
    }
}