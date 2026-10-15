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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // POST: api/enrolments/{enrolmentId}/results - Organiser records a result
        [HttpPost("enrolments/{enrolmentId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(int enrolmentId, ResultDto request)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
            if (enrolment == null) return NotFound("Enrolment not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || enrolment.Category!.Event!.OrganiserId != int.Parse(userId))
                return Forbid("You can only add results for your own events.");

            // Check if a result already exists
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == enrolmentId);
            if (existingResult != null)
                return Conflict("A result already exists for this enrolment.");

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                Status = request.Status
            };

            _context.Results.Add(result);
            enrolment.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Result recorded successfully!", resultId = result.ResultId });
        }

        // GET: api/results/my - Participant views their own results
        [HttpGet("results/my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.ParticipantId == int.Parse(userId))
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/events/{eventId}/results - Organiser views all results for their event
        [HttpGet("events/{eventId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound("Event not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only view results for your own events.");

            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Participant)
                .ThenInclude(p => p!.User)
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .ToListAsync();

            return Ok(results);
        }
    }
}