namespace EFlow.Booking.ApiTests.Infrastructure.Contracts;

internal sealed record UpdateUserEmailRequestModel
{
    public required string Email { get; init; }
}

internal sealed record UpdateUserPasswordRequestModel
{
    public required string CurrentPassword { get; init; }

    public required string NewPassword { get; init; }
}
