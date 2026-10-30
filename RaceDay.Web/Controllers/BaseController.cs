using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

// Shared helpers for every controller that calls the API
public abstract class BaseController : Controller
{
    // The API forgot this user (for example it restarted): send them to log in again
    protected IActionResult SessionExpired()
    {
        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Account");
    }
}