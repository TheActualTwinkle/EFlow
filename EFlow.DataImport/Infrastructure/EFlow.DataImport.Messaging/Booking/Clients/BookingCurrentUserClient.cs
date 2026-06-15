using System.Text.Json;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Clients;

public sealed class BookingCurrentUserClient(HttpClient httpClient)
    : IBookingCurrentUserClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CurrentUserResult> GetCurrentUserAsync(
        string? authorizationHeader,
        string? cookieHeader,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/auth/me");

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
            httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        if (!string.IsNullOrWhiteSpace(cookieHeader))
            httpRequest.Headers.TryAddWithoutValidation("Cookie", cookieHeader);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new CurrentUserResult
            {
                StatusCode = response.StatusCode,
                Body = body
            };

        return new CurrentUserResult
        {
            StatusCode = response.StatusCode,
            Body = body,
            User = JsonSerializer.Deserialize<CurrentUserView>(body, JsonOptions)
        };
    }
}
