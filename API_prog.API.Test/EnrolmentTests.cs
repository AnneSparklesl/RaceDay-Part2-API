using API_proj.Controllers;
using API_proj.DTOs;
using API_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_prog.API.Test;

public class EnrolmentTests
{
    [Fact]
    public async Task Enrol_ValidCategory_Returns201AndRecordsTheLink()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 3, role: "Participant");

        var result = await controller.Enrol(new EnrolmentRequest { CategoryId = 1 });

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);

        var saved = await context.Enrolments.SingleAsync();
        Assert.Equal(3, saved.ParticipantId);
        Assert.Equal(1, saved.CategoryId);
    }

    [Fact]
    public async Task Enrol_SameCategoryTwice_Returns409AndKeepsOneEnrolment()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 3, role: "Participant");

        await controller.Enrol(new EnrolmentRequest { CategoryId = 1 });
        var second = await controller.Enrol(new EnrolmentRequest { CategoryId = 1 });

        Assert.IsType<ConflictObjectResult>(second);
        Assert.Equal(1, await context.Enrolments.CountAsync());
    }

    [Fact]
    public async Task Enrol_UnknownCategory_Returns404AndSavesNothing()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 3, role: "Participant");

        var result = await controller.Enrol(new EnrolmentRequest { CategoryId = 99 });

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Empty(context.Enrolments);
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyTheLoggedInParticipantsEnrolments()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        context.Enrolments.AddRange(
            new Enrolment { Id = 1, ParticipantId = 3, CategoryId = 1 },
            new Enrolment { Id = 2, ParticipantId = 4, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 3, role: "Participant");

        var result = await controller.GetMine();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task Cancel_AnotherParticipantsEnrolment_Returns403AndKeepsIt()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        context.Enrolments.Add(new Enrolment { Id = 1, ParticipantId = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 4, role: "Participant");

        var result = await controller.Cancel(1);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
        Assert.Equal(1, await context.Enrolments.CountAsync());
    }

    [Fact]
    public async Task Cancel_OwnEnrolment_Returns204AndRemovesIt()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        context.Enrolments.Add(new Enrolment { Id = 1, ParticipantId = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);
        TestHelpers.AttachSession(controller, userId: 3, role: "Participant");

        var result = await controller.Cancel(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Enrolments);
    }
}
