namespace RaceDay.API.DTOs
{
    /// <summary>
    /// Standard pagination parameters for list endpoints.
    /// </summary>
    public class PaginationDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
