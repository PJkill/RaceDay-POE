using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IBlobService _blobService;

        public EventsController(RaceDayContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events.Include(e => e.Categories).AsNoTracking().ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events.Include(e => e.Categories).AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null) return NotFound("Event not found.");
            return Ok(eventItem);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromForm] EventDto request, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.OrganiserId == int.Parse(userId));
            if (organiser == null) return Unauthorized("Only Organisers can create events.");

            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                imageUrl = await _blobService.UploadFileAsync(imageFile);
            }

            var newEvent = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = request.Name,
                Description = request.Description,
                Date = request.Date,
                Location = request.Location,
                Status = request.Status,
                Distance = request.Distance,
                EventType = request.EventType,
                ImageUrl = imageUrl
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully!", eventId = newEvent.EventId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto request)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound("Event not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only edit your own events.");

            eventItem.Name = request.Name;
            eventItem.Description = request.Description;
            eventItem.Date = request.Date;
            eventItem.Location = request.Location;
            eventItem.Status = request.Status;
            eventItem.Distance = request.Distance;
            eventItem.EventType = request.EventType;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound("Event not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only delete your own events.");

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully!" });
        }
    }
}