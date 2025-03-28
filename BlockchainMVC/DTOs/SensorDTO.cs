using System;

namespace BlockchainMVC.DTOs
{
    public class SensorDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public DateTime InstallationDate { get; set; }
        public string Status { get; set; }
        public string CropId { get; set; }
    }
} 