using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class TransactionAction : ITransaction
    {
        
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Receiver { get; set; } //app web (IP)
        public int Sender { get; set; } // sensor
        [ForeignKey("Sender")]
        public Sensor? Sensor { get; set; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}