using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api)
    {
        _api = api;
    }

    // Organisers start on their dashboard, Participants on the home page
    private IActionResult RedirectToStartPage()
    {
        if (HttpContext.Session.GetString("Role") == "Organiser")
            return RedirectToAction("Index", "Dashboard");

        return RedirectToAction("Index", "Home");
    }

    // ---------- Login ----------
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
            return RedirectToStartPage();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<LoginResponse>("api/auth/login",
            new { model.Email, model.Password });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not log you in.");
            return View(model);
        }

        // Remember who this is across all pages
        HttpContext.Session.SetInt32("UserId", result.Data.Id);
        HttpContext.Session.SetString("UserName", result.Data.Name);
        HttpContext.Session.SetString("Role", result.Data.Role);

        return RedirectToStartPage();
    }

    // ---------- Register ----------
    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
            return RedirectToStartPage();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<object>("api/auth/register",
            new { model.Name, model.Email, model.Password, model.Role });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "We could not create your account.");
            return View(model);
        }

        TempData["Success"] = "Your account has been created. Please log in.";
        return RedirectToAction("Login");
    }

    // ---------- Logout ----------
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.PostAsync<object>("api/auth/logout");
        _api.ForgetApiCookie();
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }
}