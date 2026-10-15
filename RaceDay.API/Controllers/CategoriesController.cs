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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/categories - Anyone can view
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound("Event not found.");

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/{eventId}/categories - Only Organiser (owner of the event)
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryDto request)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound("Event not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only add categories to your own events.");

            var category = new Category
            {
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                EntryFee = request.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category created successfully!", categoryId = category.CategoryId });
        }

        // PUT: api/categories/{id} - Only Organiser (owner of the event)
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto request)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound("Category not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || category.Event!.OrganiserId != int.Parse(userId))
                return Forbid("You can only edit categories of your own events.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.EntryFee = request.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated successfully!" });
        }

        // DELETE: api/categories/{id} - Only Organiser (owner of the event)
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound("Category not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || category.Event!.OrganiserId != int.Parse(userId))
                return Forbid("You can only delete categories of your own events.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully!" });
        }
    }
}