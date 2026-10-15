using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Event
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [Required, MaxLength(250)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = "Run";

    public int OrganiserId { get; set; }
    public User? Organiser { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}

public static class EventTypes
{
    public const string Run = "Run";
    public const string Walk = "Walk";
    public const string Cycle = "Cycle";

    public static bool IsValid(string? value) =>
        value is not null &&
        (value.Equals(Run, StringComparison.OrdinalIgnoreCase) ||
         value.Equals(Walk, StringComparison.OrdinalIgnoreCase) ||
         value.Equals(Cycle, StringComparison.OrdinalIgnoreCase));
}
