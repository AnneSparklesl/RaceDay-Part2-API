using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API_proj.Filters;

// Usage: [SessionAuthorize] = any logged-in user
//        [SessionAuthorize("Organiser")] = Organisers only
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
            return;
        }

        var role = context.HttpContext.Session.GetString("Role");
        if (_roles.Length > 0 && (role == null || !_roles.Contains(role)))
        {
            context.Result = new ObjectResult(new { message = "You do not have permission to do this." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}