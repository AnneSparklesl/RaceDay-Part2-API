using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public VenuesController(ApplicationDBContext context)
    {
        _context = context;
    }

    // GET /api/venues - any logged-in user can view venues
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Venues.ToListAsync());
    }

    // GET /api/venues/{id} - any logged-in user
    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> GetById(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
            return NotFound(new { message = "Venue not found." });

        return Ok(venue);
    }

    // POST /api/venues - Organisers only
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(VenueRequest request)
    {
        var venue = new Venue
        {
            Name = request.Name.Trim(),
            Address = request.Address?.Trim(),
            City = request.City.Trim()
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, venue);
    }

    // PUT /api/venues/{id} - Organisers only
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, VenueRequest request)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
            return NotFound(new { message = "Venue not found." });

        venue.Name = request.Name.Trim();
        venue.Address = request.Address?.Trim();
        venue.City = request.City.Trim();

        await _context.SaveChangesAsync();
        return Ok(venue);
    }

    // DELETE /api/venues/{id} - Organisers only
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
            return NotFound(new { message = "Venue not found." });

        if (await _context.Events.AnyAsync(e => e.VenueId == id))
            return Conflict(new { message = "This venue is used by an event and cannot be deleted." });

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
