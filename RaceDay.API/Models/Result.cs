using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "Pending";

        public Enrolment? Enrolment { get; set; }
    }
}