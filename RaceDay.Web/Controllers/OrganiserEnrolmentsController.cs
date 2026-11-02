using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class OrganiserEnrolmentsController : BaseController
{
    private readonly ApiService _api;

    public OrganiserEnrolmentsController(ApiService api)
    {
        _api = api;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    public async Task<IActionResult> Index(int? eventId)
    {
        var page = new EnrolmentsPageViewModel();

        var eventsResult = await _api.GetAsync<List<EventItemViewModel>>("api/events");
        if (eventsResult.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!eventsResult.Success)
        {
            TempData["Error"] = eventsResult.Message;
            return View(page);
        }

        page.Events = (eventsResult.Data ?? new List<EventItemViewModel>())
            .Where(e => e.OrganiserId == CurrentUserId)
            .OrderBy(e => e.EventDate)
            .ToList();

        page.SelectedEvent = page.Events.FirstOrDefault(e => e.Id == eventId) ?? page.Events.FirstOrDefault();
        if (page.SelectedEvent == null)
            return View(page);

        var enrolments = await _api.GetAsync<List<EnrolmentItemViewModel>>("api/enrolments");
        if (enrolments.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!enrolments.Success)
        {
            TempData["Error"] = enrolments.Message;
            return View(page);
        }

        page.Enrolments = (enrolments.Data ?? new List<EnrolmentItemViewModel>())
            .Where(e => e.EventId == page.SelectedEvent.Id)
            .OrderBy(e => e.Participant)
            .ToList();

        return View(page);
    }

    [HttpPost]
    public async Task<IActionResult> SetStatus(int id, string status, int eventId)
    {
        var result = await _api.PutAsync<object>($"api/enrolments/{id}/status", new { Status = status });
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = status == "Confirmed" ? "Enrolment confirmed." : "Enrolment set back to pending.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id, int eventId)
    {
        var result = await _api.DeleteAsync($"api/enrolments/{id}");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Enrolment removed.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId });
    }
}
