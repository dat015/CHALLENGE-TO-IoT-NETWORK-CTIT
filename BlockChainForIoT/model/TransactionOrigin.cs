using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class TransactionOrigin : ITransaction
    {
       
       public string Type => "Origin";
        public int? CropCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string CropName { get; set; }
        public string Location { get; set; }
        public string FarmerName { get; set; } // Tên nông dân
        public string Description { get; set; }
        public int sensorId { get; set; }

        public TransactionOrigin(int? cropCode, string cropName, string location, string farmerName, string description = "")
        {
            CropCode = cropCode;
            Timestamp = DateTime.UtcNow;
            CropName = cropName;
            Location = location;
            FarmerName = farmerName;
            Description = description;
        }

        public string ToJsonString() => JsonSerializer.Serialize(this);

    }
    public class Fertilizer
    {
        public int Id { get; set; } // Mã định danh
        public string Type { get; set; } // Loại phân
        public double Amount { get; set; } // Lượng sử dụng
        public DateTime AppliedDate { get; set; } // Ngày bón
    }

    public class Pesticide
    {
        public int Id { get; set; } // Mã định danh
        public string Name { get; set; } // Tên thuốc
        public double Dosage { get; set; } // Liều lượng
        public DateTime AppliedDate { get; set; } // Ngày phun
    }
}