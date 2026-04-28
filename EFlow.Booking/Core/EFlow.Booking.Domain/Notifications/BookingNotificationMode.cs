namespace EFlow.Booking.Domain.Notifications;

public enum BookingNotificationMode
{
    None = 0,
    All = 1,
    OnlyCancellation = 2,
    OnlyNewBooking = 3
}
