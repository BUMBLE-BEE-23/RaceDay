using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Returns the authenticated user's own profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetMyProfile()
    {
        var user = await CurrentUser();
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    /// <summary>Updates the authenticated user's own non-security profile fields.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> UpdateMyProfile(UpdateProfileRequest request)
    {
        var user = await CurrentUser();
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        await db.SaveChangesAsync();
        return Ok(ToResponse(user));
    }

    private Task<User?> CurrentUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId)
            ? db.Users.SingleOrDefaultAsync(x => x.Id == userId)
            : Task.FromResult<User?>(null);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.Role, user.PhoneNumber);
}
