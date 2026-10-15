using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/enrolments/my - Participant views their own enrolments
        [HttpGet("enrolments/my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var enrolments = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c.Event)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == int.Parse(userId))
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/events/{eventId}/enrol - Participant enrols in a category
        [HttpPost("events/{eventId}/enrol")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> EnrolInEvent(int eventId, EnrolmentDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound("Event not found.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.EventId == eventId);
            if (category == null) return NotFound("Category not found for this event.");

            var participantId = int.Parse(userId);

            var existingEnrolment = await _context.Enrolments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.ParticipantId == participantId && e.Category!.EventId == eventId);
            if (existingEnrolment != null)
                return BadRequest("You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolled successfully!", enrolmentId = enrolment.EnrolmentId });
        }

        // DELETE: api/enrolments/{id} - Participant cancels their own enrolment
        [HttpDelete("enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound("Enrolment not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || enrolment.ParticipantId != int.Parse(userId))
                return Forbid("You can only cancel your own enrolments.");

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled successfully!" });
        }

        // GET: api/events/{eventId}/enrolments - Organiser views all enrolments for their event
        [HttpGet("events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound("Event not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only view enrolments for your own events.");

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .ThenInclude(p => p.User)
                .Include(e => e.Category)
                .Where(e => e.Category!.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}