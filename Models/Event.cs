using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// A race event created by an Organiser at a Venue.
[Table("Event")]
public class Event
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organiser_id")]
    public int OrganiserId { get; set; }
    [ForeignKey(nameof(OrganiserId))]
    public User Organiser { get; set; } = null!;

    [Column("venue_id")]
    public int VenueId { get; set; }
    [ForeignKey(nameof(VenueId))]
    public Venue Venue { get; set; } = null!;

    [Required, MaxLength(150)]
    [Column("name", TypeName = "varchar(150)")]
    public string Name { get; set; } = string.Empty;

    [Column("event_date", TypeName = "datetime")]
    public DateTime EventDate { get; set; }

    [MaxLength(500)]
    [Column("description", TypeName = "varchar(500)")]
    public string? Description { get; set; }

    // NEW (not in the Part 1 script): the brief requires "run, walk, or cycle".
    [Required, MaxLength(20)]
    [Column("event_type", TypeName = "varchar(20)")]
    public string EventType { get; set; } = string.Empty;

    [Column("distance_km", TypeName = "decimal(5,2)")]
    public decimal DistanceKm { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}

