using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Enrols the authenticated Participant in an event and selected category.
    /// New enrolments automatically receive Pending status.
    /// </summary>
    [HttpPost("enrolments")]
    [Authorize(Roles = UserRoles.Participant)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EnrolmentResponse>> Enrol(EnrolmentRequest request)
    {
        var participantId = GetUserId();

        if (participantId is null)
            return Unauthorized();

        var eventExists = await db.Events
            .AnyAsync(x => x.Id == request.EventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var category = await db.Categories
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId);

        if (category is null || category.EventId != request.EventId)
        {
            return BadRequest(new
            {
                message = "The selected category does not belong to this event."
            });
        }

        var alreadyEnrolled = await db.EventEnrolments
            .AnyAsync(x =>
                x.ParticipantId == participantId.Value &&
                x.EventId == request.EventId);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this event."
            });
        }

        var item = new EventEnrolment
        {
            ParticipantId = participantId.Value,
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            Status = "Pending"
        };

        db.EventEnrolments.Add(item);
        await db.SaveChangesAsync();

        var details = await (
            from enrolment in db.EventEnrolments
            join ev in db.Events on enrolment.EventId equals ev.Id
            join cat in db.Categories on enrolment.CategoryId equals cat.Id
            join participant in db.Users on enrolment.ParticipantId equals participant.Id
            where enrolment.Id == item.Id
            select new
            {
                EnrolmentId = enrolment.Id,
                EventId = ev.Id,
                EventName = ev.Name,
                CategoryId = cat.Id,
                CategoryName = cat.Name,
                ParticipantId = participant.Id,
                ParticipantName = participant.FirstName + " " + participant.LastName,
                enrolment.EnrolledUtc,
                enrolment.Status
            }
        ).SingleAsync();

        var response = new EnrolmentResponse(
            details.EnrolmentId,
            details.EventId,
            details.EventName,
            details.CategoryId,
            details.CategoryName,
            details.ParticipantId,
            details.ParticipantName,
            details.EnrolledUtc,
            details.Status
        );

        return Created($"/api/events/{item.EventId}/enrolments", response);
    }

    /// <summary>
    /// Lists enrolments for an event owned by the authenticated Organiser.
    /// </summary>
    [HttpGet("events/{eventId:int}/enrolments")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetForEvent(int eventId)
    {
        var eventItem = await db.Events
            .SingleOrDefaultAsync(x => x.Id == eventId);

        if (eventItem is null)
            return NotFound();

        if (!Owns(eventItem.OrganiserId))
            return Forbid();

        var details = await (
            from enrolment in db.EventEnrolments
            join ev in db.Events on enrolment.EventId equals ev.Id
            join cat in db.Categories on enrolment.CategoryId equals cat.Id
            join participant in db.Users on enrolment.ParticipantId equals participant.Id
            where ev.Id == eventId
            select new
            {
                EnrolmentId = enrolment.Id,
                EventId = ev.Id,
                EventName = ev.Name,
                CategoryId = cat.Id,
                CategoryName = cat.Name,
                ParticipantId = participant.Id,
                ParticipantName = participant.FirstName + " " + participant.LastName,
                enrolment.EnrolledUtc,
                enrolment.Status
            }
        ).ToListAsync();

        var responses = details.Select(x => new EnrolmentResponse(
            x.EnrolmentId,
            x.EventId,
            x.EventName,
            x.CategoryId,
            x.CategoryName,
            x.ParticipantId,
            x.ParticipantName,
            x.EnrolledUtc,
            x.Status
        )).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Lists all enrolments belonging to the authenticated Participant.
    /// </summary>
    [HttpGet("enrolments/me")]
    [Authorize(Roles = UserRoles.Participant)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetMyEnrolments()
    {
        var participantId = GetUserId();

        if (participantId is null)
            return Unauthorized();

        var details = await (
            from enrolment in db.EventEnrolments
            join ev in db.Events on enrolment.EventId equals ev.Id
            join cat in db.Categories on enrolment.CategoryId equals cat.Id
            join participant in db.Users on enrolment.ParticipantId equals participant.Id
            where enrolment.ParticipantId == participantId.Value
            select new
            {
                EnrolmentId = enrolment.Id,
                EventId = ev.Id,
                EventName = ev.Name,
                CategoryId = cat.Id,
                CategoryName = cat.Name,
                ParticipantId = participant.Id,
                ParticipantName = participant.FirstName + " " + participant.LastName,
                enrolment.EnrolledUtc,
                enrolment.Status
            }
        ).ToListAsync();

        var responses = details.Select(x => new EnrolmentResponse(
            x.EnrolmentId,
            x.EventId,
            x.EventName,
            x.CategoryId,
            x.CategoryName,
            x.ParticipantId,
            x.ParticipantName,
            x.EnrolledUtc,
            x.Status
        )).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Allows an Organiser to confirm or cancel enrolments for their own events.
    /// </summary>
    [HttpPut("enrolments/{enrolmentId:int}/status")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> UpdateStatus(
        int enrolmentId,
        [FromBody] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { message = "A status is required." });
        }

        status = status.Trim();

        if (!status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Status must be Confirmed or Cancelled."
            });
        }

        var enrolment = await db.EventEnrolments
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.Id == enrolmentId);

        if (enrolment is null)
            return NotFound(new { message = "Enrolment not found." });

        if (!Owns(enrolment.Event.OrganiserId))
            return Forbid();

        enrolment.Status = status.Equals(
            "Confirmed",
            StringComparison.OrdinalIgnoreCase)
                ? "Confirmed"
                : "Cancelled";

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment status updated successfully.",
            enrolmentId = enrolment.Id,
            status = enrolment.Status
        });
    }

    private int? GetUserId() =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id)
                ? id
                : null;

    private bool Owns(int organiserId) =>
        GetUserId() == organiserId;
}