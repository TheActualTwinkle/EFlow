using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Infrastructure.Assertions;

/// <summary>
/// Provides reusable assertions for HTTP problem details returned by the Booking API.
/// </summary>
internal static class ApiAssertions
{
    /// <summary>
    /// Verifies that the response contains the expected problem-details payload and status code.
    /// </summary>
    public static async Task AssertProblemAsync(
        this ApiSession session,
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string? titleContains = null,
        string? detailContains = null)
    {
        response.StatusCode.Should().Be(statusCode);

        using var json = await session.ReadJsonDocumentAsync(response);
        var root = json.RootElement;

        if (titleContains is not null)
            root.GetProperty("title").GetString().Should().Contain(titleContains);

        if (detailContains is not null)
            root.GetProperty("detail").GetString().Should().Contain(detailContains);
    }
}