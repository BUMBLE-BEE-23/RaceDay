using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher) : ControllerBase
{
    /// <summary>Registers a user as an Organiser or Participant. Passwords are hashed before storage.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var role = request.Role?.Trim();
        if (role is null || (!role.Equals(UserRoles.Organiser, StringComparison.OrdinalIgnoreCase)
            && !role.Equals(UserRoles.Participant, StringComparison.OrdinalIgnoreCase)))
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = role.Equals(UserRoles.Organiser, StringComparison.OrdinalIgnoreCase)
                ? UserRoles.Organiser : UserRoles.Participant,
            PhoneNumber = request.PhoneNumber?.Trim()
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Created("/api/profile/me", ToResponse(user));
    }

    /// <summary>Logs in and establishes an authenticated cookie and server-side session.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(ToResponse(user));
    }

    /// <summary>Signs out and clears the session.</summary>
    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.Role, user.PhoneNumber);
}
