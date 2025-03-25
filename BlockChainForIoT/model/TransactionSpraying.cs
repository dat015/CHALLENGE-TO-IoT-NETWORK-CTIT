using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class TransactionSpraying : ITransaction
    {
        public string Type => "Spraying";
        public int CropCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string PesticideName { get; set; } // Tên thuốc trừ sâu
        public double Quantity { get; set; } // Số lượng (lít/kg)
        public string Description { get; set; }

        public TransactionSpraying(int cropCode, string pesticideName, double quantity, string description = "")
        {
            CropCode = cropCode;
            Timestamp = DateTime.UtcNow;
            PesticideName = pesticideName;
            Quantity = quantity;
            Description = description;
        }

        public string ToJsonString() => JsonSerializer.Serialize(this);
    }
}