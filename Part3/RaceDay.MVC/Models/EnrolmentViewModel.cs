
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class EnrolmentViewModel
{
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public DateTime EnrolledUtc { get; set; }
}
