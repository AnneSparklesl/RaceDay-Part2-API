using API_proj.Controllers;
using API_proj.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_prog.API.Test;

public class AuthTests
{
    [Fact]
    public async Task Register_ValidOrganiser_Returns201AndStoresHashedPassword()
    {
        using var context = TestHelpers.CreateContext();
        var controller = new AuthController(context);
        TestHelpers.AttachSession(controller);

        var result = await controller.Register(new RegisterRequest
        {
            Name = "Olivia",
            Email = "olivia@test.com",
            Password = "Password123",
            Role = "Organiser"
        });

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);

        var saved = await context.Users.SingleAsync();
        Assert.Equal("Organiser", saved.Role);
        Assert.NotEqual("Password123", saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", saved.PasswordHash));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new AuthController(context);
        TestHelpers.AttachSession(controller);

        var result = await controller.Register(new RegisterRequest
        {
            Name = "Copycat",
            Email = "org1@test.com",
            Password = "Password123",
            Role = "Participant"
        });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(4, await context.Users.CountAsync());
    }

    [Fact]
    public async Task Register_InvalidRole_Returns400AndSavesNothing()
    {
        using var context = TestHelpers.CreateContext();
        var controller = new AuthController(context);
        TestHelpers.AttachSession(controller);

        var result = await controller.Register(new RegisterRequest
        {
            Name = "Bad Role",
            Email = "bad@test.com",
            Password = "Password123",
            Role = "Admin"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(context.Users);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200AndStoresUserIdAndRoleInSession()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new AuthController(context);
        var session = TestHelpers.AttachSession(controller);

        var result = await controller.Login(new LoginRequest
        {
            Email = "org1@test.com",
            Password = "Password123"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, session.GetInt32("UserId"));
        Assert.Equal("Organiser", session.GetString("Role"));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401AndCreatesNoSession()
    {
        using var context = TestHelpers.CreateContext();
        TestHelpers.SeedBasics(context);
        var controller = new AuthController(context);
        var session = TestHelpers.AttachSession(controller);

        var result = await controller.Login(new LoginRequest
        {
            Email = "org1@test.com",
            Password = "wrong"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Null(session.GetInt32("UserId"));
    }
}