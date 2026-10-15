using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public decimal EntryFee { get; set; }
    }
}