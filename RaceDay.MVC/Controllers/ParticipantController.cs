using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ParticipantController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");

            var client = GetAuthenticatedClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}Enrolments/my";

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var enrolments = JsonSerializer.Deserialize<List<EnrolmentViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(enrolments);
            }

            return View(new List<EnrolmentViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");

            var client = GetAuthenticatedClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}events/{eventId}/enrol";

            var payload = new { CategoryId = categoryId };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Failed to enrol: " + error;
            }

            return RedirectToAction("Details", "Home", new { id = eventId });
        }
    }

    // --- View Models needed for the JSON deserialization ---
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public CategoryViewModel? Category { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public EventViewModel? Event { get; set; }
    }
}