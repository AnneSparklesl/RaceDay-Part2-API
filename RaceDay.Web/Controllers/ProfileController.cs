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
    private readonly ImageStorageService _images;

    public ProfileController(ApiService api, ImageStorageService images)
    {
        _api = api;
        _images = images;
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

        var picture = await _api.GetAsync<PictureResponse>("api/users/me/picture");
        result.Data.ProfilePictureUrl = picture.Data?.PictureUrl;

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

    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        var current = await _api.GetAsync<PictureResponse>("api/users/me/picture");
        if (current.StatusCode == HttpStatusCode.Unauthorized)
            return SessionExpired();

        var saved = await _images.SaveAsync(picture, "profiles");
        if (saved.Error != null)
        {
            TempData["Error"] = saved.Error;
            return RedirectToAction("Index");
        }

        var result = await _api.PutAsync<object>("api/users/me/picture", new { Url = saved.Url });
        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            _images.Delete(saved.Url);
            return SessionExpired();
        }

        if (!result.Success)
        {
            _images.Delete(saved.Url);
            TempData["Error"] = result.Message ?? "We could not save your picture.";
            return RedirectToAction("Index");
        }

        // The new picture is saved, so remove the old file
        _images.Delete(current.Data?.PictureUrl);

        TempData["Success"] = "Profile picture updated.";
        return RedirectToAction("Index");
    }

    // Role, member-since and picture are not posted back, so fetch them again for display
    private async Task FillDisplayFieldsAsync(ProfileViewModel model)
    {
        var current = await _api.GetAsync<ProfileViewModel>("api/users/me");
        model.Role = current.Data?.Role;
        model.CreatedAt = current.Data?.CreatedAt;

        var picture = await _api.GetAsync<PictureResponse>("api/users/me/picture");
        model.ProfilePictureUrl = picture.Data?.PictureUrl;
    }
}