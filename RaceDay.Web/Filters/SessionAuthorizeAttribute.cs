using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

// Put [SessionAuthorize] on a controller to require login,
// or [SessionAuthorize("Organiser")] to require a specific role.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string? _role;

    public SessionAuthorizeAttribute(string? role = null)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        // Not logged in: send to the login page
        if (session.GetInt32("UserId") == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Logged in but the wrong role: send back to the home page
        if (_role != null && session.GetString("Role") != _role)
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}
