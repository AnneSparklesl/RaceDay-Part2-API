using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RaceDay.Web.ViewModels;

// An event as the API sends it
public class EventItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int VenueId { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int OrganiserId { get; set; }
    public string Organiser { get; set; } = string.Empty;
}

// The create / edit form
public class EventFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the event name.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description.")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose the date and time.")]
    public DateTime? EventDate { get; set; }

    [Required(ErrorMessage = "Please choose a venue.")]
    public int? VenueId { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    public string? EventType { get; set; }

    [Required(ErrorMessage = "Please enter the distance in km.")]
    [Range(typeof(decimal), "0.1", "999.99", ErrorMessage = "Distance must be between 0.1 and 999.99 km.")]
    public decimal? DistanceKm { get; set; }

    // Filled by the controller for the dropdown, not part of the form
    [ValidateNever]
    public List<VenueViewModel> Venues { get; set; } = new();
}