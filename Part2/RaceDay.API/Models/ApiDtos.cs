
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class RegisterRequest
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? PhoneNumber);

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }
}

public class EventRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(250)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    [Required]
    public string EventType { get; set; } = string.Empty;
}

public class CategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string CategoryType { get; set; } = string.Empty;

    [Range(0, 120)]
    public int? MinimumAge { get; set; }

    [Range(0, 120)]
    public int? MaximumAge { get; set; }

    [Range(0.1, 1000)]
    public decimal? DistanceKm { get; set; }
}

public class EnrolmentRequest
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class RecordResultRequest
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }

    public TimeSpan FinishTime { get; set; }
}

public record EventResponse(
    int Id,
    string Name,
    string Description,
    DateTime EventDate,
    string Location,
    decimal DistanceKm,
    string EventType,
    int OrganiserId);

public record CategoryResponse(
    int Id,
    int EventId,
    string Name,
    string CategoryType,
    int? MinimumAge,
    int? MaximumAge,
    decimal? DistanceKm);

public record EnrolmentResponse(
    int Id,
    int EventId,
    string EventName,
    int CategoryId,
    string CategoryName,
    int ParticipantId,
    string ParticipantName,
    DateTime EnrolledUtc);

public record ResultResponse(
    int Id,
    int EventId,
    string EventName,
    int ParticipantId,
    string ParticipantName,
    int FinishingPosition,
    TimeSpan FinishTime,
    DateTime RecordedUtc);
