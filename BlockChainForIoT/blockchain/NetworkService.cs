using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChainForIoT.blockchain
{
    public class NetworkService
    {
        private readonly List<string> _authorityPublicKeys;
        private readonly string _nodeAddress;
        private readonly List<string> _peerNodes;
        private readonly HttpClient _httpClient;

        public List<string> PeerNodes => _peerNodes;

        public NetworkService(List<string> authorityPublicKeys, string nodeAddress, List<string> peerNodes, HttpClient httpClient)
        {
            _authorityPublicKeys = authorityPublicKeys;
            _nodeAddress = nodeAddress;
            _peerNodes = peerNodes ?? new List<string>();
            _httpClient = httpClient;
        }

        public async Task SyncWithPeersAsync(List<block> chain)
        {
            foreach (var peer in _peerNodes)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{peer}/api/blockchain/chain");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var peerChain = JsonSerializer.Deserialize<List<block>>(json);
                        if (peerChain.Count > chain.Count && new AuthorityManager(null,null).IsChainValid(peerChain))
                        {
                            chain.Clear();
                            chain.AddRange(peerChain);
                            Console.WriteLine($"Synced chain from {peer}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync with {peer}: {ex.Message}");
                }
            }
        }

        public async Task BroadcastBlockAsync(block newBlock)
        {
            foreach (var peer in _peerNodes)
            {
                try
                {
                    await _httpClient.PostAsJsonAsync($"{peer}/api/blockchain/add-block", newBlock);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to broadcast block to {peer}: {ex.Message}");
                }
            }
        }
    }
}   