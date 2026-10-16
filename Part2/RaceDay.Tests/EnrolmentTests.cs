using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentTests
{
    [Fact]
    public async Task ParticipantCanEnrol_AndEnrolmentIsRecorded()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateParticipantController(db, seed.ParticipantId);

        var result = await controller.Enrol(new EnrolmentRequest
        {
            EventId = seed.EventId,
            CategoryId = seed.CategoryId
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<EnrolmentResponse>(created.Value);

        Assert.Equal(seed.EventId, response.EventId);
        Assert.Equal(seed.CategoryId, response.CategoryId);
        Assert.Equal(seed.ParticipantId, response.ParticipantId);

        var saved = await db.EventEnrolments.SingleAsync();

        Assert.Equal(seed.ParticipantId, saved.ParticipantId);
        Assert.Equal(seed.EventId, saved.EventId);
        Assert.Equal(seed.CategoryId, saved.CategoryId);
    }

    [Fact]
    public async Task ParticipantCannotEnrol_InCategoryFromAnotherEvent()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateParticipantController(db, seed.ParticipantId);

        var result = await controller.Enrol(new EnrolmentRequest
        {
            EventId = seed.EventId,
            CategoryId = seed.OtherCategoryId
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.EventEnrolments.ToListAsync());
    }

    [Fact]
    public async Task ParticipantCannotEnrolInSameEventTwice()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        db.EventEnrolments.Add(new EventEnrolment
        {
            ParticipantId = seed.ParticipantId,
            EventId = seed.EventId,
            CategoryId = seed.CategoryId
        });

        await db.SaveChangesAsync();

        var controller = CreateParticipantController(db, seed.ParticipantId);

        var result = await controller.Enrol(new EnrolmentRequest
        {
            EventId = seed.EventId,
            CategoryId = seed.CategoryId
        });

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Single(await db.EventEnrolments.ToListAsync());
    }

    [Fact]
    public async Task ParticipantCannotEnrolInNonexistentEvent()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateParticipantController(db, seed.ParticipantId);

        var result = await controller.Enrol(new EnrolmentRequest
        {
            EventId = 99999,
            CategoryId = seed.CategoryId
        });

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Empty(await db.EventEnrolments.ToListAsync());
    }

    [Fact]
    public void EnrolmentEndpointRequiresParticipantRole()
    {
        var method = typeof(EnrolmentsController)
            .GetMethod(nameof(EnrolmentsController.Enrol));

        Assert.NotNull(method);

        var attributes = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        Assert.Contains(attributes, attribute =>
            attribute.Roles?.Split(',')
                .Contains(UserRoles.Participant) == true);
    }

    private static EnrolmentsController CreateParticipantController(
        ApplicationDbContext db, int participantId)
    {
        var controller = new EnrolmentsController(db);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    participantId.ToString()),
                new Claim(ClaimTypes.Role, UserRoles.Participant)
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

    private static async Task<(
        ApplicationDbContext Db,
        int ParticipantId,
        int EventId,
        int CategoryId,
        int OtherEventId,
        int OtherCategoryId)> CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        var organiser = new User
        {
            FirstName = "Test",
            LastName = "Organiser",
            Email = "organiser@example.com",
            PasswordHash = "test-hash",
            Role = UserRoles.Organiser
        };

        var participant = new User
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            PasswordHash = "test-hash",
            Role = UserRoles.Participant
        };

        db.Users.AddRange(organiser, participant);
        await db.SaveChangesAsync();

        var firstEvent = new Event
        {
            Name = "Test Run",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5m,
            EventType = "Run",
            OrganiserId = organiser.Id
        };

        var secondEvent = new Event
        {
            Name = "Second Run",
            Description = "Another test event",
            EventDate = DateTime.UtcNow.AddDays(14),
            Location = "Pretoria",
            DistanceKm = 10m,
            EventType = "Run",
            OrganiserId = organiser.Id
        };

        db.Events.AddRange(firstEvent, secondEvent);
        await db.SaveChangesAsync();

        var firstCategory = new Category
        {
            EventId = firstEvent.Id,
            Name = "5 km",
            CategoryType = "Distance",
            DistanceKm = 5m
        };

        var secondCategory = new Category
        {
            EventId = secondEvent.Id,
            Name = "10 km",
            CategoryType = "Distance",
            DistanceKm = 10m
        };

        db.Categories.AddRange(firstCategory, secondCategory);
        await db.SaveChangesAsync();

        return (
            db,
            participant.Id,
            firstEvent.Id,
            firstCategory.Id,
            secondEvent.Id,
            secondCategory.Id);
    }
}