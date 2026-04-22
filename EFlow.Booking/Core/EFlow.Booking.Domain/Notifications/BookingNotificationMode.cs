namespace EFlow.Booking.Domain.Notifications;

public enum BookingNotificationMode
{
    All = 0,
    OnlyCancellation = 1,
    OnlyNewBooking = 2,
    None = 3
}
