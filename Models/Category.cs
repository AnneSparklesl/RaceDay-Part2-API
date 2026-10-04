
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// A distance/age category within an Event 
[Table("Category")]
public class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    [Required, MaxLength(50)]
    [Column("name", TypeName = "varchar(50)")]
    public string Name { get; set; } = string.Empty;

    [Column("distance_km", TypeName = "decimal(5,2)")]
    public decimal DistanceKm { get; set; }
}

