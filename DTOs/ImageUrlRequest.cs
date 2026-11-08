using System.ComponentModel.DataAnnotations;

namespace API_proj.DTOs;

public class ImageUrlRequest
{
    [Required, MaxLength(300)]
    public string Url { get; set; } = string.Empty;
}