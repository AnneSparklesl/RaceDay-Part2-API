using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Participant")]
public class MyResultsController : BaseController
{
    private readonly ApiService _api;

    public MyResultsController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<MyResultViewModel>>("api/users/me/results");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
            TempData["Error"] = result.Message;

        var results = (result.Data ?? new List<MyResultViewModel>())
            .OrderByDescending(r => r.EventDate)
            .ToList();

        return View(results);
    }
}
