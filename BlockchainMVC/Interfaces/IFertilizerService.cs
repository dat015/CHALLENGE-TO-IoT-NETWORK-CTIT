using System.Collections.Generic;
using System.Threading.Tasks;
using BlockchainMVC.DTOs;

namespace BlockchainMVC.Interfaces
{
    public interface IFertilizerService
    {
        Task<bool> AddFertilizerAsync(FertilizerDTO fertilizer);
        Task<bool> UpdateFertilizerStatusAsync(int id, string status);
        Task<FertilizerDTO> GetFertilizerByIdAsync(int id);
        Task<IEnumerable<FertilizerDTO>> GetAllFertilizersAsync();
        Task<bool> ValidateFertilizerAsync(int id);
    }
} 