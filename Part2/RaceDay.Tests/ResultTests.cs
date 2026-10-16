
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class ResultTests
{
    [Fact]
    public async Task Record_ValidResult_ReturnsOkAndSavesResult()
    {
        await using var db = await CreateDatabase();
        var seed = await SeedEnrolment(db);
        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var response = await controller.Record(new RecordResultRequest
        {
            EnrolmentId = seed.EnrolmentId,
            FinishingPosition = 1,
            FinishTime = TimeSpan.FromMinutes(25)
        });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var result = Assert.IsType<ResultResponse>(ok.Value);

        Assert.Equal(seed.EventId, result.EventId);
        Assert.Equal(seed.ParticipantId, result.ParticipantId);
        Assert.Equal(1, result.FinishingPosition);
        Assert.Equal(TimeSpan.FromMinutes(25), result.FinishTime);
        Assert.Single(await db.Results.ToListAsync());
    }

    [Fact]
    public async Task Record_UnknownEnrolment_ReturnsNotFound()
    {
        await using var db = await CreateDatabase();
        var seed = await SeedEnrolment(db);
        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var response = await controller.Record(new RecordResultRequest
        {
            EnrolmentId = 999,
            FinishingPosition = 1,
            FinishTime = TimeSpan.FromMinutes(20)
        });

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task Record_NegativeFinishTime_ReturnsBadRequest()
    {
        await using var db = await CreateDatabase();
        var seed = await SeedEnrolment(db);
        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var response = await controller.Record(new RecordResultRequest
        {
            EnrolmentId = seed.EnrolmentId,
            FinishingPosition = 1,
            FinishTime = TimeSpan.FromSeconds(-1)
        });

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Empty(await db.Results.ToListAsync());
    }

    [Fact]
    public async Task Record_AnotherOrganisersEvent_ReturnsForbid()
    {
        await using var db = await CreateDatabase();
        var seed = await SeedEnrolment(db);
        var controller = CreateOrganiserController(db, 999);

        var response = await controller.Record(new RecordResultRequest
        {
            EnrolmentId = seed.EnrolmentId,
            FinishingPosition = 1,
            FinishTime = TimeSpan.FromMinutes(30)
        });

        Assert.IsType<ForbidResult>(response.Result);
        Assert.Empty(await db.Results.ToListAsync());
    }

    [Fact]
    public async Task Record_ExistingResult_UpdatesIt()
    {
        await using var db = await CreateDatabase();
        var seed = await SeedEnrolment(db);

        db.Results.Add(new Result
        {
            EventEnrolmentId = seed.EnrolmentId,
            FinishingPosition = 3,
            FinishTime = TimeSpan.FromMinutes(35)
        });
        await db.SaveChangesAsync();

        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var response = await controller.Record(new RecordResultRequest
        {
            EnrolmentId = seed.EnrolmentId,
            FinishingPosition = 2,
            FinishTime = TimeSpan.FromMinutes(28)
        });

        Assert.IsType<OkObjectResult>(response.Result);

        var saved = await db.Results.SingleAsync();
        Assert.Equal(2, saved.FinishingPosition);
        Assert.Equal(TimeSpan.FromMinutes(28), saved.FinishTime);
    }

    private static async Task<ApplicationDbContext> CreateDatabase()
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

        return db;
    }

    private static async Task<(
        int OrganiserId,
        int ParticipantId,
        int EventId,
        int EnrolmentId)> SeedEnrolment(ApplicationDbContext db)
    {
        var organiser = await db.Users.SingleAsync(
            x => x.Role == UserRoles.Organiser);
        var participant = await db.Users.SingleAsync(
            x => x.Role == UserRoles.Participant);

        var ev = new RaceDay.API.Models.Event
        {
            Name = "Test Run",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Run",
            OrganiserId = organiser.Id
        };

        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var category = new Category
        {
            EventId = ev.Id,
            Name = "5 km",
            CategoryType = "Distance",
            DistanceKm = 5
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var enrolment = new EventEnrolment
        {
            EventId = ev.Id,
            CategoryId = category.Id,
            ParticipantId = participant.Id
        };

        db.EventEnrolments.Add(enrolment);
        await db.SaveChangesAsync();

        return (
            organiser.Id,
            participant.Id,
            ev.Id,
            enrolment.Id);
    }

    private static ResultsController CreateOrganiserController(
        ApplicationDbContext db, int organiserId)
    {
        var controller = new ResultsController(db);

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
