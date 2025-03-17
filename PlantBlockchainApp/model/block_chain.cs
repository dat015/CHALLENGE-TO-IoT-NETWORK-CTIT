using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PlantBlockchainApp.model.block;

namespace PlantBlockchainApp.model
{
    public class block_chain
    {
        private List<Block> chain;
        private int difficulty = 2; // Độ khó PoW

        public block_chain()
        {
            chain = new List<Block>();
            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            var genesisBlock = new Block(0, "0", "Genesis Block");
            genesisBlock.MineBlock(difficulty);
            chain.Add(genesisBlock);
        }

        public Block GetLatestBlock()
        {
            return chain[chain.Count - 1];
        }

        public void AddBlock(string data)
        {
            var newBlock = new Block(GetLatestBlock().Index + 1, GetLatestBlock().Hash, data);
            newBlock.MineBlock(difficulty);
            chain.Add(newBlock);
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash() ||
                    currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }

        public List<Block> GetChain()
        {
            return chain;
        }
    }
}