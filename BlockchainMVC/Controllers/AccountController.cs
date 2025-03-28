using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BlockchainMVC.Data;
using Microsoft.EntityFrameworkCore;
using BlockchainMVC.Models;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.AccountType.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(IFormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var email = form["Email"].ToString();
            var password = form["Password"].ToString();
            var confirmPassword = form["ConfirmPassword"].ToString();
            var fullName = form["FullName"].ToString();
            var phoneNumber = form["PhoneNumber"].ToString();
            var address = form["Address"].ToString();
            var accountType = (AccountType)int.Parse(form["AccountType"].ToString());

            // Kiểm tra mật khẩu xác nhận
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp");
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Email đã được sử dụng");
                return View();
            }

            // Tạo user mới
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Password = password, // Trong thực tế nên mã hóa mật khẩu
                PhoneNumber = phoneNumber,
                Address = address,
                AccountType = accountType,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.");
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
} 