using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("JWToken") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}Auth/login";

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                // Extract token and role from API response
                var token = jsonDoc.RootElement.GetProperty("token").GetString();
                var role = jsonDoc.RootElement.GetProperty("user").GetProperty("role").GetString();
                var userName = jsonDoc.RootElement.GetProperty("user").GetProperty("fullName").GetString();

                // Save to Session
                HttpContext.Session.SetString("JWToken", token!);
                HttpContext.Session.SetString("UserRole", role!);
                HttpContext.Session.SetString("UserName", userName!);

                // Redirect based on role
                if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
                else return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}Auth/register";

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed: {errorMsg}");
            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}