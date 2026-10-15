using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists categories available for an event. Public endpoint.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetForEvent(int eventId)
    {
        if (!await db.Events.AnyAsync(x => x.Id == eventId)) return NotFound();
        var categories = await db.Categories.AsNoTracking().Where(x => x.EventId == eventId).ToListAsync();
        return Ok(categories.Select(ToResponse));
    }

    /// <summary>Adds a category to an event owned by the Organiser.</summary>
    [HttpPost("events/{eventId:int}/categories")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<CategoryResponse>> Create(int eventId, CategoryRequest request)
    {
        var itemEvent = await db.Events.SingleOrDefaultAsync(x => x.Id == eventId);
        if (itemEvent is null) return NotFound();
        if (!Owns(itemEvent.OrganiserId)) return Forbid();
        if (!ValidCategory(request)) return BadRequest(new { message = "CategoryType must be Age or Distance; provide valid category limits." });
        var category = new Category
        {
            EventId = eventId, Name = request.Name.Trim(),
            CategoryType = request.CategoryType.Equals("Age", StringComparison.OrdinalIgnoreCase) ? "Age" : "Distance",
            MinimumAge = request.MinimumAge, MaximumAge = request.MaximumAge, DistanceKm = request.DistanceKm
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return Created($"/api/categories/{category.Id}", ToResponse(category));
    }

    /// <summary>Updates a category belonging to an event owned by the Organiser.</summary>
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
    {
        var category = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound();
        if (category.Event is null || !Owns(category.Event.OrganiserId)) return Forbid();
        if (!ValidCategory(request)) return BadRequest(new { message = "Invalid category data." });
        category.Name = request.Name.Trim();
        category.CategoryType = request.CategoryType.Equals("Age", StringComparison.OrdinalIgnoreCase) ? "Age" : "Distance";
        category.MinimumAge = request.MinimumAge;
        category.MaximumAge = request.MaximumAge;
        category.DistanceKm = request.DistanceKm;
        await db.SaveChangesAsync();
        return Ok(ToResponse(category));
    }

    /// <summary>Deletes a category belonging to an event owned by the Organiser.</summary>
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound();
        if (category.Event is null || !Owns(category.Event.OrganiserId)) return Forbid();
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private bool Owns(int organiserId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id == organiserId;

    private static bool ValidCategory(CategoryRequest request)
    {
        if (request.CategoryType.Equals("Age", StringComparison.OrdinalIgnoreCase))
            return request.MinimumAge.HasValue && request.MaximumAge.HasValue &&
                   request.MinimumAge.Value <= request.MaximumAge.Value && request.DistanceKm is null;
        if (request.CategoryType.Equals("Distance", StringComparison.OrdinalIgnoreCase))
            return request.DistanceKm.HasValue && request.DistanceKm.Value > 0 &&
                   request.MinimumAge is null && request.MaximumAge is null;
        return false;
    }

    private static CategoryResponse ToResponse(Category x) =>
        new(x.Id, x.EventId, x.Name, x.CategoryType, x.MinimumAge, x.MaximumAge, x.DistanceKm);
}
