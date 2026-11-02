using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class ResultsController : BaseController
{
    private readonly ApiService _api;

    public ResultsController(ApiService api)
    {
        _api = api;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    public async Task<IActionResult> Index(int? eventId)
    {
        var page = new ResultsPageViewModel();

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
            .OrderByDescending(e => e.EventDate)
            .ToList();

        page.SelectedEvent = page.Events.FirstOrDefault(e => e.Id == eventId) ?? page.Events.FirstOrDefault();
        if (page.SelectedEvent == null)
            return View(page);

        var enrolments = await _api.GetAsync<List<EnrolmentItemViewModel>>("api/enrolments");
        if (enrolments.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var results = await _api.GetAsync<List<ResultItemViewModel>>($"api/events/{page.SelectedEvent.Id}/results");
        if (results.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!enrolments.Success)
        {
            TempData["Error"] = enrolments.Message;
            return View(page);
        }

        var resultList = results.Data ?? new List<ResultItemViewModel>();

        page.Rows = (enrolments.Data ?? new List<EnrolmentItemViewModel>())
            .Where(e => e.EventId == page.SelectedEvent.Id)
            .Select(e =>
            {
                var result = resultList.FirstOrDefault(r => r.EnrolmentId == e.Id);
                return new ResultRowViewModel
                {
                    EnrolmentId = e.Id,
                    Participant = e.Participant,
                    Category = e.Category,
                    Status = e.Status,
                    ResultId = result?.Id ?? 0,
                    FinishTime = result?.FinishTime ?? string.Empty,
                    Position = result?.Position
                };
            })
            .OrderBy(r => r.Position ?? int.MaxValue)
            .ThenBy(r => r.Participant)
            .ToList();

        return View(page);
    }

    [HttpPost]
    public async Task<IActionResult> Save(int enrolmentId, int resultId, int eventId, string? finishTime, int? position)
    {
        var time = (finishTime ?? string.Empty).Trim();

        if (!Regex.IsMatch(time, @"^\d{2}:\d{2}:\d{2}$") || position == null || position < 1)
        {
            TempData["Error"] = "Enter the finish time as HH:MM:SS (for example 00:52:30) and a finishing position of 1 or more.";
            return RedirectToAction("Index", new { eventId });
        }

        // Results can only be entered once the event has taken place
        var found = await _api.GetAsync<EventItemViewModel>($"api/events/{eventId}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (found.Success && found.Data != null && found.Data.EventDate > DateTime.Now)
        {
            TempData["Error"] = "Results can only be entered after the event has taken place.";
            return RedirectToAction("Index", new { eventId });
        }

        ApiResult<object> result = resultId > 0
            ? await _api.PutAsync<object>($"api/results/{resultId}", new { FinishTime = time, Position = position })
            : await _api.PostAsync<object>("api/results", new { EnrolmentId = enrolmentId, FinishTime = time, Position = position });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = resultId > 0 ? "Result updated." : "Result saved and published.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var result = await _api.DeleteAsync($"api/results/{id}");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Result deleted.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId });
    }
}