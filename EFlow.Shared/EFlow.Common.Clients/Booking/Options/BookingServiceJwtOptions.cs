namespace EFlow.Common.Clients.Booking.Options;

public sealed record BookingServiceJwtOptions
{
    public required string Key { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public int ExpireMinutes { get; init; } = 5;
}
