namespace EFlow.Booking.WebApi.Contracts.Bookings;

public record UpdateBookingRequest
{
    public Guid? StudentId { get; init; }

    public Guid? SlotId { get; init; }
}
