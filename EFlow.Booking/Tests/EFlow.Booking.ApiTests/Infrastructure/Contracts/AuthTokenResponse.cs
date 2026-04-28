namespace EFlow.Booking.ApiTests.Infrastructure.Contracts;

/// <summary>
/// Represents the authentication token returned by successful login and registration calls.
/// </summary>
internal sealed record AuthTokenResponse
{
    /// <summary>
    /// Gets the issued JWT token value.
    /// </summary>
    public required string Token { get; init; }
}