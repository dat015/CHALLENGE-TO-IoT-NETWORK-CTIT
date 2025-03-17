using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PlantBlockchainApp.model
{
    public class block
    {
        public class Block
        {
            public int Index { get; set; }
            public string PreviousHash { get; set; }
            public string Hash { get; set; }
            public string Data { get; set; }
            public long Timestamp { get; set; }
            public int Nonce { get; set; }

            public Block(int index, string previousHash, string data)
            {
                Index = index;
                PreviousHash = previousHash;
                Data = data;
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Hash = CalculateHash();
            }

            public string CalculateHash()
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    string rawData = Index + PreviousHash + Timestamp + Data + Nonce;
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
            }

            public void MineBlock(int difficulty)
            {
                string target = new string('0', difficulty); // Ví dụ: "0000"
                while (Hash.Substring(0, difficulty) != target)
                {
                    Nonce++;
                    Hash = CalculateHash();
                }
                Console.WriteLine($"Block mined! Hash: {Hash}");
            }
        }
    }
}