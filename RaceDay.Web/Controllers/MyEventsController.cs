using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Participant")]
public class MyEventsController : BaseController
{
    private readonly ApiService _api;

    public MyEventsController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<MyEnrolmentViewModel>>("api/users/me/enrolments");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
            TempData["Error"] = result.Message;

        var enrolments = (result.Data ?? new List<MyEnrolmentViewModel>())
            .OrderBy(e => e.EventDate)
            .ToList();

        return View(enrolments);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _api.DeleteAsync($"api/enrolments/{id}");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Your enrolment has been cancelled.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index");
    }
}