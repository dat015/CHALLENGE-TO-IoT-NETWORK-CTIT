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
            string peer_node_json = Environment.GetEnvironmentVariable("PEER_NODES");
            _authorityPublicKeys = authorityPublicKeys;
            _nodeAddress = nodeAddress;
            _peerNodes = JsonSerializer.Deserialize<List<string>>(peer_node_json);
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
                        if (peerChain.Count > chain.Count && new AuthorityManager(null, null).IsChainValid(peerChain))
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

        public async Task<bool> BroadcastBlockAsync(block newBlock)
        {
            int totalPeers = _peerNodes.Count;
            int okCount = 0;

            foreach (var peer in _peerNodes)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"{peer}/api/blockchain/add-block", newBlock);
                    if (response.IsSuccessStatusCode)
                    {
                        okCount++;
                        Console.WriteLine($"Peer {peer} accepted block.");
                    }
                    else
                    {
                        string reason = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Peer {peer} rejected block: {reason}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to broadcast to {peer}: {ex.Message}");
                }
            }

            Console.WriteLine($"Accepted by {okCount}/{totalPeers} peers.");
            return okCount == totalPeers && totalPeers > 0;
        }
    }
}