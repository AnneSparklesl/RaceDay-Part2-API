using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// Links a Participant to a Category they have entered.
[Table("Enrolment")]
public class Enrolment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("participant_id")]
    public int ParticipantId { get; set; }
    [ForeignKey(nameof(ParticipantId))]
    public User Participant { get; set; } = null!;

    [Column("category_id")]
    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [Column("enrolment_date", TypeName = "datetime")]
    public DateTime? EnrolmentDate { get; set; }

    // "Pending" when a Participant enrols, "Confirmed" once the Organiser confirms
    [Required, MaxLength(20)]
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; } = "Pending";

    public Result? Result { get; set; }
}
