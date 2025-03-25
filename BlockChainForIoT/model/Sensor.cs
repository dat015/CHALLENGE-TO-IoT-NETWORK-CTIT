using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class Sensor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string sensorCode { get; set; }
        public List<Crop>? Crops { get; set; }
    }
}