namespace RaceDay.Web.ViewModels;

public class DashboardEventRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int EnrolmentCount { get; set; }
    public bool IsUpcoming { get; set; }
}

public class DashboardViewModel
{
    public string OrganiserName { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public DashboardEventRow? NextEvent { get; set; }
    public List<DashboardEventRow> Events { get; set; } = new();
}