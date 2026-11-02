using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ApiService _api;

    public HomeController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? search, string? type, int? venueId)
    {
        var session = HttpContext.Session;

        // Organisers work from their dashboard
        if (session.GetString("Role") == "Organiser")
            return RedirectToAction("Index", "Dashboard");

        var page = new HomePageViewModel
        {
            LoggedIn = session.GetInt32("UserId") != null,
            Search = search,
            Type = type,
            VenueId = venueId
        };

        // Visitors see the landing hero
        if (!page.LoggedIn)
            return View(page);

        var result = await _api.GetAsync<List<EventItemViewModel>>("api/events");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(page);
        }

        // Only upcoming events, soonest first
        var upcoming = (result.Data ?? new List<EventItemViewModel>())
            .Where(e => e.EventDate >= DateTime.Now)
            .OrderBy(e => e.EventDate)
            .ToList();

        page.Venues = upcoming
            .GroupBy(e => e.VenueId)
            .Select(g => new VenueOption { Id = g.Key, Name = g.First().Venue })
            .OrderBy(v => v.Name)
            .ToList();

        var filtered = upcoming.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            filtered = filtered.Where(e =>
                e.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (e.Description ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
            filtered = filtered.Where(e => e.EventType == type);

        if (venueId.HasValue)
            filtered = filtered.Where(e => e.VenueId == venueId.Value);

        page.Events = filtered.ToList();
        return View(page);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}