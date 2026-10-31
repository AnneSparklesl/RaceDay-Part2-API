using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class DashboardController : BaseController
{
    private readonly ApiService _api;

    public DashboardController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var eventsResult = await _api.GetAsync<List<EventItemViewModel>>("api/events");
        if (eventsResult.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var enrolmentsResult = await _api.GetAsync<List<EnrolmentItemViewModel>>("api/enrolments");
        if (enrolmentsResult.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!eventsResult.Success)
            TempData["Error"] = eventsResult.Message;
        else if (!enrolmentsResult.Success)
            TempData["Error"] = enrolmentsResult.Message;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var now = DateTime.Now;

        var myEvents = (eventsResult.Data ?? new List<EventItemViewModel>())
            .Where(e => e.OrganiserId == userId)
            .OrderBy(e => e.EventDate)
            .ToList();

        var enrolments = enrolmentsResult.Data ?? new List<EnrolmentItemViewModel>();

        var rows = myEvents.Select(e => new DashboardEventRow
        {
            Id = e.Id,
            Name = e.Name,
            EventType = e.EventType,
            EventDate = e.EventDate,
            Venue = e.Venue,
            DistanceKm = e.DistanceKm,
            EnrolmentCount = enrolments.Count(x => x.EventId == e.Id),
            IsUpcoming = e.EventDate >= now
        }).ToList();

        var model = new DashboardViewModel
        {
            OrganiserName = HttpContext.Session.GetString("UserName") ?? "Organiser",
            TotalEvents = rows.Count,
            UpcomingEvents = rows.Count(r => r.IsUpcoming),
            TotalEnrolments = rows.Sum(r => r.EnrolmentCount),
            NextEvent = rows.FirstOrDefault(r => r.IsUpcoming),
            Events = rows
        };

        return View(model);
    }
}