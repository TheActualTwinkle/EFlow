using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EFlow.DataImport.ApiTests.Infrastructure.Sessions;

public sealed class DataImportApiSession : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _bookingClient;
    private readonly HttpClient _dataImportClient;

    public DataImportApiSession(Uri bookingBaseUri, Uri dataImportBaseUri)
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true
        };
        var dataImportHandler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true
        };

        _bookingClient = new HttpClient(handler) { BaseAddress = bookingBaseUri };
        _dataImportClient = new HttpClient(dataImportHandler) { BaseAddress = dataImportBaseUri };
    }

    public void Dispose()
    {
        _bookingClient.Dispose();
        _dataImportClient.Dispose();
    }

    public Task<HttpResponseMessage> BookingGetAsync(string path) =>
        _bookingClient.GetAsync(path);

    public Task<HttpResponseMessage> BookingPostAsync<T>(string path, T body) =>
        _bookingClient.PostAsJsonAsync(path, body, SerializerOptions);

    public async Task<HttpResponseMessage> DataImportPostMultipartAsync(
        Guid groupId,
        string csv,
        IReadOnlyList<string> fields,
        bool hasHeaderRow = false)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "File", "students.csv");

        foreach (var field in fields)
            content.Add(new StringContent(field), "Fields");

        content.Add(new StringContent(hasHeaderRow.ToString()), "HasHeaderRow");

        return await _dataImportClient.PostAsync($"/api/csv/students?groupId={groupId}", content);
    }

    public async Task<HttpResponseMessage> AnonymousDataImportPostMultipartAsync(
        Guid groupId,
        string csv,
        IReadOnlyList<string> fields)
    {
        using var client = new HttpClient
        {
            BaseAddress = _dataImportClient.BaseAddress
        };
        using var content = new MultipartFormDataContent();

        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "File", "students.csv");

        foreach (var field in fields)
            content.Add(new StringContent(field), "Fields");

        return await client.PostAsync($"/api/csv/students?groupId={groupId}", content);
    }

    public async Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(SerializerOptions);

    public async Task<string> ReadTextAsync(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    public Guid GetCreatedId(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.ToString();

        if (string.IsNullOrWhiteSpace(location))
            throw new InvalidOperationException("Location header is missing.");

        return Guid.Parse(location.Split('/', StringSplitOptions.RemoveEmptyEntries).Last());
    }
}
