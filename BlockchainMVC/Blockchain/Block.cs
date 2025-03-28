using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainMVC.Blockchain
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }

        public Block(int index, string data, string previousHash)
        {
            Index = index;
            Timestamp = DateTime.UtcNow;
            Data = data;
            PreviousHash = previousHash;
            Hash = CalculateHash();
            Nonce = 0;
        }

        public string CalculateHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string rawData = $"{Index}{Timestamp}{Data}{PreviousHash}{Nonce}";
                byte[] bytes = Encoding.UTF8.GetBytes(rawData);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public void MineBlock(int difficulty)
        {
            string target = new string('0', difficulty);
            while (Hash.Substring(0, difficulty) != target)
            {
                Nonce++;
                Hash = CalculateHash();
            }
        }
    }
} 