namespace RaceDay.Web.ViewModels;

public class VenueOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// The home page (landing for visitors, event browser for logged-in Participants)
public class HomePageViewModel
{
    public bool LoggedIn { get; set; }
    public List<EventItemViewModel> Events { get; set; } = new();
    public List<VenueOption> Venues { get; set; } = new();

    public string? Search { get; set; }
    public string? Type { get; set; }
    public int? VenueId { get; set; }

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(Type) || VenueId.HasValue;
}

// One of the Participant's own enrolments
public class MyEnrolmentViewModel
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Event { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? EnrolmentDate { get; set; }
    public string Status { get; set; } = "Pending";
}

// The event detail page
public class EventDetailsViewModel
{
    public EventItemViewModel Event { get; set; } = new();
    public List<CategoryItemViewModel> Categories { get; set; } = new();
    public HashSet<int> EnrolledCategoryIds { get; set; } = new();
    public bool IsParticipant { get; set; }
    public bool IsUpcoming => Event.EventDate >= DateTime.Now;
}
