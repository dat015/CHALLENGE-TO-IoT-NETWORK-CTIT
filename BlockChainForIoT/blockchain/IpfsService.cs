using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlockChainForIoT.blockchain
{
    public class IpfsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _pinataApiKey;
        private readonly string _pinataSecretApiKey;
        private readonly string _pinataJwt;
        private readonly string _pinataApiUrl = "https://api.pinata.cloud/pinning/pinFileToIPFS";

        public string MetadataCid { get; private set; }
        public string LatestIpfsCid { get; private set; }
        public Dictionary<int, List<string>> CropCodeToCids { get; private set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> SensorIdToCids { get; private set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> ActionIdToCids { get; private set; } = new Dictionary<int, List<string>>();

        public IpfsService(IConfiguration config, HttpClient httpClient)
        {
            _pinataApiKey = config["pinata:pinata_key"];
            _pinataSecretApiKey = config["pinata:pinata_secret"];
            _pinataJwt = config["pinata:pinata_jwt"];
            _httpClient = httpClient;

            // Tự động tải CID mới nhất từ Pinata khi khởi tạo
            InitializeMetadataAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeMetadataAsync()
        {
            try
            {
                Console.WriteLine("Initializing metadata...");
                // Lấy CID mới nhất từ Pinata
                var latestCids = await GetLatestCidsFromPinata();

                // Nếu không có CID hợp lệ, khởi tạo dữ liệu mặc định
                if (string.IsNullOrEmpty(latestCids.MetadataCid) || string.IsNullOrEmpty(latestCids.LatestIpfsCid))
                {
                    Console.WriteLine("No valid CIDs found on Pinata. Initializing default metadata and chain...");

                    // Khởi tạo metadata mặc định
                    var defaultMetadata = new
                    {
                        CropCodeToCids = new Dictionary<int, List<string>>(),
                        SensorIdToCids = new Dictionary<int, List<string>>(),
                        ActionIdToCids = new Dictionary<int, List<string>>()
                    };
                    string metadataJson = JsonSerializer.Serialize(defaultMetadata);
                    MetadataCid = await PublishToPinataAsync(metadataJson, "metadata.json");
                    Console.WriteLine($"Created new metadata with CID: {MetadataCid}");

                    // Khởi tạo chain mặc định
                    var defaultChain = new List<block>();
                    string chainJson = JsonSerializer.Serialize(defaultChain);
                    LatestIpfsCid = await PublishToPinataAsync(chainJson, "chain.json");
                    Console.WriteLine($"Created new chain with CID: {LatestIpfsCid}");
                }
                else
                {
                    MetadataCid = latestCids.MetadataCid;
                    LatestIpfsCid = latestCids.LatestIpfsCid;
                    Console.WriteLine($"Using existing CIDs from Pinata: MetadataCid={MetadataCid}, LatestIpfsCid={LatestIpfsCid}");
                    await RestoreDictionariesFromIpfs();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during metadata initialization: {ex.Message}");
                throw;
            }
        }

        public async Task<(string MetadataCid, string LatestIpfsCid)> GetLatestCidsFromPinata()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_pinataJwt}");

                Console.WriteLine("Fetching pin list from Pinata...");
                var response = await _httpClient.GetAsync("https://api.pinata.cloud/data/pinList?status=pinned");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to fetch pin list: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Failed to fetch pin list: {response.StatusCode}");
                }

                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw Pinata API response: {json}");

                try
                {
                    // Thử deserialize trực tiếp để xem cấu trúc JSON
                    var rawResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    Console.WriteLine($"Response structure: {JsonSerializer.Serialize(rawResponse, new JsonSerializerOptions { WriteIndented = true })}");

                    // Cập nhật model class để khớp với cấu trúc JSON thực tế
                    var pinList = JsonSerializer.Deserialize<PinataPinListResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    if (pinList?.Rows == null)
                    {
                        Console.WriteLine("Invalid response format from Pinata");
                        return (null, null);
                    }

                    Console.WriteLine($"Successfully parsed response with {pinList.Rows.Count} files");

                    // Lọc và sắp xếp theo thời gian pin mới nhất
                    var metadataFiles = pinList.Rows
                        .Where(r => r.Metadata?.Name == "metadata.json")
                        .OrderByDescending(r => r.DatePinned)
                        .ToList();
                    Console.WriteLine($"Found {metadataFiles.Count} metadata files");

                    var chainFiles = pinList.Rows
                        .Where(r => r.Metadata?.Name == "chain.json")
                        .OrderByDescending(r => r.DatePinned)
                        .ToList();
                    Console.WriteLine($"Found {chainFiles.Count} chain files");
                        
                    // In ra tất cả các file để debug
                    foreach (var file in metadataFiles)
                    {
                        Console.WriteLine($"Metadata file: {file.IpfsPinHash}, Date: {file.DatePinned}");
                    }
                    foreach (var file in chainFiles)
                    {
                        Console.WriteLine($"Chain file: {file.IpfsPinHash}, Date: {file.DatePinned}");
                    }

                    // Lấy CID mới nhất (sử dụng FirstOrDefault vì đã sắp xếp OrderByDescending)
                    var metadataCid = metadataFiles.FirstOrDefault()?.IpfsPinHash;
                    var chainCid = chainFiles.FirstOrDefault()?.IpfsPinHash;

                    Console.WriteLine($"Latest metadata.json CID: {metadataCid}");
                    Console.WriteLine($"Latest chain.json CID: {chainCid}");

                    // Kiểm tra tính hợp lệ của CID
                    if (!string.IsNullOrEmpty(metadataCid))
                    {
                        try
                        {
                            Console.WriteLine($"Validating metadata CID: {metadataCid}");
                            var metadataResponse = await _httpClient.GetAsync($"https://gateway.pinata.cloud/ipfs/{metadataCid}");
                            if (!metadataResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Warning: Metadata CID {metadataCid} is not accessible");
                                metadataCid = null;
                            }
                            else
                            {
                                string content = await metadataResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"Metadata content: {content}");
                                Console.WriteLine($"Metadata CID {metadataCid} is valid");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error validating metadata CID: {ex.Message}");
                            metadataCid = null;
                        }
                    }

                    if (!string.IsNullOrEmpty(chainCid))
                    {
                        try
                        {
                            Console.WriteLine($"Validating chain CID: {chainCid}");
                            var chainResponse = await _httpClient.GetAsync($"https://gateway.pinata.cloud/ipfs/{chainCid}");
                            if (!chainResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Warning: Chain CID {chainCid} is not accessible");
                                chainCid = null;
                            }
                            else
                            {
                                string content = await chainResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"Chain content: {content}");
                                Console.WriteLine($"Chain CID {chainCid} is valid");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error validating chain CID: {ex.Message}");
                            chainCid = null;
                        }
                    }

                    return (metadataCid, chainCid);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parsing error: {ex.Message}");
                    Console.WriteLine($"Error location: Line {ex.LineNumber}, Position {ex.BytePositionInLine}");
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest CIDs from Pinata: {ex.Message}");
                return (null, null);
            }
        }

        // Định nghĩa lớp để ánh xạ phản hồi từ /data/pinList
        private class PinataPinListResponse
        {
            public int Count { get; set; }
            public List<PinListRow> Rows { get; set; }
        }

        private class PinListRow
        {
            public string Id { get; set; }
            [JsonPropertyName("ipfs_pin_hash")]
            public string IpfsPinHash { get; set; }
            public int Size { get; set; }
            [JsonPropertyName("user_id")]
            public string UserId { get; set; }
            [JsonPropertyName("date_pinned")]
            public DateTime DatePinned { get; set; }
            [JsonPropertyName("date_unpinned")]
            public DateTime? DateUnpinned { get; set; }
            public PinMetadata Metadata { get; set; }
            public List<Region> Regions { get; set; }
            [JsonPropertyName("mime_type")]
            public string MimeType { get; set; }
            [JsonPropertyName("number_of_files")]
            public int NumberOfFiles { get; set; }
        }

        private class PinMetadata
        {
            public string Name { get; set; }
            public object Keyvalues { get; set; }
        }

        private class Region
        {
            [JsonPropertyName("regionId")]
            public string RegionId { get; set; }
            [JsonPropertyName("currentReplicationCount")]
            public int CurrentReplicationCount { get; set; }
            [JsonPropertyName("desiredReplicationCount")]
            public int DesiredReplicationCount { get; set; }
        }

        public async Task<string> PublishToPinataAsync(string jsonData, string fileName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_pinataJwt}");
                var content = new MultipartFormDataContent { { new StringContent(jsonData), "file", fileName } };
                var response = await _httpClient.PostAsync(_pinataApiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Pinata upload failed: {response.StatusCode} - {error}");
                }
                var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
                string cid = jsonResult["IpfsHash"].ToString();
                Console.WriteLine($"Published to Pinata: {fileName} with CID: {cid}");

                // Đợi một chút để Pinata xử lý file
                await Task.Delay(2000);



                // Cập nhật CID tương ứng từ kết quả mới nhất
                if (fileName == "metadata.json")
                    MetadataCid = cid;
                else if (fileName == "chain.json")
                    LatestIpfsCid = cid;

                return cid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing to Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task<string> PublishToIpfsAsync(List<block> chain)
        {
            try
            {
                string json = JsonSerializer.Serialize(chain);
                LatestIpfsCid = await PublishToPinataAsync(json, "chain.json");
                Console.WriteLine($"Published chain to Pinata with CID: {LatestIpfsCid}");
                return LatestIpfsCid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PublishToIpfsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateMetadataOnIpfs()
        {
            try
            {
                var metadata = new { CropCodeToCids, SensorIdToCids, ActionIdToCids };
                string metadataJson = JsonSerializer.Serialize(metadata);
                Console.WriteLine($"Updating metadata: {metadataJson}");
                MetadataCid = await PublishToPinataAsync(metadataJson, "metadata.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating metadata on IPFS: {ex.Message}");
                throw;
            }
        }

        public async Task<string> LoadBlockDataFromIpfs(string cid)
        {
            try
            {
                Console.WriteLine($"Loading data from Pinata: {cid}");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _pinataJwt);
                var response = await _httpClient.GetAsync($"https://gateway.pinata.cloud/ipfs/{cid}");
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to load CID {cid}: {response.StatusCode} - {errorContent}");
                }
                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || (!content.TrimStart().StartsWith("{") && !content.TrimStart().StartsWith("[")))
                {
                    throw new InvalidOperationException($"CID {cid} does not contain valid JSON data");
                }
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading from Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task RestoreChainFromIpfs(List<block> chain)
        {
            if (!string.IsNullOrEmpty(LatestIpfsCid))
            {
                try
                {
                    string json = await LoadBlockDataFromIpfs(LatestIpfsCid);
                    chain.Clear();
                    chain.AddRange(JsonSerializer.Deserialize<List<block>>(json));
                    Console.WriteLine($"Restored chain from Pinata with {chain.Count} blocks:");
                    foreach (var block in chain)
                    {
                        Console.WriteLine($"Block {block.Index}: {block.IpfsCid}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restore chain: {ex.Message}");
                }
            }
        }

        public async Task RestoreDictionariesFromIpfs()
        {
            if (!string.IsNullOrEmpty(MetadataCid))
            {
                try
                {
                    string json = await LoadBlockDataFromIpfs(MetadataCid);
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, List<string>>>>(json);
                    CropCodeToCids = metadata.GetValueOrDefault("CropCodeToCids") ?? new Dictionary<int, List<string>>();
                    SensorIdToCids = metadata.GetValueOrDefault("SensorIdToCids") ?? new Dictionary<int, List<string>>();
                    ActionIdToCids = metadata.GetValueOrDefault("ActionIdToCids") ?? new Dictionary<int, List<string>>();
                    Console.WriteLine($"Dictionaries restored: CropCodeToCids={CropCodeToCids.Count}, SensorIdToCids={SensorIdToCids.Count}, ActionIdToCids={ActionIdToCids.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restore dictionaries: {ex.Message}");
                }
            }
        }

        // Xóa các phương thức liên quan đến tệp cục bộ
        // public void LoadMetadataCid() { ... }
        // private async Task SaveMetadataCid() { ... }
        // public void ClearMetadataCidFile() { ... }
    }
}