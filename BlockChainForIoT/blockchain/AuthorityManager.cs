using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json; // Thêm để xử lý JSON
using System.Threading.Tasks;

namespace BlockChainForIoT.blockchain
{
    public class AuthorityManager
    {
        private readonly RSA _privateKey;
        private readonly List<string> _authorityPublicKeys;
        private readonly HttpClient _httpClient;
        List<string> publicKeys;
        public List<string> AuthorityPublicKeys => _authorityPublicKeys;

        public AuthorityManager(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            // Đọc private key từ biến môi trường
            string privateKeyBase64 = Environment.GetEnvironmentVariable("AUTHORITY_PRIVATE_KEY");
            // string privateKeyBase64 = config["authority:AUTHORITY_PRIVATE_KEY"];
            if (string.IsNullOrEmpty(privateKeyBase64))
            {
                throw new Exception("AUTHORITY_PRIVATE_KEY environment variable is not set.");
            }

            // Khởi tạo private key
            _privateKey = RSA.Create();
            _privateKey.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);

            // Đọc danh sách public keys từ biến môi trường
            string publicKeysJson = Environment.GetEnvironmentVariable("AUTHORITY_PUBLIC_KEYS");

            // var publicKeys = config.GetSection("authority:AUTHORITY_PUBLIC_KEYS").Get<List<string>>();
            publicKeys = JsonSerializer.Deserialize<List<string>>(publicKeysJson);
            if (publicKeys == null || publicKeys.Count == 0)
            {
                throw new InvalidOperationException("AUTHORITY_PUBLIC_KEYS is empty or invalid.");
            }

            // Gán danh sách public keys
            _authorityPublicKeys = publicKeys;


            // Thêm public key của node hiện tại (từ private key) vào danh sách nếu chưa có
            string myPublicKey = Convert.ToBase64String(_privateKey.ExportRSAPublicKey());
            if (!_authorityPublicKeys.Contains(myPublicKey))
            {
                _authorityPublicKeys.Add(myPublicKey);
                Console.WriteLine($"Added node's public key to authority list: {myPublicKey}");
            }
        }

        public async Task AddAuthorityNodeAsync(string publicKey, List<string> peerNodes)
        {
            if (!_authorityPublicKeys.Contains(publicKey))
            {
                _authorityPublicKeys.Add(publicKey);
                Console.WriteLine($"Added new authority public key: {publicKey}");
                foreach (var peer in peerNodes)
                {
                    try
                    {
                        await _httpClient.PostAsJsonAsync($"{peer}/api/blockchain/add-authority", publicKey);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to notify peer {peer}: {ex.Message}");
                    }
                }
            }
        }

        public void AddGenesisBlock(List<block> chain)
        {
            if (chain.Count == 0)
            {
                // Sử dụng khóa công khai cố định từ danh sách _authorityPublicKeys
                string genesisAuthorityKey = _authorityPublicKeys.FirstOrDefault()
                    ?? "MIIBCgKCAQEAoZXY6gezqcEC6dcQ0yZYiMMIMlX8tZvjgOJEHapmAHzr9ySXESAqyFJpre/q67kXR+Zlt4Y7h2QWMDOWbd//jpIPmLxS8U2KDbCJtYeNScrH/XogDIdp5SAJ9BCVIrCSvdAF7ZOkqHEmtARdmp9FHwWbswVHS7PDQ/iXsGDG8NqJphnSvtHjeZTW7aG9rEQqrcu9Oxzut3pAFBd5zsAbC8Ed3LYYr/SNAtA7+Mo9/ulQQ5ZQe91WoysuEaQiJ5nV82ntDIFMMrFCyaaygPRsLoud1q5A/7/W8kWF0+cuTCV4bjvPPGX4N9uYnqzm4j5tkr3k5fzDl4VPq1/EX+c6bQIDAQAB";

                // Tạo block genesis với các giá trị cố định
                block genesisBlock = new block(0, "QmGenesisBlock", "0", genesisAuthorityKey, "0")
                {
                    TimeStamp = DateTime.Parse("2025-01-01T00:00:00Z") // Thời gian cố định (UTC)
                };

                // Chữ ký cố định cho genesis (không phụ thuộc _privateKey)
                genesisBlock.Signature = "GENESIS_SIGNATURE";
                genesisBlock.Hash = genesisBlock.CalculateHash();

                chain.Add(genesisBlock);
                Console.WriteLine($"Genesis block added with Hash={genesisBlock.Hash}");
            }
        }

        public bool IsChainValid(List<block> chain)
        {
            if (chain.Count == 1) return true;

            for (int i = 1; i < chain.Count; i++)
            {
                Console.WriteLine(chain[i].GetDataToSign);
                block currentBlock = chain[i];
                block previousBlock = chain[i - 1];

                string calculatedHash = currentBlock.CalculateHash();
                if (currentBlock.Hash != calculatedHash)
                {
                    Console.WriteLine($"Hash mismatch at index {i}: Received={currentBlock.Hash}, Calculated={calculatedHash}");
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Console.WriteLine($"PreviousHash mismatch at index {i}: Expected={previousBlock.Hash}, Received={currentBlock.PreviousHash}");
                    return false;
                }

                if (!_authorityPublicKeys.Contains(currentBlock.AuthorityPublicKey))
                {
                    Console.WriteLine($"AuthorityPublicKey not recognized at index {i}: {currentBlock.AuthorityPublicKey}");
                    return false;
                }

                RSA publicKey = RSA.Create();
                publicKey.ImportRSAPublicKey(Convert.FromBase64String(currentBlock.AuthorityPublicKey), out _);
                byte[] signature = Convert.FromBase64String(currentBlock.Signature);
                string dataToSign = currentBlock.GetDataToSign();
                if (!publicKey.VerifyData(Encoding.UTF8.GetBytes(dataToSign), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    Console.WriteLine($"Signature verification failed at index {i}: DataToSign={dataToSign}, Signature={currentBlock.Signature}");
                    return false;
                }
            }
            return true;
        }

        public RSA PrivateKey => _privateKey;
    }
}