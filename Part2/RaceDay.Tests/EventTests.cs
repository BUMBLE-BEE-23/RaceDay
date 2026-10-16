
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EventTests
{
    [Fact]
    public async Task GetEvents_ReturnsOk()
    {
        await using var db = CreateDatabase();
        var controller = new EventsController(db);

        var result = await controller.GetEvents();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetEvent_MissingId_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new EventsController(db);

        var result = await controller.GetEvent(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateEvent_InvalidEventType_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var controller = CreateOrganiserController(db, 1);

        var request = new EventRequest
        {
            Name = "Test Race",
            Description = "Test description",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Flying"
        };

        var result = await controller.CreateEvent(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.Events.ToListAsync());
    }

    [Fact]
    public async Task UpdateEvent_OtherOrganisersEvent_ReturnsForbid()
    {
        await using var db = CreateDatabase();

        db.Users.Add(new User
        {
            Id = 2,
            FirstName = "Other",
            LastName = "Organiser",
            Email = "other@example.com",
            PasswordHash = "test-hash",
            Role = UserRoles.Organiser
        });

        db.Events.Add(new Event
        {
            Name = "Other Organiser Race",
            Description = "Test race",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Pretoria",
            DistanceKm = 10,
            EventType = "Run",
            OrganiserId = 2
        });

        await db.SaveChangesAsync();

        var eventId = await db.Events
            .Where(e => e.OrganiserId == 2)
            .Select(e => e.Id)
            .SingleAsync();

        var controller = CreateOrganiserController(db, 1);

        var request = new EventRequest
        {
            Name = "Changed Race",
            Description = "Changed description",
            EventDate = DateTime.UtcNow.AddDays(14),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Run"
        };

        var result = await controller.UpdateEvent(eventId, request);

        Assert.IsType<ForbidResult>(result.Result);

        var savedEvent = await db.Events.FindAsync(eventId);
        Assert.Equal("Other Organiser Race", savedEvent!.Name);
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        db.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Organiser",
            Email = "organiser@example.com",
            PasswordHash = "test-hash",
            Role = UserRoles.Organiser
        });

        db.SaveChanges();

        return db;
    }

    private static EventsController CreateOrganiserController(
        ApplicationDbContext db, int organiserId)
    {
        var controller = new EventsController(db);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    organiserId.ToString()),
                new Claim(ClaimTypes.Role, UserRoles.Organiser)
            },
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }
}
