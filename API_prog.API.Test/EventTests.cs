using API_proj.Controllers;
using API_proj.DTOs;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_prog.API.Test;

public class EventTests
{
    private static EventRequest ValidRequest(string name = "New Event", string type = "Walk", int venueId = 1) => new()
    {
        Name = name,
        Description = "Test event",
        EventDate = new DateTime(2026, 12, 1),
        VenueId = venueId,
        EventType = type,
        DistanceKm = 5
    };

    [Fact]
    public async Task Create_ValidEvent_ByOrganiser_Returns201AndSavesOrganiserId()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Create(ValidRequest());

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);

        var saved = await context.Events.SingleAsync(e => e.Name == "New Event");
        Assert.Equal(1, saved.OrganiserId);
        Assert.Equal("Walk", saved.EventType);
    }

    [Fact]
    public async Task Create_InvalidEventType_Returns400AndSavesNothing()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Create(ValidRequest(type: "Swim"));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(1, await context.Events.CountAsync());
    }

    [Fact]
    public async Task Create_UnknownVenue_Returns400()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Create(ValidRequest(venueId: 99));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_OwnEvent_Returns200AndChangesName()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Update(1, ValidRequest(name: "Renamed Run", type: "Run"));

        Assert.IsType<OkObjectResult>(result);
        var changed = await context.Events.FindAsync(1);
        Assert.Equal("Renamed Run", changed!.Name);
    }

    [Fact]
    public async Task Update_AnotherOrganisersEvent_Returns403AndChangesNothing()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 2, role: "Organiser");

        var result = await controller.Update(1, ValidRequest(name: "Hijacked"));

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
        var unchanged = await context.Events.FindAsync(1);
        Assert.Equal("Test Run", unchanged!.Name);
    }

    [Fact]
    public async Task Delete_EventWithCategories_Returns409AndKeepsEvent()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Delete(1);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(1, await context.Events.CountAsync());
    }

    [Fact]
    public async Task Delete_OwnEventWithoutCategories_Returns204AndRemovesIt()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        context.Events.Add(new Event
        {
            Id = 2,
            OrganiserId = 1,
            VenueId = 1,
            Name = "Empty Event",
            EventDate = new DateTime(2026, 12, 5),
            EventType = "Run",
            DistanceKm = 5
        });
        await context.SaveChangesAsync();

        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 1, role: "Organiser");

        var result = await controller.Delete(2);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Events.FindAsync(2));
    }

    [Fact]
    public async Task Delete_AnotherOrganisersEvent_Returns403()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EventsController(context);
        TestHelpers.AttachSession(controller, userId: 2, role: "Organiser");

        var result = await controller.Delete(1);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
        Assert.Equal(1, await context.Events.CountAsync());
    }
}