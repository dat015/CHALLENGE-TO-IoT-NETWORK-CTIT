using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class Batch
    {
        public int Id { get; set; }
        public string location { get; set; }    
        public string crop { get; set; }
        public int FarmId { get; set; }
        [ForeignKey("FarmId")]
        public Farm? Farm { get; set; }
        public List<TransactionSensor>? Transactions { get; set; }
        
    }
}