namespace RaceDay.Web.ViewModels;

// An enrolment as the API sends it to an Organiser
public class EnrolmentItemViewModel
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public string Participant { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Event { get; set; } = string.Empty;
    public DateTime? EnrolmentDate { get; set; }
}
