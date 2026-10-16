
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class CategoryTests
{
    [Fact]
    public async Task GetForEvent_UnknownEvent_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new CategoriesController(db);

        var result = await controller.GetForEvent(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetForEvent_ExistingEvent_ReturnsOk()
    {
        await using var db = CreateDatabase();
        await SeedEvent(db);

        var controller = new CategoriesController(db);

        var result = await controller.GetForEvent(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_CategoryForOwnEvent_ReturnsCreated()
    {
        await using var db = CreateDatabase();
        await SeedEvent(db);

        var controller = CreateOrganiserController(db, 1);

        var result = await controller.Create(1, new CategoryRequest
        {
            Name = "5 km",
            CategoryType = "Distance",
            DistanceKm = 5
        });

        Assert.IsType<CreatedResult>(result.Result);

        var saved = await db.Categories.SingleAsync();
        Assert.Equal("5 km", saved.Name);
        Assert.Equal("Distance", saved.CategoryType);
        Assert.Equal(5m, saved.DistanceKm);
    }

    [Fact]
    public async Task Create_InvalidCategory_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        await SeedEvent(db);

        var controller = CreateOrganiserController(db, 1);

        var result = await controller.Create(1, new CategoryRequest
        {
            Name = "Invalid category",
            CategoryType = "Distance",
            DistanceKm = null,
            MinimumAge = 10,
            MaximumAge = 20
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.Categories.ToListAsync());
    }

    [Fact]
    public async Task Create_CategoryForAnotherOrganisersEvent_ReturnsForbid()
    {
        await using var db = CreateDatabase();
        await SeedEvent(db);

        var controller = CreateOrganiserController(db, 2);

        var result = await controller.Create(1, new CategoryRequest
        {
            Name = "5 km",
            CategoryType = "Distance",
            DistanceKm = 5
        });

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Empty(await db.Categories.ToListAsync());
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        db.Users.AddRange(
            new User
            {
                Id = 1,
                FirstName = "First",
                LastName = "Organiser",
                Email = "first@example.com",
                PasswordHash = "test-hash",
                Role = UserRoles.Organiser
            },
            new User
            {
                Id = 2,
                FirstName = "Second",
                LastName = "Organiser",
                Email = "second@example.com",
                PasswordHash = "test-hash",
                Role = UserRoles.Organiser
            });

        db.SaveChanges();

        return db;
    }

    private static async Task SeedEvent(ApplicationDbContext db)
    {
        db.Events.Add(new Event
        {
            Id = 1,
            Name = "Test Run",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Run",
            OrganiserId = 1
        });

        await db.SaveChangesAsync();
    }

    private static CategoriesController CreateOrganiserController(
        ApplicationDbContext db, int organiserId)
    {
        var controller = new CategoriesController(db);

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
