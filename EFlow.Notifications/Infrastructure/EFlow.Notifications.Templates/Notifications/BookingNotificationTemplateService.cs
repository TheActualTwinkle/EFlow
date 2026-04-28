using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Notifications.Templates.Models;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using EFlow.Notifications.Templates.Rendering.Interfaces;

namespace EFlow.Notifications.Templates.Notifications;

public sealed class BookingNotificationTemplateService(ITemplateRenderer templateRenderer)
    : IBookingNotificationTemplateService
{
    public async Task<(string Subject, string Body)> CreateBookingCreatedAsync(
        BookingRecordModel bookingRecord,
        CancellationToken cancellationToken = new())
    {
        var model = new BookingEmailTemplateModel
        {
            Title = "Новая запись на защиту",
            Lead = $"Студент {bookingRecord.StudentFullName} записался на защиту.",
            AccentLabel = "Запись подтверждена",
            SubjectName = bookingRecord.SubmissionSlotModel.SubjectName,
            TeacherName = bookingRecord.SubmissionSlotModel.TeacherFullName,
            StudentFullName = bookingRecord.StudentFullName,
            TimeRange = TemplateFormatter.FormatTimeRange(bookingRecord.SubmissionSlotModel.StartTime, bookingRecord.SubmissionSlotModel.EndTime),
            Location = TemplateFormatter.FormatOptionalText(bookingRecord.SubmissionSlotModel.Location, "Место не было указано"),
            Footer = "Проверьте список записей и при необходимости свяжитесь со студентом."
        };

        var body = await templateRenderer.RenderAsync("/EmailTemplates/BookingNotification.cshtml", model, cancellationToken);

        return ("Новая запись на защиту", body);
    }

    public async Task<(string Subject, string Body)> CreateBookingCancelledAsync(
        BookingRecordModel bookingRecord,
        CancellationToken cancellationToken = new())
    {
        var model = new BookingEmailTemplateModel
        {
            Title = "Отмена записи на защиту",
            Lead = $"Студент {bookingRecord.StudentFullName} отменил запись на слот защиты.",
            AccentLabel = "Запись отменена",
            SubjectName = bookingRecord.SubmissionSlotModel.SubjectName,
            TeacherName = bookingRecord.SubmissionSlotModel.TeacherFullName,
            StudentFullName = bookingRecord.StudentFullName,
            TimeRange = TemplateFormatter.FormatTimeRange(bookingRecord.SubmissionSlotModel.StartTime, bookingRecord.SubmissionSlotModel.EndTime),
            Location = TemplateFormatter.FormatOptionalText(bookingRecord.SubmissionSlotModel.Location, "Место не было указано"),
            Footer = "Если нужно, освободившееся место можно предложить другим студентам."
        };

        var body = await templateRenderer.RenderAsync("/EmailTemplates/BookingNotification.cshtml", model, cancellationToken);

        return ("Отмена записи на защиту", body);
    }

    public async Task<(string Subject, string Body)> CreateSubmissionSlotCreatedAsync(
        SubmissionSlotModel submissionSlot,
        CancellationToken cancellationToken = new())
    {
        var model = new SubmissionSlotCreatedEmailTemplateModel
        {
            Title = "Новый слот защиты",
            Lead = "В расписании появился новый слот, по которому включены уведомления.",
            SubjectName = submissionSlot.SubjectName,
            TeacherFullName = submissionSlot.TeacherFullName,
            TimeRange = TemplateFormatter.FormatTimeRange(submissionSlot.StartTime, submissionSlot.EndTime),
            Capacity = $"{submissionSlot.MaxStudents} чел.",
            Audience = TemplateFormatter.FormatAudience(submissionSlot.AllowAllGroups, submissionSlot.AllowedGroupNames),
            Location = TemplateFormatter.FormatOptionalText(submissionSlot.Location, "Место не было указано"),
            Comment = TemplateFormatter.FormatOptionalText(submissionSlot.Comment, "Комментарий отсутствует")
        };

        var body = await templateRenderer.RenderAsync("/EmailTemplates/SubmissionSlotCreated.cshtml", model, cancellationToken);

        return ($"Создан новый слот по предмету «{submissionSlot.SubjectName}»", body);
    }

    public async Task<(string Subject, string Body)> CreateSubmissionSlotUpdatedAsync(
        SubmissionSlotModel oldSubmissionSlot,
        SubmissionSlotModel newSubmissionSlot,
        CancellationToken cancellationToken = new())
    {
        var model = new SubmissionSlotUpdatedEmailTemplateModel
        {
            Title = "Слот защиты обновлён",
            Lead = "Ниже собраны изменения по слоту, который вы отслеживаете.",
            SubjectName = newSubmissionSlot.SubjectName,
            TeacherName = newSubmissionSlot.TeacherFullName,
            CurrentTimeRange = TemplateFormatter.FormatTimeRange(newSubmissionSlot.StartTime, newSubmissionSlot.EndTime),
            Changes = BuildChanges(oldSubmissionSlot, newSubmissionSlot)
        };
        
        var body = await templateRenderer.RenderAsync("/EmailTemplates/SubmissionSlotUpdated.cshtml", model, cancellationToken);

        return ($"Изменения в слоте по предмету «{newSubmissionSlot.SubjectName}»", body);
    }

    public async Task<(string Subject, string Body)> CreateReminderAsync(
        SubmissionSlotModel submissionSlot,
        SubmissionRemindTimeModel submissionRemindTime,
        CancellationToken cancellationToken = new())
    {
        var model = new ReminderEmailTemplateModel
        {
            Title = "Напоминание о защите",
            Lead = $"Скоро начнётся слот защиты. Напоминание: {TemplateFormatter.FormatReminderSchedule(submissionRemindTime)}.",
            SubjectName = submissionSlot.SubjectName,
            TeacherName = submissionSlot.TeacherFullName,
            TimeRange = TemplateFormatter.FormatTimeRange(submissionSlot.StartTime, submissionSlot.EndTime),
            Location = TemplateFormatter.FormatOptionalText(submissionSlot.Location, "Место не было указано"),
            Audience = TemplateFormatter.FormatAudience(submissionSlot.AllowAllGroups, submissionSlot.AllowedGroupNames)
        };

        var body = await templateRenderer.RenderAsync("/EmailTemplates/Reminder.cshtml", model, cancellationToken);

        return ($"Напоминание: скоро защита по предмету «{submissionSlot.SubjectName}»", body);
    }

    private static SubmissionSlotUpdatedEmailTemplateModel.ChangedItem[] BuildChanges(
        SubmissionSlotModel oldSlot,
        SubmissionSlotModel newSlot)
    {
        var changes = new List<SubmissionSlotUpdatedEmailTemplateModel.ChangedItem>();

        AddChangeIfNeeded(changes, "Предмет", oldSlot.SubjectName, newSlot.SubjectName);
        AddChangeIfNeeded(changes, "Преподаватель", oldSlot.TeacherFullName, newSlot.TeacherFullName);
        AddChangeIfNeeded(changes, "Время", TemplateFormatter.FormatTimeRange(oldSlot.StartTime, oldSlot.EndTime), TemplateFormatter.FormatTimeRange(newSlot.StartTime, newSlot.EndTime));
        AddChangeIfNeeded(changes, "Лимит студентов", oldSlot.MaxStudents.ToString(), newSlot.MaxStudents.ToString());
        AddChangeIfNeeded(changes, "Доступ", TemplateFormatter.FormatAudience(oldSlot.AllowAllGroups, oldSlot.AllowedGroupNames), TemplateFormatter.FormatAudience(newSlot.AllowAllGroups, newSlot.AllowedGroupNames));
        AddChangeIfNeeded(changes, "Место", TemplateFormatter.FormatOptionalText(oldSlot.Location, "Не указано"), TemplateFormatter.FormatOptionalText(newSlot.Location, "Не указано"));
        AddChangeIfNeeded(changes, "Комментарий", TemplateFormatter.FormatOptionalText(oldSlot.Comment, "Отсутствует"), TemplateFormatter.FormatOptionalText(newSlot.Comment, "Отсутствует"));

        return changes.ToArray();
    }

    private static void AddChangeIfNeeded(
        List<SubmissionSlotUpdatedEmailTemplateModel.ChangedItem> changes,
        string label,
        string oldValue,
        string newValue)
    {
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
            return;

        changes.Add(
            new SubmissionSlotUpdatedEmailTemplateModel.ChangedItem
            {
                Label = label,
                OldValue = oldValue,
                NewValue = newValue
            });
    }
}