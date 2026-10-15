using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = UserRoles.Participant;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}

public static class UserRoles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
