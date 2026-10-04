using API_proj.data;
using API_proj.DTOs;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public AuthController(ApplicationDBContext context)
    {
        _context = context;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (request.Role != "Organiser" && request.Role != "Participant")
            return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

        var email = request.Email.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "That email is already registered." });

        var newUser = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { id = newUser.Id, email = newUser.Email });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        // Server-side session: remember who this is for all later requests
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new { id = user.Id, name = user.Name, email = user.Email, role = user.Role });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }
}
