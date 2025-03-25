using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ipfs.Http;
using Microsoft.Extensions.Configuration;

namespace BlockChainForIoT.blockchain
{
    public class IpfsService
    {
        private readonly IpfsClient _ipfs;
        private readonly HttpClient _httpClient;
        private readonly string _pinataApiKey;
        private readonly string _pinataSecretApiKey;
        private readonly string _pinataJwt;
        private readonly string _pinataApiUrl = "https://api.pinata.cloud/pinning/pinFileToIPFS";
        private const string MetadataCidFile = "metadata_cid.json";

        public string MetadataCid { get; private set; }
        public string LatestIpfsCid { get; private set; }
        public Dictionary<int, List<string>> CropCodeToCids { get; private set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> SensorIdToCids { get; private set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> ActionIdToCids { get; private set; } = new Dictionary<int, List<string>>();

        public IpfsService(IConfiguration config, HttpClient httpClient)
        {
            //_pinataApiKey = Environment.GetEnvironmentVariable("PINATA_KEY");
            _pinataApiKey = config["pinata:pinata_key"];



            //_pinataSecretApiKey = Environment.GetEnvironmentVariable("PINATA_SECRET");
            _pinataSecretApiKey = config["pinata:pinata_secret"];

            // _pinataJwt = Environment.GetEnvironmentVariable("PINATA_JWT");
            _pinataJwt = config["pinata:pinata_jwt"];
            Console.WriteLine($"Pinata JWT: {_pinataJwt}");
            // _ipfs = new IpfsClient("http://127.0.0.1:5001");
            string ipfsAddress = Environment.GetEnvironmentVariable("IPFS_ADDRESS") ?? "http://ipfs:5001";
            _ipfs = new IpfsClient(ipfsAddress);
            _httpClient = httpClient;
        }

        public async Task<string> PublishToPinataAsync(string jsonData, string fileName)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_pinataJwt}");
            var content = new MultipartFormDataContent { { new StringContent(jsonData), "file", fileName } };
            var response = await _httpClient.PostAsync(_pinataApiUrl, content);
            response.EnsureSuccessStatusCode();
            var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
            return jsonResult["IpfsHash"].ToString();
        }

        public async Task<string> PublishToIpfsAsync(List<block> chain)
        {
            string json = JsonSerializer.Serialize(chain);
            LatestIpfsCid = await PublishToPinataAsync(json, "chain.json");
            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                await _ipfs.FileSystem.AddAsync(stream, "chain.json");
                await _ipfs.Pin.AddAsync(LatestIpfsCid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to pin to local IPFS: {ex.Message}");
            }
            return LatestIpfsCid;
        }

        public async Task UpdateMetadataOnIpfs()
        {
            var metadata = new { CropCodeToCids, SensorIdToCids, ActionIdToCids };
            string metadataJson = JsonSerializer.Serialize(metadata);
            Console.WriteLine(metadataJson);
            MetadataCid = await PublishToPinataAsync(metadataJson, "metadata.json");
            await SaveMetadataCid();
        }

        public void ClearMetadataCidFile()
        {
            string metadataCidFile = "metadata_cid.json"; // Đảm bảo đường dẫn đúng
            if (File.Exists(metadataCidFile))
            {
                File.Delete(metadataCidFile);
                Console.WriteLine("Metadata CID file deleted.");
            }
            MetadataCid = null;
            LatestIpfsCid = null;
        }
        public async Task<string> LoadBlockDataFromIpfs(string cid)
        {
            Console.WriteLine($"Loading data from IPFS: {cid}");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _pinataJwt);
            var response = await _httpClient.GetAsync($"https://gateway.pinata.cloud/ipfs/{cid}");
            Console.WriteLine($"Response from Pinata: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to load CID {cid}: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Failed to load CID {cid}: {response.StatusCode} - {errorContent}");
            }

            string content = await response.Content.ReadAsStringAsync();
            // Kiểm tra xem nội dung có phải JSON không (chấp nhận cả mảng JSON và đối tượng JSON)
            if (string.IsNullOrWhiteSpace(content) || (!content.TrimStart().StartsWith("{") && !content.TrimStart().StartsWith("[")))
            {
                Console.WriteLine($"CID {cid} does not contain valid JSON data: {content}");
                throw new InvalidOperationException($"CID {cid} does not contain valid JSON data: {content}");
            }

            return content;
        }
        public async Task RestoreChainFromIpfs(List<block> chain)
        {
            if (!string.IsNullOrEmpty(LatestIpfsCid))
            {
                string json = await LoadBlockDataFromIpfs(LatestIpfsCid);
                chain.Clear();
                chain.AddRange(JsonSerializer.Deserialize<List<block>>(json));
                foreach (var block in chain)
                {
                    Console.WriteLine($"Block {block.Index}: {block.IpfsCid}");
                }
            }
        }

        public async Task RestoreDictionariesFromIpfs()
        {
            if (!string.IsNullOrEmpty(MetadataCid))
            {
                string json = await LoadBlockDataFromIpfs(MetadataCid);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, List<string>>>>(json);
                CropCodeToCids = metadata.GetValueOrDefault("CropCodeToCids") ?? new Dictionary<int, List<string>>();
                SensorIdToCids = metadata.GetValueOrDefault("SensorIdToCids") ?? new Dictionary<int, List<string>>();
                ActionIdToCids = metadata.GetValueOrDefault("ActionIdToCids") ?? new Dictionary<int, List<string>>();
            }
        }

        public void LoadMetadataCid()
        {
            if (File.Exists(MetadataCidFile))
            {
                string json = File.ReadAllText(MetadataCidFile);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                MetadataCid = data.GetValueOrDefault("MetadataCid");
                LatestIpfsCid = data.GetValueOrDefault("LatestIpfsCid");
            }
        }

        private async Task SaveMetadataCid()
        {
            var data = new Dictionary<string, string> { { "MetadataCid", MetadataCid }, { "LatestIpfsCid", LatestIpfsCid } };
            File.WriteAllText(MetadataCidFile, JsonSerializer.Serialize(data));
        }
    }
}