using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class Crop
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int sensorId { get; set; }
        public DateTime? DatePlanted { get; set; }
        public DateTime? DateHarvested { get; set; }
        public Sensor? Sensor { get; set; }
    }
}