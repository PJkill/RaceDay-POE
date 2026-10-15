using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests
    {
        private RaceDayContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayContext(options);
        }

        private EnrolmentsController GetControllerWithUser(RaceDayContext context, int userId, string role)
        {
            var controller = new EnrolmentsController(context);
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
        public async Task EnrolInEvent_ValidParticipant_ReturnsSuccess()
        {
            var context = GetInMemoryContext();
            var organiser = new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" };
            var participant = new User { UserId = 2, Email = "part@test.com", PasswordHash = "x", FullName = "Part", Role = "Participant" };
            context.Users.AddRange(organiser, participant);
            await context.SaveChangesAsync();

            context.Participants.Add(new Participant { ParticipantId = participant.UserId });
            context.Events.Add(new Event { EventId = 1, OrganiserId = 1, Name = "Test Event", Date = DateTime.Now.AddDays(10), Location = "CPT", Distance = "10km", EventType = "run" });
            await context.SaveChangesAsync();

            context.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "Open", EntryFee = 100 });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, participant.UserId, "Participant");
            var dto = new EnrolmentDto { CategoryId = 1 };

            var result = await controller.EnrolInEvent(1, dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, await context.Enrolments.CountAsync());
        }

        [Fact]
        public async Task EnrolInEvent_InvalidCategory_ReturnsNotFound()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventId = 1, OrganiserId = 1, Name = "Event", Date = DateTime.Now, Location = "X", Distance = "5km", EventType = "run" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, 2, "Participant");
            var dto = new EnrolmentDto { CategoryId = 999 };

            var result = await controller.EnrolInEvent(1, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EnrolInEvent_DuplicateEnrolment_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { EventId = 1, OrganiserId = 1, Name = "Event", Date = DateTime.Now, Location = "X", Distance = "5km", EventType = "run" });
            context.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "Cat", EntryFee = 100 });
            context.Enrolments.Add(new Enrolment { EnrolmentId = 1, ParticipantId = 2, CategoryId = 1, Status = "Confirmed" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, 2, "Participant");
            var dto = new EnrolmentDto { CategoryId = 1 };

            var result = await controller.EnrolInEvent(1, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelEnrolment_NonOwner_ReturnsForbid()
        {
            var context = GetInMemoryContext();
            context.Enrolments.Add(new Enrolment { EnrolmentId = 1, ParticipantId = 999, CategoryId = 1, Status = "Confirmed" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithUser(context, 2, "Participant");

            var result = await controller.CancelEnrolment(1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}