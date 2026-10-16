using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Simple health check endpoint to verify the API is running.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// GET: api/health — Returns 200 OK if the API is alive.
        /// </summary>
        [HttpGet]
        public IActionResult Get() => Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
