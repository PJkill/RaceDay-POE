using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events - Anyone can view all events
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();
            return Ok(events);
        }

        // GET: api/events/{id} - Anyone can view a single event
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null) return NotFound("Event not found.");
            return Ok(eventItem);
        }

        // POST: api/events - Only Organisers can create
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(EventDto request)
        {
            // Get the logged-in user's OrganiserId from the JWT token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.OrganiserId == int.Parse(userId));
            if (organiser == null) return Unauthorized("Only Organisers can create events.");

            var newEvent = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = request.Name,
                Description = request.Description,
                Date = request.Date,
                Location = request.Location,
                Status = request.Status,
                Distance = request.Distance,
                EventType = request.EventType
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully!", eventId = newEvent.EventId });
        }

        // PUT: api/events/{id} - Only the owner Organiser can update
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

        // DELETE: api/events/{id} - Only the owner Organiser can delete
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