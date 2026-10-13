using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public EnrolmentsController(ApplicationDBContext context)
    {
        _context = context;
    }

    // GET /api/enrolments - Organisers see the enrolments for events they organise
    [HttpGet]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> GetForOrganiser()
    {
        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;

        var enrolments = await _context.Enrolments
            .Where(e => e.Category.Event.OrganiserId == organiserId)
            .OrderBy(e => e.Category.EventId)
            .Select(e => new
            {
                e.Id,
                e.ParticipantId,
                Participant = e.Participant.Name,
                e.CategoryId,
                Category = e.Category.Name,
                EventId = e.Category.EventId,
                Event = e.Category.Event.Name,
                e.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET /api/users/me/enrolments - a Participant sees their own enrolments
    [HttpGet("/api/users/me/enrolments")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> GetMine()
    {
        var participantId = HttpContext.Session.GetInt32("UserId")!.Value;

        var enrolments = await _context.Enrolments
            .Where(e => e.ParticipantId == participantId)
            .OrderBy(e => e.Category.EventId)
            .Select(e => new
            {
                e.Id,
                e.CategoryId,
                Category = e.Category.Name,
                EventId = e.Category.EventId,
                Event = e.Category.Event.Name,
                e.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // POST /api/enrolments - a Participant enters a category
    [HttpPost]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(EnrolmentRequest request)
    {
        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return NotFound(new { message = "Category not found." });

        var participantId = HttpContext.Session.GetInt32("UserId")!.Value;

        if (await _context.Enrolments.AnyAsync(e =>
                e.ParticipantId == participantId && e.CategoryId == request.CategoryId))
            return Conflict(new { message = "You are already enrolled in this category." });

        var enrolment = new Enrolment
        {
            ParticipantId = participantId,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.Now
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            enrolment.Id,
            enrolment.ParticipantId,
            enrolment.CategoryId,
            enrolment.EnrolmentDate
        });
    }

    // DELETE /api/enrolments/{id} - owner can cancel their own; Organisers can cancel any
    [HttpDelete("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var role = HttpContext.Session.GetString("Role");

        if (role != "Organiser" && enrolment.ParticipantId != userId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only cancel your own enrolments." });

        if (await _context.Results.AnyAsync(r => r.EnrolmentId == id))
            return Conflict(new { message = "This enrolment has a result and cannot be cancelled." });

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
