using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public class Farm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string location { get; set; }
        public List<Batch>? Batches { get; set; }

    }
}