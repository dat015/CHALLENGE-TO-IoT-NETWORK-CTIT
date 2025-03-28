using System.Threading.Tasks;

namespace BlockchainMVC.Interfaces
{
    public interface IBlockchainService
    {
        Task<bool> AddBlockAsync(string data);
        Task<bool> ValidateChainAsync();
        Task<string> GetLatestBlockHashAsync();
    }
} 