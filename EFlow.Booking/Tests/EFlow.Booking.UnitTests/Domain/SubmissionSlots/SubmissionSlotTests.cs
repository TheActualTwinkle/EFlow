using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using FluentAssertions;

namespace EFlow.Booking.UnitTests.Domain.SubmissionSlots;

public class SubmissionSlotTests
{
    [Fact]
    public void CancelBooking_WhenStudentHasNotificationSettings_ShouldRemoveOnlyThatStudentsSettings()
    {
        // Arrange
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var groupId = new GroupId(Guid.CreateVersion7());
        var studentId = new StudentId(Guid.CreateVersion7());
        var otherUserId = Guid.CreateVersion7();
        var slot = SubmissionSlot.Create(
            new SubjectId(Guid.CreateVersion7()),
            new TeacherId(Guid.CreateVersion7()),
            new DateTime(2026, 05, 12, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 05, 12, 12, 0, 0, DateTimeKind.Utc),
            5,
            true,
            now);
        
        var student = Student.Create(
            studentId,
            groupId,
            "Ivan",
            "Petrov",
            null,
            new DateOnly(2005, 05, 01),
            now,
            now);

        slot.AddAdmission(studentId, now);
        slot.UpdateNotificationSettings(studentId.Value, [], [SubmissionRemindTime.OneWeek], BookingNotificationMode.All);
        slot.UpdateNotificationSettings(otherUserId, [], [SubmissionRemindTime.TwoDays], BookingNotificationMode.OnlyCancellation);
        var booking = slot.BookToSlot(student, Array.Empty<BookingRecord>(), now);

        // Act
        slot.CancelBooking(booking, now.AddMinutes(5));

        // Assert
        var recipients = slot.GetNotificationRecipients();
        recipients.Should().ContainSingle();
        recipients.Should().ContainSingle(recipient =>
            recipient.UserId == otherUserId &&
            recipient.BookingNotificationMode == BookingNotificationMode.OnlyCancellation);
        recipients.Should().NotContain(recipient => recipient.UserId == studentId.Value);
    }
}
