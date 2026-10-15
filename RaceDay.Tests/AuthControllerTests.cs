using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayContext(options);
        }

        private JwtService GetJwtService()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretKeyForRaceDayApplication12345!" },
                { "Jwt:Issuer", "RaceDayAPI" },
                { "Jwt:Audience", "RaceDayClient" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            return new JwtService(config);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            var context = GetInMemoryContext();
            var controller = new RaceDay.API.Controllers.AuthController(context, GetJwtService());
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User",
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.NotNull(result);
            Assert.Equal(1, await context.Users.CountAsync());
            var savedUser = await context.Users.FirstAsync();
            Assert.Equal("test@example.com", savedUser.Email);
            Assert.Equal("Participant", savedUser.Role);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            context.Users.Add(new User { Email = "test@example.com", PasswordHash = "x", FullName = "Existing", Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = new RaceDay.API.Controllers.AuthController(context, GetJwtService());
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "New User",
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            var controller = new RaceDay.API.Controllers.AuthController(context, GetJwtService());
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User",
                Role = "Admin"
            };

            var result = await controller.Register(dto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var context = GetInMemoryContext();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            context.Users.Add(new User { Email = "test@example.com", PasswordHash = passwordHash, FullName = "Test", Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = new RaceDay.API.Controllers.AuthController(context, GetJwtService());
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var result = await controller.Login(dto);

            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var context = GetInMemoryContext();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            context.Users.Add(new User { Email = "test@example.com", PasswordHash = passwordHash, FullName = "Test", Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = new RaceDay.API.Controllers.AuthController(context, GetJwtService());
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };

            var result = await controller.Login(dto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult>(result);
        }
    }
}