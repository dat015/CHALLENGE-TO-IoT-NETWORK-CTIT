using System;

namespace Web3IoT.Models
{
    public class CropTrace
    {
        public string Type { get; set; }
        public int CropCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string CropName { get; set; }
        public string Location { get; set; }
        public string FarmerName { get; set; }
        public string Description { get; set; }
        public int? SensorId { get; set; }
        public string PesticideName { get; set; }
        public string FertilizerName { get; set; }
        public int? Quantity { get; set; }
    }
} 