
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists events in date order. Public endpoint.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEvents()
    {
        var events = await db.Events
            .AsNoTracking()
            .OrderBy(x => x.EventDate)
            .ToListAsync();

        return Ok(events.Select(ToResponse));
    }

    /// <summary>Gets one event by ID. Public endpoint.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetEvent(int id)
    {
        var item = await db.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    /// <summary>Creates an event for the authenticated Organiser.</summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponse>> CreateEvent(EventRequest request)
    {
        if (!EventTypes.IsValid(request.EventType))
            return BadRequest(new
            {
                message = "EventType must be Run, Walk, or Cycle."
            });

        var organiserId = GetUserId();
        if (organiserId is null)
            return Unauthorized();

        var item = new Event
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = NormalizeType(request.EventType),
            OrganiserId = organiserId.Value
        };

        db.Events.Add(item);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = item.Id },
            ToResponse(item));
    }

    /// <summary>Updates an event owned by the authenticated Organiser.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> UpdateEvent(
        int id, EventRequest request)
    {
        if (!EventTypes.IsValid(request.EventType))
            return BadRequest(new
            {
                message = "EventType must be Run, Walk, or Cycle."
            });

        var item = await db.Events
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item is null)
            return NotFound();

        if (!Owns(item))
            return Forbid();

        item.Name = request.Name.Trim();
        item.Description = request.Description;
        item.EventDate = request.EventDate;
        item.Location = request.Location.Trim();
        item.DistanceKm = request.DistanceKm;
        item.EventType = NormalizeType(request.EventType);

        await db.SaveChangesAsync();

        return Ok(ToResponse(item));
    }

    /// <summary>Deletes an event owned by the authenticated Organiser.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var item = await db.Events
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item is null)
            return NotFound();

        if (!Owns(item))
            return Forbid();

        db.Events.Remove(item);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private int? GetUserId() =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : null;

    private bool Owns(Event item) =>
        GetUserId() == item.OrganiserId;

    private static string NormalizeType(string value) =>
        value.Equals(EventTypes.Run, StringComparison.OrdinalIgnoreCase)
            ? EventTypes.Run
            : value.Equals(EventTypes.Walk, StringComparison.OrdinalIgnoreCase)
                ? EventTypes.Walk
                : EventTypes.Cycle;

    private static EventResponse ToResponse(Event x) =>
        new(
            x.Id,
            x.Name,
            x.Description,
            x.EventDate,
            x.Location,
            x.DistanceKm,
            x.EventType,
            x.OrganiserId);
}
