using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = "Upcoming";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // New fields required by Part 2
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // run, walk, cycle

        public Organiser? Organiser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}