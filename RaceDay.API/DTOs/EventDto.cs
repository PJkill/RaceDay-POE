using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Upcoming";

        // New fields required by Part 2
        [Required]
        public string Distance { get; set; } = string.Empty;
        [Required]
        public string EventType { get; set; } = string.Empty; // run, walk, or cycle
    }
}