using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BlockchainMVC.Data;
using BlockchainMVC.Models;

namespace BlockchainMVC.Controllers
{
    [Authorize]
    public class PlantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var plants = await _context.Plants
                .Include(p => p.Crop)
                .Include(p => p.Origin)
                .OrderByDescending(p => p.PlantingDate)
                .ToListAsync();

            return View(plants);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(IFormCollection form)
        {
            // TODO: Implement blockchain registration logic
            // For now, just redirect to home page
            return RedirectToAction("Index", "Home");
        }
    }
} 