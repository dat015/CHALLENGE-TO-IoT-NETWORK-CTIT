using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.dto
{
    public class crop_dto
    {
        public string Name { get; set; }
        public int sensorId { get; set; }
        public DateTime? DatePlanted { get; set; }
        public DateTime? DateHarvested { get; set; }
    }
}