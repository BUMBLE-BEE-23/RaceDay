
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class EventViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Event Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Event Date")]
    [DataType(DataType.DateTime)]
    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

    [Required]
    [StringLength(250)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(0.1, 1000)]
    [Display(Name = "Distance (KM)")]
    public decimal DistanceKm { get; set; }

    [Required]
    [Display(Name = "Event Type")]
    public string EventType { get; set; } = string.Empty;

    public int OrganiserId { get; set; }

    [Display(Name = "Event Banner")]
    public IFormFile? BannerFile { get; set; }

    public string? BannerUrl { get; set; }
}
