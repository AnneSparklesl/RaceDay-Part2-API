using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class ResultCreateRequest
{
    public int EnrolmentId { get; set; }

    // Written as hours:minutes:seconds, e.g. "00:52:30"
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Position { get; set; }
}

public class ResultUpdateRequest
{
    // Written as hours:minutes:seconds, e.g. "00:52:30"
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Position { get; set; }
}
