namespace EFlow.Booking.WebApi.Contracts.Auth;

public sealed record CurrentUserView
{
    public required Guid Id { get; init; }

    public required string? UserName { get; init; }

    public required string? Email { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }
}
