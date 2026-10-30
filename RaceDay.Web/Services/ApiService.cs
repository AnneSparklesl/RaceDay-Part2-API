using System.Net;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services;

// What every API call returns to the controllers
public class ApiResult<T>
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class ApiService
{
    private const string CookieKey = "ApiCookie";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private ISession Session => _accessor.HttpContext!.Session;

    // Urls are relative to BaseUrl, so write "api/events" (no slash at the start)
    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(Build(HttpMethod.Get, url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) =>
        SendAsync<T>(Build(HttpMethod.Post, url, body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null) =>
        SendAsync<T>(Build(HttpMethod.Put, url, body));

    public Task<ApiResult<object>> DeleteAsync(string url) =>
        SendAsync<object>(Build(HttpMethod.Delete, url));

    // Forget the API's session cookie (used when the user logs out)
    public void ForgetApiCookie() => Session.Remove(CookieKey);

    private HttpRequestMessage Build(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);

        // Send this user's own API session cookie with the request
        var cookie = Session.GetString(CookieKey);
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.Add("Cookie", cookie);

        if (body != null)
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        return request;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request)
    {
        try
        {
            var response = await _http.SendAsync(request);
            SaveApiCookie(response);

            var text = await response.Content.ReadAsStringAsync();
            var result = new ApiResult<T>
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    result.Data = JsonSerializer.Deserialize<T>(text, JsonOptions);
            }
            else
            {
                result.Message = ReadMessage(text, response.StatusCode);
            }

            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Never show raw errors to the user
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Message = "We could not reach the RaceDay service. Please try again."
            };
        }
    }

    // The API sends its session id in a Set-Cookie header. Keep it in THIS user's web session.
    private void SaveApiCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return;

        var pairs = values.Select(v => v.Split(';')[0]);
        Session.SetString(CookieKey, string.Join("; ", pairs));
    }

    private static string ReadMessage(string text, HttpStatusCode status)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var message))
                return message.GetString() ?? "Something went wrong.";

            // Validation errors from the API: take the first one
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                foreach (var field in errors.EnumerateObject())
                    foreach (var error in field.Value.EnumerateArray())
                        return error.GetString() ?? "Some of the information is invalid.";
        }
        catch (JsonException) { }

        return status switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "We could not find what you were looking for.",
            _ => "Something went wrong. Please try again."
        };
    }
}