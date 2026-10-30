using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public class VenueViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the venue name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Please enter the city.")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
}

public class VenuesPageViewModel
{
    public List<VenueViewModel> Venues { get; set; } = new();
}
