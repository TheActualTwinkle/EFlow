namespace EFlow.Booking.WebApi.Contracts.Bookings;

public record CreateBookingRequest
{
    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }
}
