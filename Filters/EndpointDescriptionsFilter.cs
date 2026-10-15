using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_proj.Filters;

// Gives every endpoint a clear purpose, expected input and list of responses in Swagger
public class EndpointDescriptionsFilter : IOperationFilter
{
    private static (string Summary, string Role, string Input, string[] Responses) D(
        string summary, string role, string input, params string[] responses)
        => (summary, role, input, responses);

    private static readonly Dictionary<string, (string Summary, string Role, string Input, string[] Responses)> Docs = new()
    {
        // ---- Auth ----
        ["POST api/auth/register"] = D("Register a new account", "Public",
            "JSON body: name, email, password (at least 6 characters) and role ('Organiser' or 'Participant').",
            "201|Account created. Returns the new id and email.", "400|Invalid role or invalid fields.", "409|That email is already registered."),
        ["POST api/auth/login"] = D("Log in and start a session", "Public",
            "JSON body: email and password.",
            "200|Logged in. Returns id, name, email and role, and starts a session.", "401|Invalid email or password."),
        ["POST api/auth/logout"] = D("Log out and end the session", "Public",
            "No input.", "200|Session cleared."),

        // ---- Profile ----
        ["GET api/users/me"] = D("View my own profile", "Any logged-in user",
            "No input.", "200|Returns id, name, email, role and created date."),
        ["PUT api/users/me"] = D("Update my own name and email", "Any logged-in user",
            "JSON body: name and email.",
            "200|Profile updated.", "400|Invalid name or email.", "409|Email already used by another account."),
        ["DELETE api/users/me"] = D("Delete my own account", "Any logged-in user",
            "No input.", "204|Account deleted.", "409|You still have events or enrolments."),
        ["GET api/users/me/enrolments"] = D("List my own enrolments", "Participant only",
            "No input.", "200|Array of my enrolments with category and event names."),
        ["GET api/users/me/results"] = D("List my own results", "Participant only",
            "No input.", "200|Array of my results with finish time and position."),

        // ---- Venues ----
        ["GET api/venues"] = D("List all venues", "Any logged-in user",
            "No input.", "200|Array of venues."),
        ["GET api/venues/{id}"] = D("Get one venue", "Any logged-in user",
            "Path: venue id.", "200|The venue.", "404|Venue not found."),
        ["POST api/venues"] = D("Create a venue", "Organiser only",
            "JSON body: name, address (optional) and city.", "201|Venue created.", "400|Missing or invalid fields."),
        ["PUT api/venues/{id}"] = D("Update a venue", "Organiser only",
            "Path: venue id. JSON body: name, address and city.", "200|Venue updated.", "404|Venue not found."),
        ["DELETE api/venues/{id}"] = D("Delete a venue", "Organiser only",
            "Path: venue id.", "204|Venue deleted.", "404|Venue not found.", "409|The venue is used by an event."),

        // ---- Events ----
        ["GET api/events"] = D("List all events", "Any logged-in user",
            "No input.", "200|Array of events with venue and organiser names."),
        ["GET api/events/{id}"] = D("Get one event", "Any logged-in user",
            "Path: event id.", "200|The event.", "404|Event not found."),
        ["POST api/events"] = D("Create an event", "Organiser only",
            "JSON body: name, description, eventDate, venueId, eventType ('Run', 'Walk' or 'Cycle') and distanceKm. The organiser is the logged-in user.",
            "201|Event created.", "400|Invalid eventType, unknown venue or invalid fields."),
        ["PUT api/events/{id}"] = D("Update my own event", "Organiser only",
            "Path: event id. JSON body: the same fields as create.",
            "200|Event updated.", "400|Invalid eventType, venue or fields.", "403|You can only change your own events.", "404|Event not found."),
        ["DELETE api/events/{id}"] = D("Delete my own event", "Organiser only",
            "Path: event id.",
            "204|Event deleted.", "403|You can only delete your own events.", "404|Event not found.", "409|Delete the event's categories first."),
        ["GET api/events/{eventId}/categories"] = D("List an event's categories", "Any logged-in user",
            "Path: event id.", "200|Array of categories.", "404|Event not found."),
        ["GET api/events/{eventId}/results"] = D("Leaderboard for an event", "Any logged-in user",
            "Path: event id.", "200|Results ordered by finishing position.", "404|Event not found."),

        // ---- Categories ----
        ["GET api/categories"] = D("List all categories", "Any logged-in user",
            "No input.", "200|Array of categories."),
        ["GET api/categories/{id}"] = D("Get one category", "Any logged-in user",
            "Path: category id.", "200|The category.", "404|Category not found."),
        ["POST api/categories"] = D("Add a category to my event", "Organiser only",
            "JSON body: eventId, name and distanceKm.",
            "201|Category created.", "403|You can only add categories to your own events.", "404|Event not found."),
        ["PUT api/categories/{id}"] = D("Update a category", "Organiser only",
            "Path: category id. JSON body: name and distanceKm.",
            "200|Category updated.", "403|You can only change categories of your own events.", "404|Category not found."),
        ["DELETE api/categories/{id}"] = D("Delete a category", "Organiser only",
            "Path: category id.",
            "204|Category deleted.", "403|You can only delete categories of your own events.", "404|Category not found.", "409|The category has enrolments."),

        // ---- Enrolments ----
        ["GET api/enrolments"] = D("List the enrolments for my events", "Organiser only",
            "No input.", "200|Array of enrolments with participant, category and event."),
        ["POST api/enrolments"] = D("Enrol in a category", "Participant only",
            "JSON body: categoryId.",
            "201|Enrolment created.", "404|Category not found.", "409|You are already enrolled in this category."),
        ["DELETE api/enrolments/{id}"] = D("Cancel an enrolment", "Any logged-in user",
            "Path: enrolment id. Participants can cancel their own; Organisers can cancel any.",
            "204|Enrolment cancelled.", "403|You can only cancel your own enrolments.", "404|Enrolment not found.", "409|The enrolment already has a result."),

        // ---- Results ----
        ["GET api/results"] = D("List all results", "Any logged-in user",
            "No input.", "200|Array of results."),
        ["GET api/results/{id}"] = D("Get one result", "Any logged-in user",
            "Path: result id.", "200|The result.", "404|Result not found."),
        ["POST api/results"] = D("Record a finish time and position", "Organiser only",
            "JSON body: enrolmentId, finishTime (HH:MM:SS, e.g. 00:52:30) and position.",
            "201|Result recorded.", "400|finishTime is not in HH:MM:SS format.", "403|You can only record results for your own events.",
            "404|Enrolment not found.", "409|A result already exists for this enrolment."),
        ["PUT api/results/{id}"] = D("Correct a result", "Organiser only",
            "Path: result id. JSON body: finishTime (HH:MM:SS) and position.",
            "200|Result updated.", "400|finishTime is not in HH:MM:SS format.", "403|You can only change results for your own events.", "404|Result not found."),
        ["DELETE api/results/{id}"] = D("Delete a result", "Organiser only",
            "Path: result id.",
            "204|Result deleted.", "403|You can only delete results for your own events.", "404|Result not found."),
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var key = $"{context.ApiDescription.HttpMethod} {context.ApiDescription.RelativePath}";
        if (!Docs.TryGetValue(key, out var doc))
            return;

        operation.Summary = doc.Summary;
        operation.Description = $"**Who can call it:** {doc.Role}\n\n**Input:** {doc.Input}";

        operation.Responses.Clear();
        foreach (var response in doc.Responses)
        {
            var parts = response.Split('|', 2);
            operation.Responses[parts[0]] = new OpenApiResponse { Description = parts[1] };
        }

        if (doc.Role != "Public" && !operation.Responses.ContainsKey("401"))
            operation.Responses["401"] = new OpenApiResponse { Description = "You must be logged in." };

        if (doc.Role.Contains("only") && !operation.Responses.ContainsKey("403"))
            operation.Responses["403"] = new OpenApiResponse { Description = "You do not have the right role for this." };
    }
}