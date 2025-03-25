using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlockChainForIoT.model;

namespace BlockChainForIoT.blockchain
{
    public class block
    {

        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string IpfsCid { get; set; } // CID của dữ liệu giao dịch trên IPFS
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string AuthorityPublicKey { get; set; } // Thêm khóa công khai của authority
        public string Signature { get; set; } // Thêm chữ ký số

        // Constructor cập nhật, bỏ Difficulty và Nonce vì không cần trong PoA
        public block(int index, string ipfsCid, string previousHash, string authorityPublicKey)
        {
            Index = index;
            TimeStamp = DateTime.Now;
            IpfsCid = ipfsCid;
            PreviousHash = previousHash;
            AuthorityPublicKey = authorityPublicKey;
            // Signature sẽ được thiết lập sau khi ký
            // Hash sẽ được tính sau khi có Signature
        }

        // Dữ liệu để ký (không bao gồm Signature và Hash)
        public string GetDataToSign()
        {
            return Index.ToString() + TimeStamp.ToString("o") + IpfsCid + PreviousHash;
        }

        // Tính hash của block, bao gồm tất cả các trường (kể cả Signature)
        public string CalculateHash()
        {
            using (var sha256 = SHA256.Create())
            {
                string authKey = AuthorityPublicKey ?? "";
                string sig = Signature ?? "";
                string rawData = $"{Index}{TimeStamp:yyyy-MM-ddTHH:mm:ss.fffffffzzz}{IpfsCid}{PreviousHash}{authKey}{sig}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes); 
            }
        }
    }
}