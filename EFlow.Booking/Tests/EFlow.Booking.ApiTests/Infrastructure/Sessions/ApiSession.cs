using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EFlow.Booking.ApiTests.Infrastructure.Sessions;

/// <summary>
/// Wraps <see cref="HttpClient" /> with JSON helpers and isolated cookies for API tests.
/// </summary>
public sealed class ApiSession : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    /// <summary>
    /// Creates a new API session bound to the supplied base URI.
    /// </summary>
    public ApiSession(Uri baseUri)
    {
        var cookies = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = baseUri
        };
    }

    /// <summary>
    /// Releases the underlying HTTP client resources.
    /// </summary>
    public void Dispose() =>
        _client.Dispose();

    /// <summary>
    /// Sends a GET request to the specified relative path.
    /// </summary>
    public Task<HttpResponseMessage> GetAsync(string path) =>
        _client.GetAsync(path);

    /// <summary>
    /// Sends a DELETE request to the specified relative path.
    /// </summary>
    public Task<HttpResponseMessage> DeleteAsync(string path) =>
        _client.DeleteAsync(path);

    /// <summary>
    /// Sends a JSON POST request to the specified relative path.
    /// </summary>
    public Task<HttpResponseMessage> PostAsync<T>(string path, T body) =>
        _client.PostAsJsonAsync(path, body, SerializerOptions);

    /// <summary>
    /// Sends a JSON PATCH request to the specified relative path.
    /// </summary>
    public Task<HttpResponseMessage> PatchAsync<T>(string path, T body) =>
        _client.PatchAsJsonAsync(path, body, SerializerOptions);

    /// <summary>
    /// Sends a JSON PUT request to the specified relative path.
    /// </summary>
    public Task<HttpResponseMessage> PutAsync<T>(string path, T body) =>
        _client.PutAsJsonAsync(path, body, SerializerOptions);

    /// <summary>
    /// Deserializes the response body into the requested model type.
    /// </summary>
    public async Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(SerializerOptions);

    /// <summary>
    /// Reads the response body as plain text.
    /// </summary>
    public async Task<string> ReadTextAsync(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    /// <summary>
    /// Parses the response body into a JSON document, substituting an empty object for blank content.
    /// </summary>
    public async Task<JsonDocument> ReadJsonDocumentAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
    }

    /// <summary>
    /// Extracts the created resource identifier from the response location header.
    /// </summary>
    public Guid GetCreatedId(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.ToString();

        if (string.IsNullOrWhiteSpace(location))
            throw new InvalidOperationException("Location header is missing.");

        var lastSegment = location.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

        return Guid.Parse(lastSegment);
    }
}