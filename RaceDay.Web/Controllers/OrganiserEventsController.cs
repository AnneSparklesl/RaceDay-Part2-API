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
    private readonly ImageStorageService _images;

    public OrganiserEventsController(ApiService api, ImageStorageService images)
    {
        _api = api;
        _images = images;
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

        // Save the banner file first, so a bad image stops us before the event is created
        string? bannerUrl = null;
        if (model.Banner != null && model.Banner.Length > 0)
        {
            var saved = await _images.SaveAsync(model.Banner, "banners");
            if (saved.Error != null)
            {
                ModelState.AddModelError(nameof(EventFormViewModel.Banner), saved.Error);
                return View("Form", model);
            }
            bannerUrl = saved.Url;
        }

        var result = await _api.PostAsync<EventItemViewModel>("api/events", ToRequest(model));
        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            _images.Delete(bannerUrl);
            return SessionExpired();
        }

        if (!result.Success || result.Data == null)
        {
            _images.Delete(bannerUrl);
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not create the event.");
            return View("Form", model);
        }

        if (bannerUrl != null)
        {
            var banner = await _api.PutAsync<object>($"api/events/{result.Data.Id}/banner", new { Url = bannerUrl });
            if (!banner.Success)
            {
                _images.Delete(bannerUrl);
                TempData["Error"] = "The event was created, but the banner could not be saved. Edit the event to try again.";
                return RedirectToAction("Index");
            }
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
            BannerUrl = found.Data.BannerUrl,
            Venues = venues.Data ?? new List<VenueViewModel>()
        };

        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        // Load the saved event again: we need its real current banner, and to confirm ownership
        var current = await _api.GetAsync<EventItemViewModel>($"api/events/{model.Id}");
        if (current.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!current.Success || current.Data == null)
        {
            TempData["Error"] = current.Message ?? "We could not find that event.";
            return RedirectToAction("Index");
        }

        if (current.Data.OrganiserId != CurrentUserId)
        {
            TempData["Error"] = "You can only edit your own events.";
            return RedirectToAction("Index");
        }

        model.BannerUrl = current.Data.BannerUrl;

        var venues = await GetVenuesAsync();
        if (venues.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();
        model.Venues = venues.Data ?? new List<VenueViewModel>();

        if (!ModelState.IsValid)
            return View("Form", model);

        string? newBannerUrl = null;
        if (model.Banner != null && model.Banner.Length > 0)
        {
            var saved = await _images.SaveAsync(model.Banner, "banners");
            if (saved.Error != null)
            {
                ModelState.AddModelError(nameof(EventFormViewModel.Banner), saved.Error);
                return View("Form", model);
            }
            newBannerUrl = saved.Url;
        }

        var result = await _api.PutAsync<object>($"api/events/{model.Id}", ToRequest(model));
        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            _images.Delete(newBannerUrl);
            return SessionExpired();
        }

        if (!result.Success)
        {
            _images.Delete(newBannerUrl);
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
            return View("Form", model);
        }

        if (newBannerUrl != null)
        {
            var banner = await _api.PutAsync<object>($"api/events/{model.Id}/banner", new { Url = newBannerUrl });
            if (banner.Success)
            {
                // The new banner is saved, so the old file is no longer needed
                _images.Delete(current.Data.BannerUrl);
            }
            else
            {
                _images.Delete(newBannerUrl);
                TempData["Error"] = "Your changes were saved, but the new banner could not be saved. Please try again.";
                return RedirectToAction("Index");
            }
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
