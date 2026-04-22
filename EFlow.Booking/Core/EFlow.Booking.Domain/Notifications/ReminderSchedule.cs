namespace EFlow.Booking.Domain.Notifications;

[Flags]
public enum ReminderSchedule
{
    None = 0,
    TwoWeeks = 1,
    OneWeek = 2,
    TwoDays = 4,
    FourHours = 8
}
