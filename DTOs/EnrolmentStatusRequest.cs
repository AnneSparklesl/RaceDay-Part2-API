using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class EnrolmentStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}