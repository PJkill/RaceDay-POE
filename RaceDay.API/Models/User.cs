using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}