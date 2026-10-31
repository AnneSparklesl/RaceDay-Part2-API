using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

// A category as the API sends it
public class CategoryItemViewModel
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
}

// Add / edit form
public class CategoryFormViewModel
{
    public int Id { get; set; }
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter the category name.")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the distance in km.")]
    [Range(typeof(decimal), "0.1", "999.99", ErrorMessage = "Distance must be between 0.1 and 999.99 km.")]
    public decimal? DistanceKm { get; set; }
}

// Everything the categories page shows
public class CategoriesPageViewModel
{
    public EventItemViewModel Event { get; set; } = new();
    public List<CategoryItemViewModel> Categories { get; set; } = new();
}
