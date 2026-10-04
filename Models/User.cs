using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// Stores both Organisers and Participants, distinguished by Role.
[Table("User")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("name", TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("email", TypeName = "varchar(150)")]
    public string Email { get; set; } = string.Empty;

    // Never store the plain password - this holds the BCrypt hash.
    [Required, MaxLength(255)]
    [Column("password_hash", TypeName = "varchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    // "Organiser" or "Participant"
    [Required, MaxLength(20)]
    [Column("role", TypeName = "varchar(20)")]
    public string Role { get; set; } = string.Empty;

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}

