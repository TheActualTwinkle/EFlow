using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using FluentAssertions;
using System.Text.Json;

namespace EFlow.Booking.ApiTests.Infrastructure.Assertions;

/// <summary>
/// Provides reusable assertions for HTTP problem details returned by the Booking API.
/// </summary>
internal static class ApiAssertions
{
    public const string ForbiddenTitle = "Forbidden";
    public const string ValidationErrorTitle = "Validation Error";

    extension(ApiSession session)
    {
        /// <summary>
        /// Verifies that the response contains the expected problem-details payload and status code.
        /// </summary>
        public async Task AssertProblemAsync(
            HttpResponseMessage response,
            HttpStatusCode statusCode,
            string? titleContains = null,
            string? detailContains = null,
            string? code = null)
        {
            response.StatusCode.Should().Be(statusCode);

            using var json = await session.ReadJsonDocumentAsync(response);
            var root = json.RootElement;

            if (titleContains is not null)
                root.GetProperty("title").GetString().Should().Contain(titleContains);

            if (detailContains is not null)
                root.GetProperty("detail").GetString().Should().Contain(detailContains);

            if (code is not null)
                root.GetProperty("code").GetString().Should().Be(code);
        }

        /// <summary>
        /// Verifies that the response contains validation problem details for the supplied error key.
        /// </summary>
        public async Task AssertValidationProblemAsync(
            HttpResponseMessage response,
            HttpStatusCode statusCode,
            string errorKey,
            string? errorContains = null)
        {
            response.StatusCode.Should().Be(statusCode);

            using var json = await session.ReadJsonDocumentAsync(response);
            var root = json.RootElement;

            root.GetProperty("title").GetString()?.ToLowerInvariant().Should().Contain("validation");
            root.GetProperty("errors").TryGetProperty(errorKey, out var errorValues).Should().BeTrue();
            errorValues.ValueKind.Should().Be(JsonValueKind.Array);

            if (errorContains is not null)
                errorValues
                    .EnumerateArray()
                    .Select(x => x.GetString())
                    .Should()
                    .Contain(x => x != null && x.Contains(errorContains, StringComparison.OrdinalIgnoreCase));
        }
    }
}
