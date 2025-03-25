// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;

// namespace BlockChainForIoT.controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class NodeController : ControllerBase
//     {
//         private readonly Blockchain _blockchain;
//         public NodeController(Blockchain blockchain)
//         {
//             _blockchain = blockchain;
//         }
//         [HttpPost("add-authority")]
//         public async Task<IActionResult> AddAuthority([FromBody] string publicKey)
//         {
//             await _blockchain.AddAuthorityNodeAsync(publicKey);
//             return Ok("Authority added");
//         }
//     }
// }