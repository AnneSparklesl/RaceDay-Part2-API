namespace RaceDay.Web.ViewModels;

// One finished race from the Participant's history
public class MyResultViewModel
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string FinishTime { get; set; } = string.Empty;
    public int Position { get; set; }
    public int TotalFinishers { get; set; }

    // e.g. "47th out of 312 finishers"
    public string PositionText
    {
        get
        {
            // The total can never be smaller than the position itself
            var total = Math.Max(TotalFinishers, Position);
            return $"{Ordinal(Position)} out of {total} {(total == 1 ? "finisher" : "finishers")}";
        }
    }

    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo >= 11 && lastTwo <= 13)
            return $"{number}th";

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }
}