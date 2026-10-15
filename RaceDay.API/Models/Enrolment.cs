using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Confirmed";

        public Participant? Participant { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}