using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    // Display only (nullable so the form does not require them)
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

// What the API returns for GET api/users/me/picture
public class PictureResponse
{
    public string? PictureUrl { get; set; }
}