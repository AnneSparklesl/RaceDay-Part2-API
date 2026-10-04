using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models;

// Finish time and position for an Enrolment (one-to-one).
[Table("Result")]
public class Result
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("enrolment_id")]
    public int EnrolmentId { get; set; }
    [ForeignKey(nameof(EnrolmentId))]
    public Enrolment Enrolment { get; set; } = null!;

    [Column("finish_time", TypeName = "time")]
    public TimeSpan FinishTime { get; set; }

    [Column("position")]
    public int Position { get; set; }
}
