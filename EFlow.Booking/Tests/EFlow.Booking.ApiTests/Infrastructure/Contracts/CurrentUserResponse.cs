namespace EFlow.Booking.ApiTests.Infrastructure.Contracts;

/// <summary>
/// Represents the current authenticated user payload returned by the Booking API.
/// </summary>
internal sealed record CurrentUserResponse
{
    /// <summary>
    /// Gets the unique user identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the username assigned to the authenticated user.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Gets the email address of the authenticated user when it is available.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the role names granted to the authenticated user.
    /// </summary>
    public required string[] Roles { get; init; }
}