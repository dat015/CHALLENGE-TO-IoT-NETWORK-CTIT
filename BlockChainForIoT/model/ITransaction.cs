using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainForIoT.model
{
    public interface ITransaction
    {
        DateTime Timestamp { get; set; }
        string ToJsonString();
    }
}