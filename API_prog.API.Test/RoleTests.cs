using API_proj.Controllers;
using API_proj.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace API_prog.API.Test;

public class RoleTests
{
    // Builds the context the framework would hand to the attribute
    private static AuthorizationFilterContext BuildContext(int? userId, string? role)
    {
        var session = new TestSession();
        if (userId != null) session.SetInt32("UserId", userId.Value);
        if (role != null) session.SetString("Role", role);

        var httpContext = new DefaultHttpContext { Session = session };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void NotLoggedIn_IsRejectedWith401()
    {
        var attribute = new SessionAuthorizeAttribute("Organiser");
        var context = BuildContext(null, null);

        attribute.OnAuthorization(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void ParticipantOnOrganiserEndpoint_IsRejectedWith403()
    {
        var attribute = new SessionAuthorizeAttribute("Organiser");
        var context = BuildContext(3, "Participant");

        attribute.OnAuthorization(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public void OrganiserOnParticipantEndpoint_IsRejectedWith403()
    {
        var attribute = new SessionAuthorizeAttribute("Participant");
        var context = BuildContext(1, "Organiser");

        attribute.OnAuthorization(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public void CorrectRole_IsAllowedThrough()
    {
        var attribute = new SessionAuthorizeAttribute("Organiser");
        var context = BuildContext(1, "Organiser");

        attribute.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void AnyLoggedInUser_IsAllowedOnOpenEndpoint()
    {
        var attribute = new SessionAuthorizeAttribute();
        var context = BuildContext(3, "Participant");

        attribute.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    // Proves these real endpoints turn a Participant away
    [Theory]
    [InlineData(typeof(EventsController), "Create")]
    [InlineData(typeof(EventsController), "Update")]
    [InlineData(typeof(EventsController), "Delete")]
    [InlineData(typeof(CategoriesController), "Create")]
    [InlineData(typeof(ResultsController), "Create")]
    public void OrganiserEndpoints_RejectAParticipant(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName)!;
        var attribute = (SessionAuthorizeAttribute)method
            .GetCustomAttributes(typeof(SessionAuthorizeAttribute), false).Single();
        var context = BuildContext(3, "Participant");

        attribute.OnAuthorization(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
    }

    // Proves enrolling turns an Organiser away
    [Fact]
    public void EnrolEndpoint_RejectsAnOrganiser()
    {
        var method = typeof(EnrolmentsController).GetMethod("Enrol")!;
        var attribute = (SessionAuthorizeAttribute)method
            .GetCustomAttributes(typeof(SessionAuthorizeAttribute), false).Single();
        var context = BuildContext(1, "Organiser");

        attribute.OnAuthorization(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
    }
}