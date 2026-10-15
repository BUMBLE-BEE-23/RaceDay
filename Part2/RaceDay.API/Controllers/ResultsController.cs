using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Records or updates the finishing position and time for an enrolment in an event owned by the Organiser.</summary>
    [HttpPost("results")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<ResultResponse>> Record(RecordResultRequest request)
    {
        if (request.FinishTime < TimeSpan.Zero)
            return BadRequest(new { message = "FinishTime cannot be negative." });

        var enrolment = await db.EventEnrolments
            .Include(x => x.Event)
            .Include(x => x.Participant)
            .Include(x => x.Result)
            .SingleOrDefaultAsync(x => x.Id == request.EnrolmentId);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Event is null || !Owns(enrolment.Event.OrganiserId)) return Forbid();

        var result = enrolment.Result;
        if (result is null)
        {
            result = new Result { EventEnrolmentId = enrolment.Id };
            db.Results.Add(result);
        }
        result.FinishingPosition = request.FinishingPosition;
        result.FinishTime = request.FinishTime;
        result.RecordedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToResponse(result, enrolment));
    }

    /// <summary>Returns the authenticated Participant's own recorded results.</summary>
    [HttpGet("results/me")]
    [Authorize(Roles = UserRoles.Participant)]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetMyResults()
    {
        var participantId = GetUserId();
        if (participantId is null) return Unauthorized();
        var rows = await db.Results
            .Include(x => x.EventEnrolment!).ThenInclude(x => x.Event)
            .Include(x => x.EventEnrolment!).ThenInclude(x => x.Participant)
            .Where(x => x.EventEnrolment!.ParticipantId == participantId)
            .ToListAsync();
        return Ok(rows.Select(x => ToResponse(x, x.EventEnrolment!)));
    }

    /// <summary>Lists results for an event owned by the authenticated Organiser.</summary>
    [HttpGet("events/{eventId:int}/results")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetForEvent(int eventId)
    {
        var ev = await db.Events.SingleOrDefaultAsync(x => x.Id == eventId);
        if (ev is null) return NotFound();
        if (!Owns(ev.OrganiserId)) return Forbid();
        var rows = await db.Results
            .Include(x => x.EventEnrolment!).ThenInclude(x => x.Event)
            .Include(x => x.EventEnrolment!).ThenInclude(x => x.Participant)
            .Where(x => x.EventEnrolment!.EventId == eventId)
            .ToListAsync();
        return Ok(rows.Select(x => ToResponse(x, x.EventEnrolment!)));
    }

    private int? GetUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool Owns(int organiserId) => GetUserId() == organiserId;
    private static ResultResponse ToResponse(Result result, EventEnrolment enrolment) =>
        new(result.Id, enrolment.EventId, enrolment.Event?.Name ?? string.Empty,
            enrolment.ParticipantId,
            enrolment.Participant is null ? string.Empty : enrolment.Participant.FirstName + " " + enrolment.Participant.LastName,
            result.FinishingPosition, result.FinishTime, result.RecordedUtc);
}
