using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API_proj.Controllers;

[ApiController]
[Route("api/users/me/picture")]
public class ProfilePictureController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public ProfilePictureController(ApplicationDBContext context)
    {
        _context = context;
    }

    // GET /api/users/me/picture - the logged-in user's profile picture path
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> Get()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(new { pictureUrl = user.ProfilePictureUrl });
    }

    // PUT /api/users/me/picture - save the path of the picture the web app stored
    [HttpPut]
    [SessionAuthorize]
    public async Task<IActionResult> Set(ImageUrlRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        if (!request.Url.StartsWith("/uploads/profiles/"))
            return BadRequest(new { message = "That is not a valid profile picture path." });

        user.ProfilePictureUrl = request.Url;
        await _context.SaveChangesAsync();

        return Ok(new { pictureUrl = user.ProfilePictureUrl });
    }
}