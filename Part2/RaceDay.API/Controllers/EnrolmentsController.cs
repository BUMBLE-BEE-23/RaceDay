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
            CategoryId = request.CategoryId
        };

        db.EventEnrolments.Add(item);
        await db.SaveChangesAsync();

        // Build the response after querying the database.
        var details = await (
            from enrolment in db.EventEnrolments
            join ev in db.Events
                on enrolment.EventId equals ev.Id
            join cat in db.Categories
                on enrolment.CategoryId equals cat.Id
            join participant in db.Users
                on enrolment.ParticipantId equals participant.Id
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
                enrolment.EnrolledUtc
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
            details.EnrolledUtc
        );

        return Created($"/api/events/{item.EventId}/enrolments", response);
    }

    /// <summary>
    /// Lists enrolments for an event owned by the authenticated Organiser.
    /// </summary>
    [HttpGet("events/{eventId:int}/enrolments")]
    [Authorize(Roles = UserRoles.Organiser)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetForEvent(
        int eventId)
    {
        var eventItem = await db.Events
            .SingleOrDefaultAsync(x => x.Id == eventId);

        if (eventItem is null)
            return NotFound();

        if (!Owns(eventItem.OrganiserId))
            return Forbid();

        var details = await (
            from enrolment in db.EventEnrolments
            join ev in db.Events
                on enrolment.EventId equals ev.Id
            join cat in db.Categories
                on enrolment.CategoryId equals cat.Id
            join participant in db.Users
                on enrolment.ParticipantId equals participant.Id
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
                enrolment.EnrolledUtc
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
            x.EnrolledUtc
        )).ToList();

        return Ok(responses);
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
