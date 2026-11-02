using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Participant")]
public class ProfileController : BaseController
{
    private readonly ApiService _api;

    public ProfileController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<ProfileViewModel>("api/users/me");
        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Message ?? "We could not load your profile.";
            return RedirectToAction("Index", "Home");
        }

        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillDisplayFieldsAsync(model);
            return View(model);
        }

        var result = await _api.PutAsync<object>("api/users/me",
            new { Name = model.Name, Email = model.Email });

        if (result.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not save your changes.");
            await FillDisplayFieldsAsync(model);
            return View(model);
        }

        // Keep the name in the navbar up to date
        HttpContext.Session.SetString("UserName", model.Name.Trim());

        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction("Index");
    }

    // Role and member-since are not posted back, so fetch them again for display
    private async Task FillDisplayFieldsAsync(ProfileViewModel model)
    {
        var current = await _api.GetAsync<ProfileViewModel>("api/users/me");
        model.Role = current.Data?.Role;
        model.CreatedAt = current.Data?.CreatedAt;
    }
}