using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    /// <summary>
    /// DTO used when a new user registers with the API.
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"
    }
}
