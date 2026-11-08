
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class CategoryViewModel
{
    public int Id { get; set; }

    public int EventId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category Type")]
    public string CategoryType { get; set; } = string.Empty;

    [Range(0, 120)]
    [Display(Name = "Minimum Age")]
    public int? MinimumAge { get; set; }

    [Range(0, 120)]
    [Display(Name = "Maximum Age")]
    public int? MaximumAge { get; set; }

    [Range(0.1, 1000)]
    [Display(Name = "Distance (KM)")]
    public decimal? DistanceKm { get; set; }
}
