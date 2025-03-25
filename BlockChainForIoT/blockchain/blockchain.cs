using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BlockChainForIoT.model;
using Microsoft.Extensions.Configuration;

namespace BlockChainForIoT.blockchain
{
    public class Blockchain
    {
        public List<block> Chain { get; private set; }
        public string MetadataCid { get; private set; }
        public string LatestIpfsCid { get; private set; }

        private readonly IpfsService _ipfsService;
        private readonly NetworkService _networkService;
        private readonly AuthorityManager _authorityManager;
        private readonly TransactionManager _transactionManager;
        private readonly DataQuery _dataQuery;

        public Blockchain(IConfiguration config)
        {
            try
            {
                Chain = new List<block>();

                var httpClient = new HttpClient();
                Console.WriteLine("Creating IpfsService...");
                _ipfsService = new IpfsService(config, httpClient);

                Console.WriteLine("Creating AuthorityManager...");
                _authorityManager = new AuthorityManager(httpClient, config);

                Console.WriteLine("Creating NetworkService...");
                _networkService = new NetworkService(_authorityManager.AuthorityPublicKeys, config["node:address"], config.GetSection("peerNodes").Get<List<string>>(), httpClient);

                Console.WriteLine("Creating TransactionManager...");
                _transactionManager = new TransactionManager(_authorityManager, _ipfsService, _networkService, Chain);

                Console.WriteLine("Creating DataQuery...");
                _dataQuery = new DataQuery(_ipfsService, Chain);

                Console.WriteLine("Blockchain initialized.");
                InitializeChain();

                Console.WriteLine("Starting peer sync...");
                Task.Run(() => _networkService.SyncWithPeersAsync(Chain));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Blockchain constructor: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw; // Ném lại để debug dễ hơn
            }
        }
        public void ClearChain()
        {
            Chain.Clear();
            Console.WriteLine("Chain cleared.");
        }
        private void InitializeChain()
        {

            _ipfsService.LoadMetadataCid();
            MetadataCid = _ipfsService.MetadataCid;
            LatestIpfsCid = _ipfsService.LatestIpfsCid;
            Console.WriteLine($"Loaded from file: MetadataCid={MetadataCid}, LatestIpfsCid={LatestIpfsCid}");

            if (!string.IsNullOrEmpty(MetadataCid))
            {
                _ipfsService.RestoreDictionariesFromIpfs().GetAwaiter().GetResult();
            }
            if (!string.IsNullOrEmpty(LatestIpfsCid))
            {
                _ipfsService.RestoreChainFromIpfs(Chain).GetAwaiter().GetResult();
            }
            else
            {
                _authorityManager.AddGenesisBlock(Chain);
            }
        }

        
        public async Task AddAuthorityNodeAsync(string publicKey)
        {
            await _authorityManager.AddAuthorityNodeAsync(publicKey, _networkService.PeerNodes);
        }

        public async Task AddBlock(ITransaction transaction)
        {
            await _transactionManager.AddBlock(transaction);
            MetadataCid = _ipfsService.MetadataCid;
            LatestIpfsCid = _ipfsService.LatestIpfsCid;
        }

        public bool IsChainValid(List<block> chain = null)
        {
            Console.WriteLine("ok");

            Console.WriteLine(Chain.ToString());
            return _authorityManager.IsChainValid(Chain);
        }

        public List<block> GetChain() => Chain;
        public block GetLatestBlock() => Chain.Last();

        // Truy vấn dữ liệu
        public async Task<List<string>> GetBlocksByCropCode(int cropCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _dataQuery.GetBlocksByCropCode(cropCode, startDate, endDate);
        }

        public async Task<List<string>> GetBlocksBySensorId(int sensorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _dataQuery.GetBlocksBySensorId(sensorId, startDate, endDate);
        }

        public async Task<List<string>> GetBlocksByActionId(int actionId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _dataQuery.GetBlocksByActionId(actionId, startDate, endDate);
        }
    }
}













// using System.Security.Cryptography;
// using System.Text;
// using System.Text.Json;
// using BlockChainForIoT.blockchain;
// using BlockChainForIoT.model;
// using Ipfs.Http;
// using Microsoft.Extensions.Configuration; // Thêm dòng này

// public class Blockchain
// {
//     public List<block> Chain { get; set; }
//     private IpfsClient ipfs;
//     private readonly HttpClient httpClient;
//     public string MetadataCid { get; private set; }
//     public string LatestIpfsCid { get; private set; }
//     private Dictionary<int, List<string>> cropCodeToCids;
//     private Dictionary<int, List<string>> sensorIdToCids;
//     private Dictionary<int, List<string>> actionIdToCids;
//     private const string MetadataCidFile = "metadata_cid.json";
//     private List<string> authorityPublicKeys; // Danh sách khóa công khai của authorities
//     private RSA privateKey; // Khóa riêng của node hiện tại (nếu là authority) 
//     private List<string> peerNodes; // Danh sách địa chỉ các node khác (ví dụ: "http://node2:5002")
//     private readonly string nodeAddress; // Địa chỉ của node hiện tại (ví dụ: "http://node1:5001")
//     private readonly string PinataApiKey;
//     private readonly string PinataSecretApiKey;
//     private readonly string PinataJwt;
//     private readonly string PinataApiUrl = "https://api.pinata.cloud/pinning/pinFileToIPFS";

//     public Blockchain()
//     {
//         IConfiguration config = new ConfigurationBuilder()
//             .SetBasePath(Directory.GetCurrentDirectory())
//             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//             .Build();

//         PinataApiKey = config["pinata:pinata_key"];
//         PinataSecretApiKey = config["pinata:pinata_secret"];
//         PinataJwt = config["pinata:pinata_jwt"];

//         nodeAddress = config["node:address"]; // Ví dụ: "http://node1:5001"
//         peerNodes = config.GetSection("peerNodes").Get<List<string>>() ?? new List<string>();

//         authorityPublicKeys = config.GetSection("authorityPublicKeys").Get<List<string>>().ToList();
//         string privateKeyBase64 = config["authority:private_key"];
//         if (!string.IsNullOrEmpty(privateKeyBase64))
//         {
//             privateKey = RSA.Create();
//             privateKey.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);
//         }
//         if (!string.IsNullOrEmpty(privateKeyBase64))
//         {
//             privateKey = RSA.Create();
//             privateKey.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);
//             Console.WriteLine("Authority private key loaded.");

//             // Thêm public key của chính node này nếu chưa có
//             string myPublicKey = Convert.ToBase64String(privateKey.ExportRSAPublicKey());
//             if (!authorityPublicKeys.Contains(myPublicKey))
//             {
//                 authorityPublicKeys.Add(myPublicKey);
//             }
//         }



//         Chain = new List<block>();
//         ipfs = new IpfsClient("http://127.0.0.1:5001");
//         httpClient = new HttpClient();
//         cropCodeToCids = new Dictionary<int, List<string>>();
//         sensorIdToCids = new Dictionary<int, List<string>>();
//         actionIdToCids = new Dictionary<int, List<string>>();

//         LoadMetadataCid();
//         if (!string.IsNullOrEmpty(MetadataCid))
//         {
//             RestoreDictionariesFromIpfs().GetAwaiter().GetResult();
//         }
//         if (!string.IsNullOrEmpty(LatestIpfsCid))
//         {
//             RestoreChainFromIpfs().GetAwaiter().GetResult();
//         }
//         else
//         {
//             AddGenesisBlock();
//         }

//         // Đồng bộ với các node khác khi khởi động
//         Task.Run(() => SyncWithPeersAsync());
//     }

//     // Thêm authority node và thông báo cho các node khác
//     public async Task AddAuthorityNodeAsync(string publicKey)
//     {
//         if (!authorityPublicKeys.Contains(publicKey))
//         {
//             authorityPublicKeys.Add(publicKey);
//             Console.WriteLine($"Added new authority public key: {publicKey}");

//             // Thông báo cho các node khác
//             foreach (var peer in peerNodes)
//             {
//                 try
//                 {
//                     await httpClient.PostAsJsonAsync($"{peer}/api/blockchain/add-authority", publicKey);
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"Failed to notify peer {peer}: {ex.Message}");
//                 }
//             }
//         }
//     }

//     private async Task<string> PublishToPinataAsync(string jsonData, string fileName)
//     {
//         try
//         {
//             httpClient.DefaultRequestHeaders.Clear();
//             httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {PinataJwt}");
//             Console.WriteLine($"Sending request to Pinata with JWT: {PinataJwt}");

//             var content = new MultipartFormDataContent
//         {
//             { new StringContent(jsonData), "file", fileName }
//         };

//             var response = await httpClient.PostAsync(PinataApiUrl, content);
//             if (!response.IsSuccessStatusCode)
//             {
//                 string error = await response.Content.ReadAsStringAsync();
//                 throw new Exception($"Pinata upload failed: {response.StatusCode} - {error}");
//             }

//             string result = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Response from Pinata: {result}"); // Thêm log để kiểm tra JSON
//             var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(result);
//             string cid = jsonResult["IpfsHash"].ToString(); // Chuyển đổi object thành string
//             Console.WriteLine($"Published to Pinata with CID: {cid}");
//             return cid;
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error publishing to Pinata: {ex.Message}");
//             throw;
//         }
//     }
//     // Đồng bộ với các node khác
//     private async Task SyncWithPeersAsync()
//     {
//         foreach (var peer in peerNodes)
//         {
//             try
//             {
//                 var response = await httpClient.GetAsync($"{peer}/api/blockchain/chain");
//                 if (response.IsSuccessStatusCode)
//                 {
//                     string json = await response.Content.ReadAsStringAsync();
//                     var peerChain = JsonSerializer.Deserialize<List<block>>(json);
//                     if (peerChain.Count > Chain.Count && IsChainValid(peerChain))
//                     {
//                         Chain = peerChain;
//                         LatestIpfsCid = await LoadBlockDataFromIpfs(peerChain.Last().IpfsCid);
//                         Console.WriteLine($"Synced chain from {peer}");
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Failed to sync with {peer}: {ex.Message}");
//             }
//         }
//     }
//     public async Task<string> PublishToIpfsAsync()
//     {
//         string json = JsonSerializer.Serialize(Chain);
//         LatestIpfsCid = await PublishToPinataAsync(json, "chain.json");

//         // Tùy chọn: Thêm vào node cục bộ và pin
//         try
//         {
//             using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
//             var ipfsFile = await ipfs.FileSystem.AddAsync(stream, "chain.json");
//             await ipfs.Pin.AddAsync(LatestIpfsCid);
//             Console.WriteLine($"Chain also pinned to local IPFS with CID: {LatestIpfsCid}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Warning: Failed to pin to local IPFS: {ex.Message}");
//         }

//         Console.WriteLine($"Chain published to IPFS and Pinata with CID: {LatestIpfsCid}");
//         return LatestIpfsCid;
//     }

//     private async Task UpdateMetadataOnIpfs()
//     {
//         var metadata = new
//         {
//             CropCodeToCids = cropCodeToCids,
//             SensorIdToCids = sensorIdToCids,
//             ActionIdToCids = actionIdToCids
//         };
//         string metadataJson = JsonSerializer.Serialize(metadata);
//         MetadataCid = await PublishToPinataAsync(metadataJson, "metadata.json");

//         // Tùy chọn: Thêm vào node cục bộ và pin
//         try
//         {
//             var content = new MultipartFormDataContent
//             {
//                 { new StringContent(metadataJson), "file", "metadata.json" }
//             };
//             var response = await httpClient.PostAsync("http://127.0.0.1:5001/api/v0/add", content);
//             response.EnsureSuccessStatusCode();
//             await ipfs.Pin.AddAsync(MetadataCid);
//             Console.WriteLine($"Metadata also pinned to local IPFS with CID: {MetadataCid}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Warning: Failed to pin metadata to local IPFS: {ex.Message}");
//         }

//         Console.WriteLine($"Metadata published to IPFS and Pinata with CID: {MetadataCid}");
//     }

//     public async Task<string> LoadBlockDataFromIpfs(string cid)
//     {
//         try
//         {
//             if (string.IsNullOrEmpty(cid))
//                 throw new ArgumentException("CID cannot be null or empty");

//             Console.WriteLine($"Attempting to load CID: {cid}");
//             HttpResponseMessage response;

//             // Thử lấy từ node cục bộ trước
//             response = await httpClient.GetAsync($"http://127.0.0.1:8080/ipfs/{cid}");
//             if (!response.IsSuccessStatusCode)
//             {
//                 Console.WriteLine("Local IPFS failed, trying Pinata Gateway...");
//                 response = await httpClient.GetAsync($"https://gateway.pinata.cloud/ipfs/{cid}");
//             }

//             response.EnsureSuccessStatusCode();
//             string json = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"IPFS response for CID {cid}: {json}");
//             return json;
//         }
//         catch (HttpRequestException ex)
//         {
//             Console.WriteLine($"Error loading from IPFS for CID {cid}: {ex.Message}");
//             throw;
//         }
//     }

//     private async Task RestoreChainFromIpfs()
//     {
//         if (!string.IsNullOrEmpty(LatestIpfsCid))
//         {
//             try
//             {
//                 string json = await LoadBlockDataFromIpfs(LatestIpfsCid);
//                 Chain = JsonSerializer.Deserialize<List<block>>(json);
//                 Console.WriteLine($"Chain restored from IPFS with {Chain.Count} blocks:");
//                 foreach (var block in Chain)
//                 {
//                     Console.WriteLine($"Block {block.Index}: CID={block.IpfsCid}, Hash={block.Hash}");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error restoring Chain from IPFS: {ex.Message}");
//                 Chain = new List<block>();
//                 AddGenesisBlock();
//             }
//         }
//     }

//     private async Task RestoreDictionariesFromIpfs()
//     {
//         try
//         {
//             string metadataJson = await LoadBlockDataFromIpfs(MetadataCid);
//             var metadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, List<string>>>>(metadataJson);
//             cropCodeToCids = metadata.GetValueOrDefault("CropCodeToCids") ?? new Dictionary<int, List<string>>();
//             sensorIdToCids = metadata.GetValueOrDefault("SensorIdToCids") ?? new Dictionary<int, List<string>>();
//             actionIdToCids = metadata.GetValueOrDefault("ActionIdToCids") ?? new Dictionary<int, List<string>>();
//             Console.WriteLine($"Dictionaries restored from IPFS: CropCodeToCids={cropCodeToCids.Count}, SensorIdToCids={sensorIdToCids.Count}, ActionIdToCids={actionIdToCids.Count}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error restoring dictionaries from IPFS: {ex.Message}");
//         }
//     }

//     private void LoadMetadataCid()
//     {
//         if (File.Exists(MetadataCidFile))
//         {
//             string json = File.ReadAllText(MetadataCidFile);
//             var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
//             MetadataCid = data.GetValueOrDefault("MetadataCid");
//             LatestIpfsCid = data.GetValueOrDefault("LatestIpfsCid");
//             Console.WriteLine($"Loaded from file: MetadataCid={MetadataCid}, LatestIpfsCid={LatestIpfsCid}");
//         }
//     }

//     private async Task SaveMetadataCid()
//     {
//         var data = new Dictionary<string, string>
//         {
//             { "MetadataCid", MetadataCid },
//             { "LatestIpfsCid", LatestIpfsCid }
//         };
//         string json = JsonSerializer.Serialize(data);
//         File.WriteAllText(MetadataCidFile, json);
//         Console.WriteLine($"Saved to file: MetadataCid={MetadataCid}, LatestIpfsCid={LatestIpfsCid}");
//     }

//     private void AddGenesisBlock()
//     {
//         if (Chain.Count == 0)
//         {
//             string genesisAuthorityKey = authorityPublicKeys.FirstOrDefault() ?? Convert.ToBase64String(privateKey?.ExportRSAPublicKey() ?? RSA.Create().ExportRSAPublicKey());
//             block genesisBlock = new block(0, "QmGenesisBlock", "0", genesisAuthorityKey);
//             if (privateKey != null)
//             {
//                 genesisBlock.Signature = Convert.ToBase64String(privateKey.SignData(Encoding.UTF8.GetBytes(genesisBlock.GetDataToSign()), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
//             }
//             else
//             {
//                 genesisBlock.Signature = "GENESIS_SIGNATURE";
//             }
//             genesisBlock.Hash = genesisBlock.CalculateHash();
//             Chain.Add(genesisBlock);
//             Console.WriteLine("Genesis block added.");
//         }
//     }

//     public block GetLatestBlock()
//     {
//         return Chain.Last();
//     }

//     public async Task AddBlock(ITransaction transaction)
//     {
//         if (privateKey == null)
//         {
//             throw new InvalidOperationException("This node is not an authority and cannot add blocks.");
//         }

//         string jsonData = transaction.ToJsonString();
//         var content = new MultipartFormDataContent
//         {
//             { new StringContent(jsonData), "file", "block.json" }
//         };
//         var response = await httpClient.PostAsync("http://127.0.0.1:5001/api/v0/add", content);
//         response.EnsureSuccessStatusCode();
//         string result = await response.Content.ReadAsStringAsync();
//         var jsonResult = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
//         string blockCid = jsonResult["Hash"];

//         string authorityPublicKey = Convert.ToBase64String(privateKey.ExportRSAPublicKey());
//         block newBlock = new block(GetLatestBlock().Index + 1, blockCid, GetLatestBlock().Hash, authorityPublicKey); Console.WriteLine($"Mining block {newBlock.Index}...");


//         // Ký block
//         string dataToSign = newBlock.GetDataToSign();
//         byte[] signature = privateKey.SignData(Encoding.UTF8.GetBytes(dataToSign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
//         newBlock.Signature = Convert.ToBase64String(signature);
//         newBlock.Hash = newBlock.CalculateHash();

//         Chain.Add(newBlock);

//         var transactionData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

//         int? cropCode = null;
//         if (transactionData.ContainsKey("CropCode") && transactionData["CropCode"].ValueKind != JsonValueKind.Null)
//         {
//             cropCode = transactionData["CropCode"].ValueKind == JsonValueKind.Number
//                 ? transactionData["CropCode"].GetInt32()
//                 : int.Parse(transactionData["CropCode"].GetString());
//             if (!cropCodeToCids.ContainsKey(cropCode.Value))
//                 cropCodeToCids[cropCode.Value] = new List<string>();
//             cropCodeToCids[cropCode.Value].Add(blockCid);
//         }

//         if (transactionData.ContainsKey("SensorId") && transactionData["SensorId"].ValueKind != JsonValueKind.Null)
//         {
//             int sensorId = transactionData["SensorId"].ValueKind == JsonValueKind.Number
//                 ? transactionData["SensorId"].GetInt32()
//                 : int.Parse(transactionData["SensorId"].GetString());
//             if (!sensorIdToCids.ContainsKey(sensorId))
//                 sensorIdToCids[sensorId] = new List<string>();
//             sensorIdToCids[sensorId].Add(blockCid);
//         }

//         if (transactionData.ContainsKey("ActionId") && transactionData["ActionId"].ValueKind != JsonValueKind.Null)
//         {
//             int actionId = transactionData["ActionId"].ValueKind == JsonValueKind.Number
//                 ? transactionData["ActionId"].GetInt32()
//                 : int.Parse(transactionData["ActionId"].GetString());
//             if (!actionIdToCids.ContainsKey(actionId))
//                 actionIdToCids[actionId] = new List<string>();
//             actionIdToCids[actionId].Add(blockCid);
//         }

//         await UpdateMetadataOnIpfs();
//         await PublishToIpfsAsync();
//         await SaveMetadataCid();

//         // Thông báo block mới cho các node khác
//         await BroadcastBlockAsync(newBlock);
//     }

//     // private void MineBlock(block block)
//     // {
//     //     string target = new string('0', block.Difficulty);
//     //     while (true)
//     //     {
//     //         block.Hash = block.CalculateHash();
//     //         string hashHex = BitConverter.ToString(Convert.FromBase64String(block.Hash)).Replace("-", "").ToLower();
//     //         if (hashHex.StartsWith(target))
//     //         {
//     //             break;
//     //         }
//     //         block.Nonce++;
//     //     }
//     // }
//     private async Task BroadcastBlockAsync(block newBlock)
//     {
//         foreach (var peer in peerNodes)
//         {
//             try
//             {
//                 await httpClient.PostAsJsonAsync($"{peer}/api/blockchain/add-block", newBlock);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Failed to broadcast block to {peer}: {ex.Message}");
//             }
//         }
//     }
//     public async Task<List<string>> GetBlocksByCropCode(int cropCode, DateTime? startDate = null, DateTime? endDate = null)
//     {
//         if (!cropCodeToCids.ContainsKey(cropCode))
//         {
//             Console.WriteLine($"No transactions found for CropCode {cropCode}");
//             return new List<string>();
//         }

//         var blockCids = cropCodeToCids[cropCode];
//         var filteredTransactions = new List<string>();

//         foreach (var cid in blockCids)
//         {
//             try
//             {
//                 string jsonData = await LoadBlockDataFromIpfs(cid);
//                 var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
//                 DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());

//                 if (transaction.ContainsKey("CropCode") && transaction["CropCode"].ValueKind != JsonValueKind.Null)
//                 {
//                     int transCropCode = transaction["CropCode"].ValueKind == JsonValueKind.Number
//                         ? transaction["CropCode"].GetInt32()
//                         : int.Parse(transaction["CropCode"].GetString());

//                     if (transCropCode == cropCode &&
//                         (startDate == null || timestamp >= startDate) &&
//                         (endDate == null || timestamp <= endDate))
//                     {
//                         filteredTransactions.Add(jsonData);
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error processing CID {cid} for CropCode {cropCode}: {ex.Message}");
//             }
//         }
//         Console.WriteLine($"Found {filteredTransactions.Count} transactions for CropCode {cropCode}");
//         return filteredTransactions;
//     }

//     public async Task<List<string>> GetBlocksBySensorId(int sensorId, DateTime? startDate = null, DateTime? endDate = null)
//     {
//         if (!sensorIdToCids.ContainsKey(sensorId))
//         {
//             Console.WriteLine($"No transactions found for SensorId {sensorId}");
//             return new List<string>();
//         }

//         var blockCids = sensorIdToCids[sensorId];
//         var filteredTransactions = new List<string>();

//         foreach (var cid in blockCids)
//         {
//             try
//             {
//                 string jsonData = await LoadBlockDataFromIpfs(cid);
//                 var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
//                 DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());

//                 if (transaction.ContainsKey("SensorId") && transaction["SensorId"].ValueKind != JsonValueKind.Null)
//                 {
//                     int transSensorId = transaction["SensorId"].ValueKind == JsonValueKind.Number
//                         ? transaction["SensorId"].GetInt32()
//                         : int.Parse(transaction["SensorId"].GetString());

//                     if (transSensorId == sensorId &&
//                         (startDate == null || timestamp >= startDate) &&
//                         (endDate == null || timestamp <= endDate))
//                     {
//                         filteredTransactions.Add(jsonData);
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error processing CID {cid} for SensorId {sensorId}: {ex.Message}");
//             }
//         }
//         Console.WriteLine($"Found {filteredTransactions.Count} transactions for SensorId {sensorId}");
//         return filteredTransactions;
//     }

//     public async Task<List<string>> GetBlocksByActionId(int actionId, DateTime? startDate = null, DateTime? endDate = null)
//     {
//         if (!actionIdToCids.ContainsKey(actionId))
//         {
//             Console.WriteLine($"No transactions found for ActionId {actionId}");
//             return new List<string>();
//         }

//         var blockCids = actionIdToCids[actionId];
//         var filteredTransactions = new List<string>();

//         foreach (var cid in blockCids)
//         {
//             try
//             {
//                 string jsonData = await LoadBlockDataFromIpfs(cid);
//                 var transaction = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);
//                 DateTime timestamp = DateTime.Parse(transaction["Timestamp"].GetString());

//                 if (transaction.ContainsKey("ActionId") && transaction["ActionId"].ValueKind != JsonValueKind.Null)
//                 {
//                     int transActionId = transaction["ActionId"].ValueKind == JsonValueKind.Number
//                         ? transaction["ActionId"].GetInt32()
//                         : int.Parse(transaction["ActionId"].GetString());

//                     if (transActionId == actionId &&
//                         (startDate == null || timestamp >= startDate) &&
//                         (endDate == null || timestamp <= endDate))
//                     {
//                         filteredTransactions.Add(jsonData);
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error processing CID {cid} for ActionId {actionId}: {ex.Message}");
//             }
//         }
//         Console.WriteLine($"Found {filteredTransactions.Count} transactions for ActionId {actionId}");
//         return filteredTransactions;
//     }

//     public string EncryptData(ITransaction data, string key)
//     {
//         using (Aes aes = Aes.Create())
//         {
//             aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
//             aes.IV = new byte[16];
//             ICryptoTransform encryptor = aes.CreateEncryptor();
//             byte[] encrypted = encryptor.TransformFinalBlock(
//                 Encoding.UTF8.GetBytes(data.ToJsonString()),
//                 0,
//                 data.ToJsonString().Length
//             );
//             return Convert.ToBase64String(encrypted);
//         }
//     }

//     public bool IsChainValid(List<block> chain = null)
//     {
//         chain = chain ?? Chain;
//         for (int i = 1; i < chain.Count; i++)
//         {
//             block currentBlock = chain[i];
//             block previousBlock = chain[i - 1];

//             if (currentBlock.Hash != currentBlock.CalculateHash() || currentBlock.PreviousHash != previousBlock.Hash)
//                 return false;

//             if (!authorityPublicKeys.Contains(currentBlock.AuthorityPublicKey))
//                 return false;

//             RSA publicKey = RSA.Create();
//             publicKey.ImportRSAPublicKey(Convert.FromBase64String(currentBlock.AuthorityPublicKey), out _);
//             byte[] signature = Convert.FromBase64String(currentBlock.Signature);
//             if (!publicKey.VerifyData(Encoding.UTF8.GetBytes(currentBlock.GetDataToSign()), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
//                 return false;
//         }
//         return true;
//     }

//     public List<block> GetChain() => Chain;
// }