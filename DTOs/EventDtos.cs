using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class EventRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    public int VenueId { get; set; }

    // "Run", "Walk" or "Cycle"
    [Required]
    public string EventType { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }
}
