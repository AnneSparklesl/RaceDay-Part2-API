using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize]
public class EventsController : BaseController
{
    private readonly ApiService _api;

    public EventsController(ApiService api)
    {
        _api = api;
    }

    // ---------- Full event information and categories ----------
    public async Task<IActionResult> Details(int id)
    {
        var found = await _api.GetAsync<EventItemViewModel>($"api/events/{id}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!found.Success || found.Data == null)
        {
            TempData["Error"] = found.Message ?? "We could not find that event.";
            return RedirectToAction("Index", "Home");
        }

        var categories = await _api.GetAsync<List<CategoryItemViewModel>>($"api/events/{id}/categories");
        if (categories.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var model = new EventDetailsViewModel
        {
            Event = found.Data,
            Categories = categories.Data ?? new List<CategoryItemViewModel>(),
            IsParticipant = HttpContext.Session.GetString("Role") == "Participant"
        };

        // Which categories has this Participant already entered?
        if (model.IsParticipant)
        {
            var mine = await _api.GetAsync<List<MyEnrolmentViewModel>>("api/users/me/enrolments");
            if (mine.Success && mine.Data != null)
                model.EnrolledCategoryIds = mine.Data.Select(m => m.CategoryId).ToHashSet();
        }

        return View(model);
    }

    // ---------- Enrol in a category ----------
    [HttpPost]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(int categoryId, int eventId)
    {
        // No new entries once the event has taken place
        var found = await _api.GetAsync<EventItemViewModel>($"api/events/{eventId}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (found.Success && found.Data != null && found.Data.EventDate < DateTime.Now)
        {
            TempData["Error"] = "This event has already taken place.";
            return RedirectToAction("Details", new { id = eventId });
        }

        var result = await _api.PostAsync<object>("api/enrolments", new { CategoryId = categoryId });
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Details", new { id = eventId });
        }

        TempData["Success"] = "You are enrolled! The organiser will confirm your entry.";
        return RedirectToAction("Index", "MyEvents");
    }
}