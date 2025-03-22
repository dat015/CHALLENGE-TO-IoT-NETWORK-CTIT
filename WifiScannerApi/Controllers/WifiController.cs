using Microsoft.AspNetCore.Mvc;
using NativeWifi; // Thêm namespace này
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WifiScannerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WifiController : ControllerBase
    {
        [HttpGet("wifi-list")]
        public IActionResult GetWifiList()
        {
            try
            {
                var wifiList = new List<object>();
                WlanClient client = new WlanClient();

                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    // Thực hiện quét mạng Wi-Fi
                    wlanIface.Scan();
                    Thread.Sleep(2000); // Chờ 2 giây để quét hoàn tất

                    // Lấy danh sách mạng khả dụng
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);

                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {
                        string ssid = GetStringForSSID(network.dot11Ssid);
                        if (!string.IsNullOrEmpty(ssid)) // Loại bỏ SSID rỗng
                        {
                            wifiList.Add(new
                            {
                                SSID = ssid,
                                SignalQuality = network.wlanSignalQuality,
                                IsConnected = network.flags.HasFlag(Wlan.WlanAvailableNetworkFlags.Connected)
                            });
                        }
                    }
                }

                if (wifiList.Count == 0)
                {
                    return Ok(new { Message = "Không tìm thấy mạng Wi-Fi nào." });
                }

                return Ok(wifiList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Message = "Vui lòng chạy API với quyền quản trị hoặc kiểm tra Wi-Fi." });
            }
        }

        // Chuyển đổi SSID từ mảng byte sang chuỗi
        private static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
    }
}