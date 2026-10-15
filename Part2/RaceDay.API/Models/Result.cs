using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Result
{
    public int Id { get; set; }
    public int EventEnrolmentId { get; set; }
    public EventEnrolment? EventEnrolment { get; set; }

    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }

    public TimeSpan FinishTime { get; set; }

    public DateTime RecordedUtc { get; set; } = DateTime.UtcNow;
}
