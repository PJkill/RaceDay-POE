using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "Completed";
    }
}