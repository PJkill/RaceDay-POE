using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly JwtService _jwtService;

        public AuthController(RaceDayContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("User with this email already exists.");

            // Check if role is valid
            if (request.Role != "Organiser" && request.Role != "Participant")
                return BadRequest("Role must be either 'Organiser' or 'Participant'.");

            // Hash the password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create the User
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the specific role profile
            if (user.Role == "Organiser")
            {
                var organiser = new Organiser { OrganiserId = user.UserId };
                _context.Organisers.Add(organiser);
            }
            else if (user.Role == "Participant")
            {
                var participant = new Participant { ParticipantId = user.UserId };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            // Generate JWT Token
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    user.UserId,
                    user.Email,
                    user.FullName,
                    user.Role
                }
            });
        }
    }
}