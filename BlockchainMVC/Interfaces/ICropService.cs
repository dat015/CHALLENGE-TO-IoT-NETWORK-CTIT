using System.Collections.Generic;
using System.Threading.Tasks;
using BlockchainMVC.DTOs;

namespace BlockchainMVC.Interfaces
{
    public interface ICropService
    {
        Task<bool> AddCropAsync(CropDTO crop);
        Task<bool> UpdateCropStatusAsync(int id, string status);
        Task<CropDTO> GetCropByIdAsync(int id);
        Task<IEnumerable<CropDTO>> GetAllCropsAsync();
        Task<bool> ValidateCropAsync(int id);
        Task<string> GetCropOriginAsync(int id);
    }
} 