using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Notifications.Application.BookingReminders;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Messaging.Booking.Interfaces;
using EFlow.Notifications.Messaging.Booking.Models;
using EFlow.Notifications.Messaging.Booking.Settings;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EFlow.Notifications.UnitTests.BookingReminders;

/// <summary>
/// Verifies booking reminder polling.
/// </summary>
public sealed class BookingReminderJobTests
{
    /// <summary>
    /// Provides reminder lead times that are expected to trigger inside the polling window.
    /// </summary>
    public static TheoryData<SubmissionRemindTimeModel, TimeSpan> SupportedRemindTimes =>
        new()
        {
            { SubmissionRemindTimeModel.TwoWeeks, TimeSpan.FromDays(14) },
            { SubmissionRemindTimeModel.OneWeek, TimeSpan.FromDays(7) },
            { SubmissionRemindTimeModel.TwoDays, TimeSpan.FromDays(2) },
            { SubmissionRemindTimeModel.FourHours, TimeSpan.FromHours(4) }
        };

    /// <summary>
    /// Verifies that <c>ProcessAsync</c> sends an email when the reminder falls inside the polling window.
    /// </summary>
    [Theory]
    [MemberData(nameof(SupportedRemindTimes))]
    public async Task ProcessAsync_WhenReminderFallsInsidePollWindow_ShouldSendEmail(
        SubmissionRemindTimeModel remindTime,
        TimeSpan leadTime)
    {
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var (job, bookingClientMock, templateServiceMock, emailServiceMock) = CreateSut(now);

        bookingClientMock
            .Setup(x => x.GetReminderSnapshotAsync(CancellationToken.None))
            .ReturnsAsync(
            [
                CreateSnapshot("student@example.com", remindTime, now + leadTime - TimeSpan.FromMinutes(5))
            ]);

        templateServiceMock
            .Setup(x => x.CreateReminderAsync(It.IsAny<SubmissionSlotModel>(), remindTime, CancellationToken.None))
            .ReturnsAsync(("Reminder", "Body"));

        await job.ProcessAsync(CancellationToken.None);

        templateServiceMock.Verify(
            x => x.CreateReminderAsync(
                It.Is<SubmissionSlotModel>(slot => slot.StartTime == now + leadTime - TimeSpan.FromMinutes(5)),
                remindTime,
                CancellationToken.None),
            Times.Once);

        emailServiceMock.Verify(
            x => x.SendAsync(
                It.Is<NotificationMessage>(message =>
                    message.RecipientEmail == "student@example.com" &&
                    message.Subject == "Reminder" &&
                    message.Body == "Body"),
                CancellationToken.None),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <c>ProcessAsync</c> does not send an email when the reminder is older than the polling window.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenReminderIsOlderThanPollWindow_ShouldNotSendEmail()
    {
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var (job, bookingClientMock, templateServiceMock, emailServiceMock) = CreateSut(now);

        bookingClientMock
            .Setup(x => x.GetReminderSnapshotAsync(CancellationToken.None))
            .ReturnsAsync(
            [
                CreateSnapshot(
                        "student@example.com",
                        SubmissionRemindTimeModel.TwoDays,
                        now + TimeSpan.FromDays(2) - TimeSpan.FromMinutes(15))
            ]);

        await job.ProcessAsync(CancellationToken.None);
        AssertReminderWasNotSent(templateServiceMock, emailServiceMock);
    }

    /// <summary>
    /// Verifies that <c>ProcessAsync</c> does not send an email when the reminder time is still in the future.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenReminderTimeIsStillInFuture_ShouldNotSendEmail()
    {
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var (job, bookingClientMock, templateServiceMock, emailServiceMock) = CreateSut(now);

        bookingClientMock
            .Setup(x => x.GetReminderSnapshotAsync(CancellationToken.None))
            .ReturnsAsync(
            [
                CreateSnapshot(
                        "student@example.com",
                        SubmissionRemindTimeModel.FourHours,
                        now + TimeSpan.FromHours(4) + TimeSpan.FromMinutes(1))
            ]);

        await job.ProcessAsync(CancellationToken.None);
        AssertReminderWasNotSent(templateServiceMock, emailServiceMock);
    }

    private static (BookingReminderJob Job, Mock<IBookingClient> BookingClient, Mock<IBookingNotificationTemplateService> TemplateService,
        Mock<IEmailNotificationService> EmailService) CreateSut(DateTime now)
    {
        var bookingClientMock = new Mock<IBookingClient>();
        var templateServiceMock = new Mock<IBookingNotificationTemplateService>();
        var emailServiceMock = new Mock<IEmailNotificationService>();
        var loggerMock = new Mock<ILogger<BookingReminderJob>>();

        var job = new BookingReminderJob(
            bookingClientMock.Object,
            templateServiceMock.Object,
            emailServiceMock.Object,
            new FakeSystemClock(now),
            Options.Create(
                new BookingReminderSettings
                {
                    BookingApiBaseUrl = "http://localhost",
                    PollInterval = TimeSpan.FromMinutes(15)
                }),
            loggerMock.Object);

        return (job, bookingClientMock, templateServiceMock, emailServiceMock);
    }

    private static void AssertReminderWasNotSent(
        Mock<IBookingNotificationTemplateService> templateServiceMock,
        Mock<IEmailNotificationService> emailServiceMock)
    {
        templateServiceMock.Verify(
            x => x.CreateReminderAsync(It.IsAny<SubmissionSlotModel>(), It.IsAny<SubmissionRemindTimeModel>(), CancellationToken.None),
            Times.Never);

        emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), CancellationToken.None),
            Times.Never);
    }

    private static SubmissionSlotReminderSnapshot CreateSnapshot(
        string email,
        SubmissionRemindTimeModel remindTime,
        DateTime slotStartTime) =>
        new()
        {
            SubmissionSlot = new SubmissionSlot
            {
                Id = Guid.NewGuid(),
                Teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "Ivan", LastName = "Petrov" },
                Subject = new Subject { Id = Guid.NewGuid(), Name = "Distributed Systems" },
                StartTime = slotStartTime,
                EndTime = slotStartTime.AddHours(2),
                BookingCount = 3,
                Location = "A-101",
                Comment = "Bring slides",
                MaxStudents = 5,
                AllowAllGroups = true,
                AllowedGroups = [],
            },
            Recipients =
            [
                new SubmissionSlotReminderRecipient
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    RemindTime = remindTime
                }
            ]
        };
}
