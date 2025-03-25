using System.Security.Cryptography;
using System.Text;
using BlockChainForIoT.model;

namespace BlockChainForIoT.blockchain
{
    public class EncryptionService
    {
        public string EncryptData(ITransaction data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = new byte[16];
                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] encrypted = encryptor.TransformFinalBlock(
                    Encoding.UTF8.GetBytes(data.ToJsonString()),
                    0,
                    data.ToJsonString().Length
                );
                return Convert.ToBase64String(encrypted);
            }
        }
    }
}