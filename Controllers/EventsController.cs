using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDBContext _context;
    private static readonly string[] ValidTypes = { "Run", "Walk", "Cycle" };

    public EventsController(ApplicationDBContext context)
    {
        _context = context;
    }

    private static object Shape(Event e) => new
    {
        e.Id,
        e.Name,
        e.Description,
        e.EventDate,
        e.EventType,
        e.DistanceKm,
        e.VenueId,
        e.OrganiserId
    };

    // GET /api/events - any logged-in user
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.EventType,
                e.DistanceKm,
                e.VenueId,
                Venue = e.Venue.Name,
                e.OrganiserId,
                Organiser = e.Organiser.Name
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/{id} - any logged-in user
    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> GetById(int id)
    {
        var found = await _context.Events
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.EventType,
                e.DistanceKm,
                e.VenueId,
                Venue = e.Venue.Name,
                e.OrganiserId,
                Organiser = e.Organiser.Name
            })
            .FirstOrDefaultAsync();

        if (found == null)
            return NotFound(new { message = "Event not found." });

        return Ok(found);
    }

    // POST /api/events - Organisers only
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (!ValidTypes.Contains(request.EventType))
            return BadRequest(new { message = "EventType must be 'Run', 'Walk' or 'Cycle'." });

        if (!await _context.Venues.AnyAsync(v => v.Id == request.VenueId))
            return BadRequest(new { message = "That venue does not exist." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;

        var newEvent = new Event
        {
            OrganiserId = organiserId,
            VenueId = request.VenueId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate,
            EventType = request.EventType,
            DistanceKm = request.DistanceKm
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, Shape(newEvent));
    }

    // PUT /api/events/{id} - Organisers only, and only their own events
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var existing = await _context.Events.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = "Event not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (existing.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only change your own events." });

        if (!ValidTypes.Contains(request.EventType))
            return BadRequest(new { message = "EventType must be 'Run', 'Walk' or 'Cycle'." });

        if (!await _context.Venues.AnyAsync(v => v.Id == request.VenueId))
            return BadRequest(new { message = "That venue does not exist." });

        existing.Name = request.Name.Trim();
        existing.Description = request.Description?.Trim();
        existing.EventDate = request.EventDate;
        existing.VenueId = request.VenueId;
        existing.EventType = request.EventType;
        existing.DistanceKm = request.DistanceKm;

        await _context.SaveChangesAsync();
        return Ok(Shape(existing));
    }

    // DELETE /api/events/{id} - Organisers only, and only their own events
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _context.Events.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = "Event not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (existing.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only delete your own events." });

        if (await _context.Categories.AnyAsync(c => c.EventId == id))
            return Conflict(new { message = "Delete this event's categories first." });

        _context.Events.Remove(existing);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}