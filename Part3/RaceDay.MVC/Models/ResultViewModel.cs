
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class ResultViewModel
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public int EnrolmentId { get; set; }

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue,
        ErrorMessage = "Finishing position must be at least 1.")]
    public int FinishingPosition { get; set; }

    [Required]
    public TimeSpan FinishTime { get; set; }

    public DateTime RecordedUtc { get; set; }
}
