using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class TransactionSensor : ITransaction
    {
       
        public DateTime Timestamp { get; set; }
        public double Humidity { get; set; }
        public double Temperature { get; set; }
        public double SoilMoisture { get; set; }
        public int SensorId { get; set; }
        [ForeignKey ("SensorId")]
        public Sensor? Sensor { get; set; }
        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}