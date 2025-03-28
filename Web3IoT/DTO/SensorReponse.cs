using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Web3IoT.DTO
{
    public class SensorReponse
    {
        public string Message { get; set; }
        [JsonPropertyName("sensorId")]
        public int SensorId { get; set; }
    }
}