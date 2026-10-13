using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

// Every endpoint here needs a logged-in user (either role)
[ApiController]
[Route("api/users/me")]
[SessionAuthorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public UsersController(ApplicationDBContext context)
    {
        _context = context;
    }

    // GET /api/users/me - view my own profile
    [HttpGet]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var profile = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Name, u.Email, u.Role, u.CreatedAt })
            .FirstOrDefaultAsync();

        if (profile == null)
            return NotFound(new { message = "User not found." });

        return Ok(profile);
    }

    // PUT /api/users/me - update my own name and email
    [HttpPut]
    public async Task<IActionResult> UpdateMe(ProfileUpdateRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var me = await _context.Users.FindAsync(userId);
        if (me == null)
            return NotFound(new { message = "User not found." });

        var email = request.Email.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId))
            return Conflict(new { message = "That email is already used by another account." });

        me.Name = request.Name.Trim();
        me.Email = email;

        await _context.SaveChangesAsync();

        return Ok(new { me.Id, me.Name, me.Email, me.Role });
    }

    // DELETE /api/users/me - delete my own account
    [HttpDelete]
    public async Task<IActionResult> DeleteMe()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var me = await _context.Users.FindAsync(userId);
        if (me == null)
            return NotFound(new { message = "User not found." });

        if (await _context.Events.AnyAsync(e => e.OrganiserId == userId))
            return Conflict(new { message = "You still have events. Delete them before deleting your account." });

        if (await _context.Enrolments.AnyAsync(e => e.ParticipantId == userId))
            return Conflict(new { message = "You still have enrolments. Cancel them before deleting your account." });

        _context.Users.Remove(me);
        await _context.SaveChangesAsync();

        HttpContext.Session.Clear();
        return NoContent();
    }
}