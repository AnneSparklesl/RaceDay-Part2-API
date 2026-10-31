using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public class OrganiserCategoriesController : BaseController
{
    private readonly ApiService _api;

    public OrganiserCategoriesController(ApiService api)
    {
        _api = api;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    // Loads the event and checks it belongs to the logged-in Organiser
    private async Task<(IActionResult? Redirect, EventItemViewModel? Event)> LoadOwnEventAsync(int eventId)
    {
        var found = await _api.GetAsync<EventItemViewModel>($"api/events/{eventId}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return (SessionExpired(), null);

        if (!found.Success || found.Data == null)
        {
            TempData["Error"] = found.Message ?? "We could not find that event.";
            return (RedirectToAction("Index", "OrganiserEvents"), null);
        }

        if (found.Data.OrganiserId != CurrentUserId)
        {
            TempData["Error"] = "You can only manage categories for your own events.";
            return (RedirectToAction("Index", "OrganiserEvents"), null);
        }

        return (null, found.Data);
    }

    // ---------- List and add ----------
    public async Task<IActionResult> Index(int eventId)
    {
        var (redirect, ev) = await LoadOwnEventAsync(eventId);
        if (redirect != null)
            return redirect;

        var categories = await _api.GetAsync<List<CategoryItemViewModel>>($"api/events/{eventId}/categories");
        if (categories.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!categories.Success)
            TempData["Error"] = categories.Message;

        return View(new CategoriesPageViewModel
        {
            Event = ev!,
            Categories = categories.Data ?? new List<CategoryItemViewModel>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter a category name and a distance between 0.1 and 999.99 km.";
            return RedirectToAction("Index", new { eventId = model.EventId });
        }

        var result = await _api.PostAsync<object>("api/categories",
            new { EventId = model.EventId, Name = model.Name, DistanceKm = model.DistanceKm });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Category added.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId = model.EventId });
    }

    // ---------- Edit ----------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var found = await _api.GetAsync<CategoryItemViewModel>($"api/categories/{id}");
        if (found.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!found.Success || found.Data == null)
        {
            TempData["Error"] = found.Message ?? "We could not find that category.";
            return RedirectToAction("Index", "OrganiserEvents");
        }

        var (redirect, _) = await LoadOwnEventAsync(found.Data.EventId);
        if (redirect != null)
            return redirect;

        return View(new CategoryFormViewModel
        {
            Id = found.Data.Id,
            EventId = found.Data.EventId,
            Name = found.Data.Name,
            DistanceKm = found.Data.DistanceKm
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PutAsync<object>($"api/categories/{model.Id}",
            new { Name = model.Name, DistanceKm = model.DistanceKm });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
            return View(model);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction("Index", new { eventId = model.EventId });
    }

    // ---------- Delete ----------
    [HttpPost]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var result = await _api.DeleteAsync($"api/categories/{id}");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (result.Success)
            TempData["Success"] = "Category deleted.";
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index", new { eventId });
    }
}
