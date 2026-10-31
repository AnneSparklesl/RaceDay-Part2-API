using System.Globalization;
using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public ResultsController(ApplicationDBContext context)
    {
        _context = context;
    }

    private static bool TryParseTime(string text, out TimeSpan time)
    {
        return TimeSpan.TryParseExact(text, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out time)
               && time > TimeSpan.Zero;
    }

    private const string BadTimeMessage = "finishTime must look like 00:52:30 (hours:minutes:seconds).";

    // GET /api/results - any logged-in user
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll()
    {
        var results = await _context.Results
            .OrderBy(r => r.Enrolment.Category.EventId).ThenBy(r => r.Position)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                Participant = r.Enrolment.Participant.Name,
                Category = r.Enrolment.Category.Name,
                Event = r.Enrolment.Category.Event.Name,
                r.FinishTime,
                r.Position
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/results/{id} - any logged-in user
    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _context.Results
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                Participant = r.Enrolment.Participant.Name,
                Category = r.Enrolment.Category.Name,
                Event = r.Enrolment.Category.Event.Name,
                r.FinishTime,
                r.Position
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return NotFound(new { message = "Result not found." });

        return Ok(result);
    }

    // GET /api/users/me/results - a Participant sees their own results
    // Includes the event date and how many people finished the event (e.g. "47th out of 312")
    [HttpGet("/api/users/me/results")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> GetMine()
    {
        var participantId = HttpContext.Session.GetInt32("UserId")!.Value;

        var results = await _context.Results
            .Where(r => r.Enrolment.ParticipantId == participantId)
            .OrderByDescending(r => r.Enrolment.Category.Event.EventDate)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                EventId = r.Enrolment.Category.EventId,
                Category = r.Enrolment.Category.Name,
                Event = r.Enrolment.Category.Event.Name,
                EventDate = r.Enrolment.Category.Event.EventDate,
                r.FinishTime,
                r.Position,
                TotalFinishers = _context.Results.Count(x =>
                    x.Enrolment.Category.EventId == r.Enrolment.Category.EventId)
            })
            .ToListAsync();

        return Ok(results);
    }

    // POST /api/results - Organisers only, for enrolments in their own events
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(ResultCreateRequest request)
    {
        if (!TryParseTime(request.FinishTime, out var finishTime))
            return BadRequest(new { message = BadTimeMessage });

        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(e => e.Id == request.EnrolmentId);
        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (enrolment.Category.Event.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only record results for your own events." });

        if (await _context.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result
        {
            EnrolmentId = request.EnrolmentId,
            FinishTime = finishTime,
            Position = request.Position
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            result.Id,
            result.EnrolmentId,
            result.FinishTime,
            result.Position
        });
    }

    // PUT /api/results/{id} - Organisers only, for their own events
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, ResultUpdateRequest request)
    {
        if (!TryParseTime(request.FinishTime, out var finishTime))
            return BadRequest(new { message = BadTimeMessage });

        var result = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (result == null)
            return NotFound(new { message = "Result not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (result.Enrolment.Category.Event.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only change results for your own events." });

        result.FinishTime = finishTime;
        result.Position = request.Position;

        await _context.SaveChangesAsync();

        return Ok(new { result.Id, result.EnrolmentId, result.FinishTime, result.Position });
    }

    // GET /api/events/{eventId}/results - leaderboard for one event
    [HttpGet("/api/events/{eventId}/results")]
    [SessionAuthorize]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.Id == eventId))
            return NotFound(new { message = "Event not found." });

        var results = await _context.Results
            .Where(r => r.Enrolment.Category.EventId == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new
            {
                r.Id,
                r.EnrolmentId,
                Participant = r.Enrolment.Participant.Name,
                Category = r.Enrolment.Category.Name,
                r.FinishTime,
                r.Position
            })
            .ToListAsync();

        return Ok(results);
    }

    // DELETE /api/results/{id} - Organisers only, for their own events
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (result == null)
            return NotFound(new { message = "Result not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (result.Enrolment.Category.Event.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only delete results for your own events." });

        _context.Results.Remove(result);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}