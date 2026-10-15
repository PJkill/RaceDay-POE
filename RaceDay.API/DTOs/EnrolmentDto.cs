using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EnrolmentDto
    {
        [Required]
        public int CategoryId { get; set; }
    }
}