using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    /// <summary>
    /// DTO used when a user logs in to receive a JWT token.
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
