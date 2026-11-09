using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    /// <summary>
    /// DTO used by Organisers to record a race result for an enrolment.
    /// </summary>
    
    public class ResultDto
    {
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "Completed";
    }
}
