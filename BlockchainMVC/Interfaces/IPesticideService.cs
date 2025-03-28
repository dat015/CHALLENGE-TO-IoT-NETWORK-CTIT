using System.Collections.Generic;
using System.Threading.Tasks;
using BlockChainMVC.DTOs;

namespace BlockChainMVC.Interfaces
{
    public interface IPesticideService
    {
        Task<bool> AddPesticideAsync(PesticideDTO pesticide);
        Task<bool> UpdatePesticideStatusAsync(int id, string status);
        Task<PesticideDTO> GetPesticideByIdAsync(int id);
        Task<IEnumerable<PesticideDTO>> GetAllPesticidesAsync();
        Task<bool> ValidatePesticideAsync(int id);
    }
} 