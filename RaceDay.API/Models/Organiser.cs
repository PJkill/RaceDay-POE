using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Organiser
    {
        [Key, ForeignKey("User")]
        public int OrganiserId { get; set; }
        public string? OrganisationName { get; set; }
        public User? User { get; set; }
    }
}