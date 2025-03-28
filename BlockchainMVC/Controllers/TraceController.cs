using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlockchainMVC.Controllers
{
    [Authorize]
    public class TraceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string code)
        {
            // TODO: Implement blockchain search logic
            // For now, return mock data
            ViewBag.SearchResult = new
            {
                Code = code,
                Name = "Rau Xà Lách",
                PlantType = "Rau",
                PlantingDate = DateTime.Now.AddDays(-30),
                FarmerName = "Nguyễn Văn A",
                Location = "Huyện Củ Chi, TP.HCM",
                CultivationMethod = "Hữu cơ",
                IsOrganic = true
            };
            return View("Index");
        }
    }
} 