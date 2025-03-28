using System;

namespace BlockChainMVC.DTOs
{
    public class PesticideDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ActiveIngredient { get; set; }
        public string Concentration { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Manufacturer { get; set; }
        public string BatchNumber { get; set; }
        public string Status { get; set; }
    }
} 