namespace EFlow.Common.Clients.Booking.Options;

public sealed record BookingApiOptions
{
    public const string SectionName = "BookingApi";

    public required Uri BaseUrl { get; init; }
}
