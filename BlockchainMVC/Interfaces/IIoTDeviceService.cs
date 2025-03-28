using System.Threading.Tasks;

namespace BlockchainMVC.Interfaces
{
    public interface IIoTDeviceService
    {
        Task<bool> RegisterDeviceAsync(string deviceId, string deviceType);
        Task<bool> UpdateDeviceStatusAsync(string deviceId, string status);
        Task<string> GetDeviceDataAsync(string deviceId);
    }
} 