using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Diagnostics;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}Events";

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<EventViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(events);
            }

            return View(new List<EventViewModel>());
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}Events/{id}";

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Using JsonDocument to parse the nested Categories array easily
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var viewModel = new EventDetailsViewModel
                {
                    EventId = root.GetProperty("eventId").GetInt32(),
                    Name = root.GetProperty("name").GetString(),
                    Description = root.GetProperty("description").GetString(),
                    Date = root.GetProperty("date").GetDateTime(),
                    Location = root.GetProperty("location").GetString(),
                    Distance = root.GetProperty("distance").GetString(),
                    EventType = root.GetProperty("eventType").GetString()
                };

                var categories = new List<CategoryViewModel>();
                if (root.TryGetProperty("categories", out JsonElement catArray) && catArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cat in catArray.EnumerateArray())
                    {
                        categories.Add(new CategoryViewModel
                        {
                            CategoryId = cat.GetProperty("categoryId").GetInt32(),
                            Name = cat.GetProperty("name").GetString(),
                            EntryFee = cat.GetProperty("entryFee").GetDecimal()
                        });
                    }
                }

                viewModel.Categories = categories;
                return View(viewModel);
            }

            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class EventDetailsViewModel : EventViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}