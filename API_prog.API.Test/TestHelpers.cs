using API_proj.data;
using API_proj.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_prog.API.Test;

// A pretend session so controllers can be tested without a web server
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public string Id { get; } = Guid.NewGuid().ToString();
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
}

public static class TestHelpers
{
    // A brand-new empty in-memory database for every test
    public static ApplicationDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDBContext(options);
    }

    // Gives a controller a fake HTTP context.
    // Pass userId and role to pretend that person is logged in.
    public static TestSession AttachSession(ControllerBase controller, int? userId = null, string? role = null)
    {
        var session = new TestSession();

        if (userId != null)
            session.SetInt32("UserId", userId.Value);
        if (role != null)
            session.SetString("Role", role);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = session }
        };

        return session;
    }

    // Fills the fake database with a few people and one event:
    //   users 1 and 2 are Organisers, users 3 and 4 are Participants
    //   venue 1, event 1 (owned by user 1), category 1 (belongs to event 1)
    public static void SeedBasics(ApplicationDBContext context)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword("Password123", 4);

        context.Users.AddRange(
            new User { Id = 1, Name = "Organiser One", Email = "org1@test.com", PasswordHash = hash, Role = "Organiser" },
            new User { Id = 2, Name = "Organiser Two", Email = "org2@test.com", PasswordHash = hash, Role = "Organiser" },
            new User { Id = 3, Name = "Participant One", Email = "part1@test.com", PasswordHash = hash, Role = "Participant" },
            new User { Id = 4, Name = "Participant Two", Email = "part2@test.com", PasswordHash = hash, Role = "Participant" });

        context.Venues.Add(new Venue { Id = 1, Name = "City Park", City = "Johannesburg" });

        context.Events.Add(new Event
        {
            Id = 1,
            OrganiserId = 1,
            VenueId = 1,
            Name = "Test Run",
            EventDate = new DateTime(2026, 11, 15),
            EventType = "Run",
            DistanceKm = 10
        });

        context.Categories.Add(new Category { Id = 1, EventId = 1, Name = "10km Open", DistanceKm = 10 });

        context.SaveChanges();
    }
}