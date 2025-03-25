using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class TransactionFertilizing : ITransaction
    {
        public string Type => "Fertilizing";
        public int CropCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string FertilizerName { get; set; } // Tên loại phân
        public double Quantity { get; set; } // Số lượng (kg)
        public string Description { get; set; }

        public TransactionFertilizing(int cropCode, string fertilizerName, double quantity, string description = "")
        {
            CropCode = cropCode;
            Timestamp = DateTime.UtcNow;
            FertilizerName = fertilizerName;
            Quantity = quantity;
            Description = description;
        }

        public string ToJsonString() => JsonSerializer.Serialize(this);
    }
}