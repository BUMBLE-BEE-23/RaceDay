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

public class AuthAndAccessTests
{
    [Fact]
    public void PasswordHashing_VerifiesCorrectPassword()
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Role = "Participant"
        };

        var hash = hasher.HashPassword(user, "RaceDay123!");
        var result = hasher.VerifyHashedPassword(user, hash, "RaceDay123!");

        Assert.Equal(
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success,
            result);
    }

    [Theory]
    [InlineData("CreateEvent", "Organiser")]
    [InlineData("UpdateEvent", "Organiser")]
    [InlineData("DeleteEvent", "Organiser")]
    public void EventManagement_RequiresOrganiserRole(
        string actionName,
        string expectedRole)
    {
        var method = typeof(EventsController).GetMethod(actionName);

        Assert.NotNull(method);

        var attributes = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        Assert.Contains(attributes, attribute =>
            attribute.Roles?.Split(',').Contains(expectedRole) == true);
    }

    [Fact]
    public async Task EventCreation_RejectsUnsupportedEventType()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        var controller = new EventsController(db);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Organiser")
            },
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var request = new EventRequest
        {
            Name = "Test Event",
            Description = "Description",
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Flying"
        };

        var result = await controller.CreateEvent(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}