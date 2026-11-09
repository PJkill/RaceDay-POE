using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResultsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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

        public async Task<IActionResult> Manage(int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser") return RedirectToAction("Login", "Account");

            var client = GetAuthenticatedClient();

            // 1. Get Event Details
            var eventResponse = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}Events/{eventId}");
            if (!eventResponse.IsSuccessStatusCode) return NotFound();

            var eventContent = await eventResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(eventContent);
            var eventName = doc.RootElement.GetProperty("name").GetString();

            // 2. Get Enrolments for this event
            var enrolmentsResponse = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}events/{eventId}/enrolments");

            var viewModel = new ManageResultsViewModel
            {
                EventId = eventId,
                EventName = eventName ?? "Event",
                Enrolments = new List<ParticipantEnrolmentViewModel>()
            };

            if (enrolmentsResponse.IsSuccessStatusCode)
            {
                var enrolmentsContent = await enrolmentsResponse.Content.ReadAsStringAsync();

                using var enrolDoc = JsonDocument.Parse(enrolmentsContent);
                if (enrolDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in enrolDoc.RootElement.EnumerateArray())
                    {
                        var enrol = new ParticipantEnrolmentViewModel
                        {
                            EnrolmentId = item.GetProperty("enrolmentId").GetInt32(),
                            Status = item.GetProperty("status").GetString() ?? "Unknown"
                        };

                        if (item.TryGetProperty("participant", out var partObj) && partObj.ValueKind != JsonValueKind.Null)
                        {
                            if (partObj.TryGetProperty("user", out var userObj) && userObj.ValueKind != JsonValueKind.Null)
                            {
                                enrol.Participant = new ParticipantProfileViewModel
                                {
                                    User = new UserProfileViewModel
                                    {
                                        FullName = userObj.GetProperty("fullName").GetString() ?? "Unknown User",
                                        Email = userObj.GetProperty("email").GetString() ?? "No Email"
                                    }
                                };
                            }
                        }

                        if (item.TryGetProperty("category", out var catObj) && catObj.ValueKind != JsonValueKind.Null)
                        {
                            enrol.Category = new CategoryViewModel
                            {
                                Name = catObj.GetProperty("name").GetString() ?? "Unknown Category"
                            };
                        }

                        if (item.TryGetProperty("result", out var resObj) && resObj.ValueKind != JsonValueKind.Null)
                        {
                            enrol.Result = new ResultViewModel
                            {
                                FinishTime = resObj.GetProperty("finishTime").GetString(),
                                Position = resObj.GetProperty("position").GetInt32()
                            };
                        }

                        viewModel.Enrolments.Add(enrol);
                    }
                }
            }
            else if (enrolmentsResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "You can only view enrolments for events that YOU created.";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResult(int enrolmentId, int eventId, string finishTime, int? position)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser") return RedirectToAction("Login", "Account");

            var client = GetAuthenticatedClient();
            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}enrolments/{enrolmentId}/results";

            var payload = new
            {
                FinishTime = string.IsNullOrEmpty(finishTime) ? (TimeSpan?)null : TimeSpan.Parse(finishTime),
                Position = position,
                Status = "Completed"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Result recorded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to record result.";
            }

            return RedirectToAction("Manage", new { eventId = eventId });
        }
    }

    // --- View Models ---
    public class ManageResultsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<ParticipantEnrolmentViewModel> Enrolments { get; set; } = new List<ParticipantEnrolmentViewModel>();
    }

    public class ParticipantEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public ParticipantProfileViewModel? Participant { get; set; }
        public CategoryViewModel? Category { get; set; }
        public ResultViewModel? Result { get; set; }
    }

    public class ParticipantProfileViewModel
    {
        public UserProfileViewModel? User { get; set; }
    }

    public class UserProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ResultViewModel
    {
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }
}