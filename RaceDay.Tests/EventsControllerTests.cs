using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        private RaceDayContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayContext(options);
        }

        private EventsController GetControllerWithUser(RaceDayContext context, int userId, string role)
        {
            var controller = new EventsController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsSuccess()
        {
            var context = GetInMemoryContext();
            var organiserUser = new User { Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" };
            context.Users.Add(organiserUser);
            await context.SaveChangesAsync();

            context.Organisers.Add(new Organiser { OrganiserId = organiserUser.UserId });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, organiserUser.UserId, "Organiser");
            var dto = new EventDto
            {
                Name = "Test Marathon",
                Description = "Test description",
                Date = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                Distance = "42.2km",
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, await context.Events.CountAsync());
            var savedEvent = await context.Events.FirstAsync();
            Assert.Equal("Test Marathon", savedEvent.Name);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { Name = "Event 1", OrganiserId = 1, Date = DateTime.Now, Location = "JHB", Distance = "10km", EventType = "run" });
            context.Events.Add(new Event { Name = "Event 2", OrganiserId = 1, Date = DateTime.Now, Location = "CPT", Distance = "21km", EventType = "walk" });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var result = await controller.GetAllEvents();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<Event>>(okResult.Value);
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetEvent_InvalidId_ReturnsNotFound()
        {
            var context = GetInMemoryContext();
            var controller = new EventsController(context);

            var result = await controller.GetEvent(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateEvent_NonOwnerOrganiser_ReturnsForbid()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventId = 1, Name = "Existing", OrganiserId = 999, Date = DateTime.Now, Location = "X", Distance = "5km", EventType = "run" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, 888, "Organiser");
            var dto = new EventDto
            {
                Name = "Updated",
                Date = DateTime.Now.AddDays(30),
                Location = "CPT",
                Distance = "10km",
                EventType = "run"
            };

            var result = await controller.UpdateEvent(1, dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_NonOwnerOrganiser_ReturnsForbid()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventId = 1, Name = "Existing", OrganiserId = 999, Date = DateTime.Now, Location = "X", Distance = "5km", EventType = "run" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, 888, "Organiser");

            var result = await controller.DeleteEvent(1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}