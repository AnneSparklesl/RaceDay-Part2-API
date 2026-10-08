using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class VenueRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;
}
