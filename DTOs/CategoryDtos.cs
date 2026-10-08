using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class CategoryCreateRequest
{
    public int EventId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000)]
    public decimal DistanceKm { get; set; }
}

public class CategoryUpdateRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000)]
    public decimal DistanceKm { get; set; }
}