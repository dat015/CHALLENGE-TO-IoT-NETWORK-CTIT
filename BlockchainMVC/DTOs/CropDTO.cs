using System;

namespace BlockchainMVC.DTOs
{
    public class CropDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Variety { get; set; }
        public DateTime PlantingDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string FertilizerId { get; set; }
        public string PesticideId { get; set; }
    }
} 