using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// Physical location where events are hosted.
[Table("Venue")]
public class Venue
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("name", TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("address", TypeName = "varchar(200)")]
    public string? Address { get; set; }

    [Required, MaxLength(100)]
    [Column("city", TypeName = "varchar(100)")]
    public string City { get; set; } = string.Empty;
}

