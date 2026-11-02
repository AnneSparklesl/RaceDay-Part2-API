namespace RaceDay.Web.ViewModels;

// A result as the API sends it
public class ResultItemViewModel
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public string Participant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string FinishTime { get; set; } = string.Empty;
    public int Position { get; set; }
}

// One row of the results entry table: an enrolment plus its result, if any
public class ResultRowViewModel
{
    public int EnrolmentId { get; set; }
    public string Participant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ResultId { get; set; }
    public string FinishTime { get; set; } = string.Empty;
    public int? Position { get; set; }
}

public class ResultsPageViewModel
{
    public List<EventItemViewModel> Events { get; set; } = new();
    public EventItemViewModel? SelectedEvent { get; set; }
    public List<ResultRowViewModel> Rows { get; set; } = new();

    public bool EventHasConcluded => SelectedEvent != null && SelectedEvent.EventDate < DateTime.Now;
}