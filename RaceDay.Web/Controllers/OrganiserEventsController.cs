using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class OrganiserEventsController : BaseController
{
    private readonly ApiService _api;

    public OrganiserEventsController(ApiService api)
    {
        _api = api;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    private Task<ApiResult<List<VenueViewModel>>> GetVenuesAsync() =>
        _api.GetAsync<List<VenueViewModel>>("api/venues");

    // The API expects exactly these fields
    private static object ToRequest(EventFormViewModel m) => new
    {
        Name = m.Name,
        Description = m.Description,
        EventDate = m.EventDate,
        VenueId = m.VenueId,
        EventType = m.EventType,
        DistanceKm = m.DistanceKm
    };

    // ---------- List my events ----------
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<EventItemViewModel>>("api/events");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var myEvents = new List<EventItemViewModel>();
        if (result.Success)
        {
            myEvents = (result.Data ?? new List<EventItemViewModel>())
                .Where(e => e.OrganiserId == CurrentUserId)
                .OrderBy(e => e.EventDate)
                .ToList();
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return View(myEvents);
    }

    // ---------- Create ----------
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var venues = await GetVenuesAsync();
        if (venues.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!venues.Success)
        {
            TempData["Error"] = venues.Message;
            return RedirectToAction("Index");
        }

        if (venues.Data == null || !venues.Data.Any())
        {
            TempData["Error"] = "Please add a venue before creating an event.";
            return RedirectToAction("Index", "Venues");
        }

        return View("Form", new EventFormViewModel { Venues = venues.Data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        var venues = await GetVenuesAsync();
        if (venues.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();
        model.Venues = venues.Data ?? new List<VenueViewModel>();

        if (!ModelState.IsValid)
            return View("Form", model);

        var result = await _api.PostAsync<object>("api/events", ToRequest(model));
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not create the event.");
            return View("Form", model);
        }

        TempData["Success"] = "Event created.";
        return RedirectToAction("Index");
    }

    // ---------- Edit ----------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var found = await _api.GetAsync<EventItemViewModel>($"api/events/{id}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!found.Success || found.Data == null)
        {
            TempData["Error"] = found.Message ?? "We could not find that event.";
            return RedirectToAction("Index");
        }

        if (found.Data.OrganiserId != CurrentUserId)
        {
            TempData["Error"] = "You can only edit your own events.";
            return RedirectToAction("Index");
        }

        var venues = await GetVenuesAsync();
        if (venues.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var form = new EventFormViewModel
        {
            Id = found.Data.Id,
            Name = found.Data.Name,
            Description = found.Data.Description ?? string.Empty,
            EventDate = found.Data.EventDate,
            VenueId = found.Data.VenueId,
            EventType = found.Data.EventType,
            DistanceKm = found.Data.DistanceKm,
            Venues = venues.Data ?? new List<VenueViewModel>()
        };

        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        var venues = await GetVenuesAsync();
        if (venues.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();
        model.Venues = venues.Data ?? new List<VenueViewModel>();

        if (!ModelState.IsValid)
            return View("Form", model);

        var result = await _api.PutAsync<object>($"api/events/{model.Id}", ToRequest(model));
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
            return View("Form", model);
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction("Index");
    }

    // ---------- Delete ----------
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAsync($"api/events/{id}");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Event deleted.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index");
    }
}
