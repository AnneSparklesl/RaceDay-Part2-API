using API_proj.data;
using API_proj.DTOs;
using API_proj.Filters;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_proj.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public CategoriesController(ApplicationDBContext context)
    {
        _context = context;
    }

    private static object Shape(Category c) => new
    {
        c.Id,
        c.EventId,
        c.Name,
        c.DistanceKm
    };

    // GET /api/categories - any logged-in user
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.EventId).ThenBy(c => c.DistanceKm)
            .Select(c => new { c.Id, c.EventId, c.Name, c.DistanceKm })
            .ToListAsync();

        return Ok(categories);
    }

    // GET /api/events/{eventId}/categories - any logged-in user
    [HttpGet("/api/events/{eventId}/categories")]
    [SessionAuthorize]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.Id == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.DistanceKm)
            .Select(c => new { c.Id, c.EventId, c.Name, c.DistanceKm })
            .ToListAsync();

        return Ok(categories);
    }

    // GET /api/categories/{id} - any logged-in user
    [HttpGet("{id}")]
    [SessionAuthorize]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        return Ok(Shape(category));
    }

    // POST /api/categories - Organisers only, and only for their own events
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(CategoryCreateRequest request)
    {
        var parentEvent = await _context.Events.FindAsync(request.EventId);
        if (parentEvent == null)
            return NotFound(new { message = "Event not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (parentEvent.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only add categories to your own events." });

        var category = new Category
        {
            EventId = request.EventId,
            Name = request.Name.Trim(),
            DistanceKm = request.DistanceKm
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, Shape(category));
    }

    // PUT /api/categories/{id} - Organisers only, and only for their own events
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, CategoryUpdateRequest request)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (category.Event.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only change categories of your own events." });

        category.Name = request.Name.Trim();
        category.DistanceKm = request.DistanceKm;

        await _context.SaveChangesAsync();
        return Ok(Shape(category));
    }

    // DELETE /api/categories/{id} - Organisers only, and only for their own events
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (category.Event.OrganiserId != organiserId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only delete categories of your own events." });

        if (await _context.Enrolments.AnyAsync(e => e.CategoryId == id))
            return Conflict(new { message = "This category has enrolments and cannot be deleted." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
