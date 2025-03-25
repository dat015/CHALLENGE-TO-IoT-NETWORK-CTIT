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
                string genesisAuthorityKey = _authorityPublicKeys.FirstOrDefault() ?? Convert.ToBase64String(_privateKey?.ExportRSAPublicKey() ?? RSA.Create().ExportRSAPublicKey());
                block genesisBlock = new block(0, "QmGenesisBlock", "0", genesisAuthorityKey);
                if (_privateKey != null)
                {
                    genesisBlock.Signature = Convert.ToBase64String(_privateKey.SignData(Encoding.UTF8.GetBytes(genesisBlock.GetDataToSign()), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
                }
                else
                {
                    genesisBlock.Signature = "GENESIS_SIGNATURE";
                }
                genesisBlock.Hash = genesisBlock.CalculateHash();
                chain.Add(genesisBlock);
                Console.WriteLine("Genesis block added.");
            }
        }

        public bool IsChainValid(List<block> chain)
        {
            Console.WriteLine(chain == null);
            Console.WriteLine(chain.Count);
            for (int i = 1; i < chain.Count; i++)
            {
                Console.WriteLine(chain[i].Hash);
                Console.WriteLine(chain[i].CalculateHash());
                block currentBlock = chain[i];
                block previousBlock = chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash() || currentBlock.PreviousHash != previousBlock.Hash)
                    return false;

                if (!_authorityPublicKeys.Contains(currentBlock.AuthorityPublicKey))
                    return false;

                RSA publicKey = RSA.Create();
                publicKey.ImportRSAPublicKey(Convert.FromBase64String(currentBlock.AuthorityPublicKey), out _);
                byte[] signature = Convert.FromBase64String(currentBlock.Signature);
                if (!publicKey.VerifyData(Encoding.UTF8.GetBytes(currentBlock.GetDataToSign()), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    return false;
            }
            return true;
        }

        public RSA PrivateKey => _privateKey;
    }
}