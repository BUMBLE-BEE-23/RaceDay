
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class ProfileTests
{
    [Fact]
    public async Task GetMyProfile_ExistingUser_ReturnsOwnProfile()
    {
        await using var db = await CreateDatabase();
        var user = await CreateUser(db);
        var controller = CreateController(db, user.Id);

        var response = await controller.GetMyProfile();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var profile = Assert.IsType<UserResponse>(ok.Value);

        Assert.Equal(user.Id, profile.Id);
        Assert.Equal(user.Email, profile.Email);
        Assert.Equal(user.Role, profile.Role);
    }

    [Fact]
    public async Task GetMyProfile_UnknownUser_ReturnsNotFound()
    {
        await using var db = await CreateDatabase();
        var controller = CreateController(db, 999);

        var response = await controller.GetMyProfile();

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task UpdateMyProfile_ValidRequest_UpdatesOwnDetails()
    {
        await using var db = await CreateDatabase();
        var user = await CreateUser(db);
        var originalEmail = user.Email;
        var originalRole = user.Role;
        var controller = CreateController(db, user.Id);

        var response = await controller.UpdateMyProfile(
            new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "Participant",
                PhoneNumber = "0712345678"
            });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var profile = Assert.IsType<UserResponse>(ok.Value);

        Assert.Equal("Updated", profile.FirstName);
        Assert.Equal("Participant", profile.LastName);
        Assert.Equal("0712345678", profile.PhoneNumber);
        Assert.Equal(originalEmail, profile.Email);
        Assert.Equal(originalRole, profile.Role);

        var saved = await db.Users.SingleAsync(x => x.Id == user.Id);
        Assert.Equal("Updated", saved.FirstName);
        Assert.Equal(originalEmail, saved.Email);
        Assert.Equal(originalRole, saved.Role);
    }

    [Fact]
    public async Task UpdateMyProfile_UnknownUser_ReturnsNotFound()
    {
        await using var db = await CreateDatabase();
        var controller = CreateController(db, 999);

        var response = await controller.UpdateMyProfile(
            new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "User",
                PhoneNumber = null
            });

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task UpdateMyProfile_TrimsWhitespaceFromNames()
    {
        await using var db = await CreateDatabase();
        var user = await CreateUser(db);
        var controller = CreateController(db, user.Id);

        var response = await controller.UpdateMyProfile(
            new UpdateProfileRequest
            {
                FirstName = "  Alex  ",
                LastName = "  Smith  ",
                PhoneNumber = "  0712345678  "
            });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var profile = Assert.IsType<UserResponse>(ok.Value);

        Assert.Equal("Alex", profile.FirstName);
        Assert.Equal("Smith", profile.LastName);
        Assert.Equal("0712345678", profile.PhoneNumber);
    }

    [Fact]
    public async Task UpdateMyProfile_NullPhoneNumber_KeepsPhoneNumberNull()
    {
        await using var db = await CreateDatabase();
        var user = await CreateUser(db);
        var controller = CreateController(db, user.Id);

        var response = await controller.UpdateMyProfile(
            new UpdateProfileRequest
            {
                FirstName = "Alex",
                LastName = "Smith",
                PhoneNumber = null
            });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var profile = Assert.IsType<UserResponse>(ok.Value);

        Assert.Null(profile.PhoneNumber);

        var saved = await db.Users.SingleAsync(x => x.Id == user.Id);
        Assert.Null(saved.PhoneNumber);
    }

    private static Task<ApplicationDbContext> CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return Task.FromResult(new ApplicationDbContext(options));
    }

    private static async Task<User> CreateUser(
        ApplicationDbContext db)
    {
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "profile@example.com",
            PasswordHash = "test-hash",
            Role = UserRoles.Participant,
            PhoneNumber = "0700000000"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    private static ProfileController CreateController(
        ApplicationDbContext db, int userId)
    {
        var controller = new ProfileController(db);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),
                new Claim(
                    ClaimTypes.Role,
                    UserRoles.Participant)
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
