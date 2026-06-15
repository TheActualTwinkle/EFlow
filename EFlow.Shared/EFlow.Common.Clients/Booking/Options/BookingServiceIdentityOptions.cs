namespace EFlow.Common.Clients.Booking.Options;

public sealed record BookingServiceIdentityOptions
{
    public required string Subject { get; set; }

    public required string Name { get; set; }
}
