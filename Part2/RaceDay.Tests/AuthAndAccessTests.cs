
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        var hasher = new PasswordHasher<User>();
        var user = CreateUser("test@example.com", UserRoles.Participant);

        var hash = hasher.HashPassword(user, "RaceDay123!");

        Assert.NotEqual("RaceDay123!", hash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, hash, "RaceDay123!"));
    }

    [Fact]
    public void PasswordHashing_RejectsIncorrectPassword()
    {
        var hasher = new PasswordHasher<User>();
        var user = CreateUser("test@example.com", UserRoles.Participant);
        user.PasswordHash = hasher.HashPassword(user, "CorrectPass123!");

        Assert.Equal(
            PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(
                user, user.PasswordHash, "WrongPass123!"));
    }

    [Theory]
    [InlineData("CreateEvent", UserRoles.Organiser)]
    [InlineData("UpdateEvent", UserRoles.Organiser)]
    [InlineData("DeleteEvent", UserRoles.Organiser)]
    public void EventManagement_RequiresOrganiserRole(
        string actionName, string expectedRole)
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
        await using var db = CreateDatabase();
        var organiser = CreateUser("organiser@example.com", UserRoles.Organiser);
        db.Users.Add(organiser);
        await db.SaveChangesAsync();

        var controller = CreateEventsController(db, organiser.Id);

        var result = await controller.CreateEvent(new EventRequest
        {
            Name = "Test Event",
            Description = "Description",
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Flying"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.Events.ToListAsync());
    }

    [Fact]
    public async Task Register_ValidRequest_StoresHashedPassword()
    {
        await using var db = CreateDatabase();
        var hasher = new PasswordHasher<User>();
        var controller = new AuthController(db, hasher);

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            Password = "SecurePass123!",
            Role = UserRoles.Participant
        };

        var result = await controller.Register(request);

        Assert.IsType<CreatedResult>(result.Result);

        var savedUser = await db.Users.SingleAsync(
            u => u.Email == "participant@example.com");

        Assert.NotEqual(request.Password, savedUser.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(
                savedUser, savedUser.PasswordHash, request.Password));
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var controller = new AuthController(db, new PasswordHasher<User>());

        var result = await controller.Register(new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "admin@example.com",
            Password = "SecurePass123!",
            Role = "Administrator"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        var hasher = new PasswordHasher<User>();
        var existingUser = CreateUser(
            "existing@example.com", UserRoles.Participant);

        existingUser.PasswordHash =
            hasher.HashPassword(existingUser, "SecurePass123!");

        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher);

        var result = await controller.Register(new RegisterRequest
        {
            FirstName = "Another",
            LastName = "Participant",
            Email = "EXISTING@example.com",
            Password = "AnotherPass123!",
            Role = UserRoles.Participant
        });

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Single(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await using var db = CreateDatabase();
        var hasher = new PasswordHasher<User>();
        var user = CreateUser("login@example.com", UserRoles.Participant);
        user.PasswordHash = hasher.HashPassword(user, "CorrectPass123!");

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

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        await using var db = CreateDatabase();
        var controller = new AuthController(
            db, new PasswordHasher<User>());

        var result = await controller.Login(new LoginRequest
        {
            Email = "missing@example.com",
            Password = "SecurePass123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateUser(string email, string role)
    {
        return new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = "test-hash",
            Role = role
        };
    }

    private static EventsController CreateEventsController(
        ApplicationDbContext db, int organiserId)
    {
        var controller = new EventsController(db);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, organiserId.ToString()),
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
