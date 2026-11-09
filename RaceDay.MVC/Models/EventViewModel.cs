using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = "Upcoming";

        [Required]
        public string Distance { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Display(Name = "Event Banner Image (Optional)")]
        public IFormFile? ImageFile { get; set; }
    }
}