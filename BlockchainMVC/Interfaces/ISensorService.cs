using System.Collections.Generic;
using System.Threading.Tasks;
using BlockchainMVC.DTOs;

namespace BlockchainMVC.Interfaces
{
    public interface ISensorService
    {
        Task<bool> AddSensorAsync(SensorDTO sensor);
        Task<bool> UpdateSensorStatusAsync(int id, string status);
        Task<SensorDTO> GetSensorByIdAsync(int id);
        Task<IEnumerable<SensorDTO>> GetAllSensorsAsync();
        Task<bool> ValidateSensorAsync(int id);
        Task<IEnumerable<SensorDTO>> GetSensorsByCropIdAsync(int cropId);
    }
} 