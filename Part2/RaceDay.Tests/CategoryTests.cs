
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
    public async Task GetForEvent_ReturnsCategoriesForExistingEvent()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        db.Categories.Add(new Category
        {
            EventId = seed.EventId,
            Name = "Junior",
            CategoryType = "Age",
            MinimumAge = 12,
            MaximumAge = 17
        });

        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);
        var result = await controller.GetForEvent(seed.EventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<CategoryResponse>>(ok.Value);

        Assert.Single(categories);
        Assert.Equal("Junior", categories.First().Name);
    }

    [Fact]
    public async Task GetForEvent_ReturnsNotFoundForMissingEvent()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = new CategoriesController(db);

        var result = await controller.GetForEvent(99999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidAgeCategory_ReturnsCreated()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var request = new CategoryRequest
        {
            Name = "Youth",
            CategoryType = "Age",
            MinimumAge = 13,
            MaximumAge = 18,
            DistanceKm = null
        };

        var result = await controller.Create(seed.EventId, request);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<CategoryResponse>(created.Value);

        Assert.Equal("Youth", response.Name);
        Assert.Equal("Age", response.CategoryType);
        Assert.Equal(13, response.MinimumAge);
        Assert.Equal(18, response.MaximumAge);
        Assert.Single(await db.Categories.ToListAsync());
    }

    [Fact]
    public async Task Create_InvalidAgeRange_ReturnsBadRequest()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var request = new CategoryRequest
        {
            Name = "Invalid Age Group",
            CategoryType = "Age",
            MinimumAge = 30,
            MaximumAge = 18,
            DistanceKm = null
        };

        var result = await controller.Create(seed.EventId, request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.Categories.ToListAsync());
    }

    [Fact]
    public async Task Create_CategoryForMissingEvent_ReturnsNotFound()
    {
        var seed = await CreateDatabase();
        await using var db = seed.Db;

        var controller = CreateOrganiserController(db, seed.OrganiserId);

        var request = new CategoryRequest
        {
            Name = "5 km",
            CategoryType = "Distance",
            DistanceKm = 5m
        };

        var result = await controller.Create(99999, request);

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Empty(await db.Categories.ToListAsync());
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

    private static async Task<(
        ApplicationDbContext Db,
        int OrganiserId,
        int EventId)> CreateDatabase()
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

        db.Users.Add(organiser);
        await db.SaveChangesAsync();

        var itemEvent = new Event
        {
            Name = "Test Run",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5m,
            EventType = "Run",
            OrganiserId = organiser.Id
        };

        db.Events.Add(itemEvent);
        await db.SaveChangesAsync();

        return (db, organiser.Id, itemEvent.Id);
    }
}
