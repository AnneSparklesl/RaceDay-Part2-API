using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class VenuesController : BaseController
{
    private readonly ApiService _api;

    public VenuesController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var page = new VenuesPageViewModel();

        var result = await _api.GetAsync<List<VenueViewModel>>("api/venues");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            page.Venues = result.Data ?? new List<VenueViewModel>();
        else
            TempData["Error"] = result.Message;

        return View(page);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VenueViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter a venue name and a city.";
            return RedirectToAction("Index");
        }

        var result = await _api.PostAsync<object>("api/venues",
            new { model.Name, model.Address, model.City });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Venue added.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAsync($"api/venues/{id}");

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Venue deleted.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index");
    }
}