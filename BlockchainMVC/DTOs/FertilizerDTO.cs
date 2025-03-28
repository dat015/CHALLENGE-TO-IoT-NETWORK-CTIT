using System;

namespace BlockchainMVC.DTOs
{
    public class FertilizerDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Composition { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Manufacturer { get; set; }
        public string BatchNumber { get; set; }
        public string Status { get; set; }
    }
} 