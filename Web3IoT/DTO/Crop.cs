using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web3IoT.DTO
{
    public class Crop
    {
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int sensorId { get; set; }
        public DateTime? DatePlanted { get; set; }
        public DateTime? DateHarvested { get; set; }
    }
    
}