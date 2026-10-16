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

    [Fact]
    public async Task Register_ValidRequest_StoresHashedPassword()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var controller = new AuthController(db, hasher);

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            Password = "SecurePass123!",
            Role = "Participant"
        };

        var result = await controller.Register(request);

        Assert.IsType<CreatedResult>(result.Result);

        var savedUser = await db.Users.SingleAsync(
            u => u.Email == "participant@example.com");

        Assert.NotEqual(request.Password, savedUser.PasswordHash);

        Assert.Equal(
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(
                savedUser, savedUser.PasswordHash, request.Password));
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var controller = new AuthController(db, hasher);

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "admin@example.com",
            Password = "SecurePass123!",
            Role = "Administrator"
        };

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "Participant",
            Email = "existing@example.com",
            Role = "Participant"
        };

        existingUser.PasswordHash =
            hasher.HashPassword(existingUser, "SecurePass123!");

        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher);

        var request = new RegisterRequest
        {
            FirstName = "Another",
            LastName = "Participant",
            Email = "existing@example.com",
            Password = "AnotherPass123!",
            Role = "Participant"
        };

        var result = await controller.Register(request);

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "login@example.com",
            Role = "Participant"
        };

        user.PasswordHash =
            hasher.HashPassword(user, "CorrectPass123!");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher);

        var result = await controller.Login(new LoginRequest
        {
            Email = "login@example.com",
            Password = "WrongPass123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

}