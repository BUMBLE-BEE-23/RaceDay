using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string CategoryType { get; set; } = "Age";

    [Range(0, 120)]
    public int? MinimumAge { get; set; }

    [Range(0, 120)]
    public int? MaximumAge { get; set; }

    [Range(0.1, 1000)]
    public decimal? DistanceKm { get; set; }
public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}
