using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Notifications.Templates.Notifications;

internal static class TemplateFormatter
{
    internal static string FormatTimeRange(DateTime start, DateTime end) =>
        $"{start:dd.MM.yyyy HH:mm} - {end:dd.MM.yyyy HH:mm}";

    internal static string FormatAudience(bool allowAllGroups, IEnumerable<string> allowedGroupNames)
    {
        allowedGroupNames = allowedGroupNames as ICollection<string> ?? allowedGroupNames.ToList();
        
        return allowAllGroups ? "Все группы"
            : !allowedGroupNames.Any() ? "Список групп не указан"
            : string.Join(", ", allowedGroupNames.OrderBy(static name => name, StringComparer.Ordinal));
    }

    internal static string FormatOptionalText(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    internal static string FormatReminderSchedule(SubmissionRemindTimeModel submissionRemindTime) =>
        submissionRemindTime switch
        {
            SubmissionRemindTimeModel.TwoWeeks => "за 2 недели",
            SubmissionRemindTimeModel.OneWeek => "за 1 неделю",
            SubmissionRemindTimeModel.TwoDays => "за 2 дня",
            SubmissionRemindTimeModel.FourHours => "за 4 часа",
            _ => submissionRemindTime.ToString()
        };
}
