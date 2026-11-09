using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Handles user authentication: registration and login.
    /// Passwords are hashed with BCrypt. Successful logins return a JWT token.
    /// </summary>
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

        /// <summary>
        /// Registers a new user (Organiser or Participant).
        /// Hashes the password with BCrypt and creates the role-specific profile.
        /// </summary>
        /// <returns>200 OK if registration succeeds; 400 if email exists or role is invalid.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("User with this email already exists.");

            // Check if role is valid
            if (request.Role != "Organiser" && request.Role != "Participant")
                return BadRequest("Role must be either 'Organiser' or 'Participant'.");

            // Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create the User record
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the role-specific profile
            if (user.Role == "Organiser")
            {
                _context.Organisers.Add(new Organiser { OrganiserId = user.UserId });
            }
            else
            {
                _context.Participants.Add(new Participant { ParticipantId = user.UserId });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token valid for 1 day.
        /// </summary>
        /// <returns>200 OK with token and user details; 401 if credentials are invalid.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Verify the password against the stored hash
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            // Generate a JWT token
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